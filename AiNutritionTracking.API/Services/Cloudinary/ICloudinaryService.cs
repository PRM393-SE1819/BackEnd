using Microsoft.AspNetCore.Http;

namespace AiNutritionTracking.API.Services.Cloudinary
{
    public interface ICloudinaryService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string publicId);
    }
}