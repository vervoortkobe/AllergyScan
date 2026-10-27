using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Storage;

namespace AllergyScan.Services
{
    public interface IPhotoService
    {
        Task<PickedPhoto?> CaptureAsync();
        Task<PickedPhoto?> PickAsync();
    }

    public class PickedPhoto
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "image/jpeg";
    }

    public class PhotoService : IPhotoService
    {
        public async Task<PickedPhoto?> CaptureAsync()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var file = await MediaPicker.Default.CapturePhotoAsync();
                if (file is not null)
                {
                    return await LoadPhotoAsync(file);
                }
            }
            return null;
        }

        public async Task<PickedPhoto?> PickAsync()
        {
            try
            {
                var files = await MediaPicker.Default.PickPhotosAsync();
                var file = files?.FirstOrDefault();
                if (file is not null)
                {
                    return await LoadPhotoAsync(file);
                }
            }
            catch (Exception)
            {
                // Fallback for permissions or unsupported platforms
                throw;
            }
            return null;
        }

        private static async Task<PickedPhoto> LoadPhotoAsync(FileResult file)
        {
            using var stream = await file.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? ".jpg";
            var contentType = GetContentType(extension);

            return new PickedPhoto
            {
                Data = memoryStream.ToArray(),
                FileName = file.FileName,
                ContentType = contentType
            };
        }

        private static string GetContentType(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "image/jpeg"
            };
        }
    }
}
