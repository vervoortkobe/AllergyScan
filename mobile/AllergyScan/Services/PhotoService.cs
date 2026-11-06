namespace AllergyScan.Services
{
    /// <summary>Provides camera capture and gallery picking of photos.</summary>
    public interface IPhotoService
    {
        /// <summary>Captures a photo using the device camera.</summary>
        Task<PickedPhoto?> CaptureAsync();

        /// <summary>Picks a photo from the device gallery.</summary>
        Task<PickedPhoto?> PickAsync();
    }

    /// <summary>Represents a photo picked from the camera or gallery.</summary>
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
            var files = await MediaPicker.Default.PickPhotosAsync();
            var file = files?.FirstOrDefault();
            if (file is not null)
            {
                return await LoadPhotoAsync(file);
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
