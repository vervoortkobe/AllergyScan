using AllergyScan.BLL.DTO;
using AllergyScan.BLL.Exceptions;
using AllergyScan.BLL.Interfaces;
using AllergyScan.BLL.Options;
using AllergyScan.DAL;
using AllergyScan.DAL.UOW;
using Microsoft.Extensions.Options;

namespace AllergyScan.BLL.Services
{
    public class ScanService : IScanService
    {
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/png",
            "image/jpeg",
            "image/jpg",
            "image/webp"
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ScanOptions> _scanOptions;

        public ScanService(IUnitOfWork unitOfWork, IOptions<ScanOptions> scanOptions)
        {
            _unitOfWork = unitOfWork;
            _scanOptions = scanOptions;
        }

        public async Task<ScanHistoryDTO> CreateAsync(CreateScanRequest request, CancellationToken cancellationToken = default)
        {
            var text = (request.Text ?? string.Empty).Trim();
            EnsureTextIsValid(text);

            byte[]? imageBytes = null;
            string? imageContentType = null;
            string? imageFileName = null;

            if (request.Image is not null)
            {
                (imageBytes, imageContentType) = await ReadImageAsync(
                    request.Image,
                    request.ImageContentType,
                    request.ImageFileName,
                    cancellationToken);

                imageFileName = SanitizeFileName(request.ImageFileName);
            }

            var scan = new ScanHistory
            {
                Image = imageBytes,
                ImageContentType = imageContentType,
                ImageFileName = imageFileName,
                Text = text,
                Flag = request.Flag,
                ScannedAt = DateTime.UtcNow
            };

            _unitOfWork.Scans.Create(scan);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Map(scan);
        }

        public async Task<PagedResult<ScanHistoryDTO>> GetHistoryAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            page = Math.Max(1, page);
            pageSize = pageSize <= 0
                ? _scanOptions.Value.DefaultPageSize
                : Math.Min(pageSize, _scanOptions.Value.MaxPageSize);

            var (items, total) = await _unitOfWork.Scans.GetPageAsync(page, pageSize, cancellationToken);
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            return new PagedResult<ScanHistoryDTO>(
                items.Select(Map).ToList(),
                page,
                pageSize,
                total,
                totalPages);
        }

        public async Task<ScanHistoryDTO> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var scan = await _unitOfWork.Scans.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Scan with id {id} was not found.");

            return Map(scan);
        }

        public async Task<(byte[] Data, string ContentType, string FileName)> GetImageAsync(int id, CancellationToken cancellationToken = default)
        {
            var scan = await _unitOfWork.Scans.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Scan with id {id} was not found.");

            if (scan.Image is null || scan.Image.Length == 0)
            {
                throw new NotFoundException($"Scan with id {id} has no image.");
            }

            var contentType = string.IsNullOrWhiteSpace(scan.ImageContentType)
                ? "application/octet-stream"
                : scan.ImageContentType;

            var fileName = string.IsNullOrWhiteSpace(scan.ImageFileName)
                ? $"scan-{id}{GetExtension(contentType)}"
                : scan.ImageFileName;

            return (scan.Image, contentType, fileName);
        }

        public async Task<ScanHistoryDTO> UpdateAsync(int id, UpdateScanRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Text is null && request.Flag is null)
            {
                throw new BadRequestException("Provide text or a flag to update.");
            }

            var scan = await _unitOfWork.Scans.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Scan with id {id} was not found.");

            if (request.Text is not null)
            {
                var text = request.Text.Trim();
                EnsureTextIsValid(text);
                scan.Text = text;
            }

            if (request.Flag is not null)
            {
                scan.Flag = request.Flag.Value;
            }

            _unitOfWork.Scans.Update(scan);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Map(scan);
        }

        public async Task<ScanHistoryDTO> UpdateImageAsync(int id, Stream image, string? contentType, string? fileName, CancellationToken cancellationToken = default)
        {
            var scan = await _unitOfWork.Scans.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Scan with id {id} was not found.");

            var (imageBytes, normalizedContentType) = await ReadImageAsync(image, contentType, fileName, cancellationToken);

            scan.Image = imageBytes;
            scan.ImageContentType = normalizedContentType;
            scan.ImageFileName = SanitizeFileName(fileName);

            _unitOfWork.Scans.Update(scan);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Map(scan);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var scan = await _unitOfWork.Scans.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Scan with id {id} was not found.");

            _unitOfWork.Scans.Delete(scan);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            return _unitOfWork.Scans.DeleteAllAsync(cancellationToken);
        }

        private void EnsureTextIsValid(string text)
        {
            if (text.Length == 0)
            {
                throw new BadRequestException("The ingredient text cannot be empty.");
            }

            if (text.Length > _scanOptions.Value.MaxTextLength)
            {
                throw new BadRequestException($"The text is too long (max {_scanOptions.Value.MaxTextLength} characters).");
            }
        }

        private async Task<(byte[] Data, string ContentType)> ReadImageAsync(
            Stream image,
            string? contentType,
            string? fileName,
            CancellationToken cancellationToken)
        {
            var normalizedContentType = NormalizeContentType(contentType, fileName);

            if (normalizedContentType is null || !AllowedImageContentTypes.Contains(normalizedContentType))
            {
                throw new BadRequestException(
                    "Unsupported image type. Use PNG, JPEG, WEBP, BMP, TIFF or GIF.");
            }

            using var memoryStream = new MemoryStream();
            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await image.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                if (memoryStream.Length + bytesRead > _scanOptions.Value.MaxImageBytes)
                {
                    throw new BadRequestException(
                        $"The image is too large (max {_scanOptions.Value.MaxImageBytes / (1024 * 1024)} MB).");
                }

                memoryStream.Write(buffer, 0, bytesRead);
            }

            if (memoryStream.Length == 0)
            {
                throw new BadRequestException("The uploaded image is empty.");
            }

            return (memoryStream.ToArray(), normalizedContentType);
        }

        private static string? NormalizeContentType(string? contentType, string? fileName)
        {
            if (!string.IsNullOrWhiteSpace(contentType) && !contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                return contentType.Split(';')[0].Trim().ToLowerInvariant();
            }

            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

            return extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".tif" or ".tiff" => "image/tiff",
                ".gif" => "image/gif",
                _ => null
            };
        }

        private static string? SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var name = Path.GetFileName(fileName.Trim().Replace('\\', '/'));
            return name.Length > 260 ? name[..260] : name;
        }

        private static string GetExtension(string contentType)
        {
            return contentType.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/webp" => ".webp",
                "image/bmp" => ".bmp",
                "image/tiff" => ".tiff",
                "image/gif" => ".gif",
                _ => ".bin"
            };
        }

        private static ScanHistoryDTO Map(ScanHistory scan)
        {
            var hasImage = scan.Image is not null || !string.IsNullOrWhiteSpace(scan.ImageContentType);

            // SQLite stores DateTime as text and loses the Kind, so restore UTC for the API.
            var scannedAt = DateTime.SpecifyKind(scan.ScannedAt, DateTimeKind.Utc);

            return new ScanHistoryDTO(
                scan.Id,
                scan.Text,
                scan.Flag,
                scannedAt,
                hasImage,
                hasImage ? $"/api/scans/{scan.Id}/image" : null);
        }
    }
}
