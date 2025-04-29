using BackendApi.Data;
using BackendApi.Models;
using BackendApi.Services; // Added
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Added for IFormFile
using System; // Added for Guid
using System.IO; // Added for Path

namespace BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService; // Added
        private readonly OpenAiService _openAiService; // Added

        // Inject all services
        public PostsController(ApplicationDbContext context, BlobStorageService blobStorageService, OpenAiService openAiService)
        {
            _context = context;
            _blobStorageService = blobStorageService; // Added
            _openAiService = openAiService; // Added
        }

        // POST: api/posts
        // Handles the submission of a new post with an optional image
        [HttpPost]
        // Use [FromForm] to accept multipart/form-data
        public async Task<ActionResult<Post>> CreatePost([FromForm] Post post, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Handle image upload if a file is provided
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    // Generate a unique file name to prevent collisions
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    // Upload the file using the BlobStorageService
                    using var stream = imageFile.OpenReadStream();
                    var imageUrl = await _blobStorageService.UploadFileAsync(stream, uniqueFileName, imageFile.ContentType);

                    // Set the ImageURL property on the post object
                    post.ImageURL = imageUrl;
                }
                catch (Exception ex)
                {
                    // Log the exception (using a proper logging framework is recommended)
                    Console.WriteLine($"Error uploading image: {ex.Message}");
                    // Return an error response to the client
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading image.");
                }
            }
            else
            {
                post.ImageURL = null; // Ensure ImageURL is null if no file is uploaded
            }

            // Set the CreatedAt timestamp before saving
            post.CreatedAt = DateTime.UtcNow;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // POST: api/posts/suggest-story
        [HttpPost("suggest-story")]
        public async Task<ActionResult<string>> SuggestStory([FromBody] SuggestionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CurrentText))
            {
                return BadRequest("Current story text is required.");
            }

            var suggestion = await _openAiService.GetStorySuggestionAsync(request.CurrentText);

            if (suggestion == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get suggestion from AI service.");
            }

            return Ok(suggestion);
        }

        // POST: api/posts/suggest-images
        [HttpPost("suggest-images")]
        public async Task<ActionResult<string>> SuggestImages([FromBody] SuggestionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CurrentText))
            {
                return BadRequest("Story text is required to generate image suggestions.");
            }

            // Create a prompt for DALL-E based on the story text
            // You might want to refine this prompt for better results
            var imagePrompt = $"Generate an image depicting the following historical scene: {request.CurrentText}";

            var imageUrl = await _openAiService.GenerateImageAsync(imagePrompt);

            if (imageUrl == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate image suggestion from AI service.");
            }

            // Return the URL of the generated image
            return Ok(imageUrl.ToString());
        }

        // TODO: Add endpoints for:
        // - GET /api/posts/{id} (to retrieve a specific post - needed for CreatedAtAction)
        // - GET /api/posts (to retrieve all posts)
        // - PUT /api/posts/{id} (to update a post)
        // - DELETE /api/posts/{id} (to delete a post)
        // - POST /api/posts/suggest-image (for AI image suggestions)
        // - POST /api/posts/upload-image (for handling image uploads)
    }

    // Simple DTO (Data Transfer Object) for the suggestion request body
    public class SuggestionRequest
    {
        public string CurrentText { get; set; } = string.Empty;
    }
}
