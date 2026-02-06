namespace BlogApi.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadFolder = "uploads/blog-images";

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only images are allowed.");

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
                throw new ArgumentException("File size exceeds 5MB limit");

            var uploadsPath = Path.Combine(_environment.WebRootPath, _uploadFolder);
            Directory.CreateDirectory(uploadsPath);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{_uploadFolder}/{uniqueFileName}";
        }

        public async Task<List<string>> SaveImagesAsync(List<IFormFile> files)
        {
            var imageUrls = new List<string>();
            foreach (var file in files)
            {
                var url = await SaveImageAsync(file);
                imageUrls.Add(url);
            }
            return imageUrls;
        }

        public Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return Task.CompletedTask;

            var fileName = imageUrl.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public async Task DeleteImagesAsync(List<string> imageUrls)
        {
            foreach (var url in imageUrls)
            {
                await DeleteImageAsync(url);
            }
        }
    }
}
