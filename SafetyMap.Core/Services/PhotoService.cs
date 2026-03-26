using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using SafetyMap.Core.Contracts;

namespace SafetyMap.Core.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> AddPhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            try 
            {
                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                    Folder = "safetymap-reports"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    return null;

                return uploadResult.SecureUrl?.ToString();
            }
            catch
            {
                // Return null if upload fails (e.g. invalid credentials)
                return null;
            }
        }

        public async Task<bool> DeletePhotoAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            // Extract the public ID from the Cloudinary URL
            // URL format: https://res.cloudinary.com/{cloud}/image/upload/v{version}/{folder}/{publicId}.{ext}
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');

                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex < 0 || uploadIndex + 2 >= segments.Length)
                    return false;

                var relevantParts = segments.Skip(uploadIndex + 2).ToArray();
                var publicIdWithExt = string.Join("/", relevantParts);
                var publicId = System.IO.Path.ChangeExtension(publicIdWithExt, null);

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }
    }
}
