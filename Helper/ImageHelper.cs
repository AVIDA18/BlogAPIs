using BlogApi.Services;

namespace BlogApis.Helper
{
    public class ImageHelper
    {
        private readonly IFileService _fileService;

        public ImageHelper(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return new List<string>();

            try
            {
                var imageUrls = await _fileService.SaveImagesAsync(files);
                return imageUrls;
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"Image upload failed: {ex.Message}");
            }
        }
    }
}