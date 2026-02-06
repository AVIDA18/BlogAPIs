using BlogApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IFileService _fileService;

        public ImageController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var imageUrl = await _fileService.SaveImageAsync(file);
                return Ok(new { imageUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files)
        {
            try
            {
                var imageUrls = await _fileService.SaveImagesAsync(files);
                return Ok(new { imageUrls });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
