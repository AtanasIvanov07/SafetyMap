using Microsoft.AspNetCore.Http;

namespace SafetyMap.Core.Contracts
{
    public interface IPhotoService
    {
        Task<string?> AddPhotoAsync(IFormFile file);
        Task<bool> DeletePhotoAsync(string imageUrl);
    }
}
