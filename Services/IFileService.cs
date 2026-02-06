namespace BlogApi.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file);
        Task<List<string>> SaveImagesAsync(List<IFormFile> files);
        Task DeleteImageAsync(string imageUrl);
        Task DeleteImagesAsync(List<string> imageUrls);
    }
}
