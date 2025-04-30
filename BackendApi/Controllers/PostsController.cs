using BackendApi.Data;
using BackendApi.Models;
using BackendApi.Services; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; 
using System;
using System.IO; 
using System.Globalization; 

namespace BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService; 
        private readonly OpenAiService _openAiService; 

        public PostsController(ApplicationDbContext context, BlobStorageService blobStorageService, OpenAiService openAiService)
        {
            _context = context;
            _blobStorageService = blobStorageService; 
            _openAiService = openAiService; 
        }

        // GET: api/posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            return await _context.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // POST: api/posts
        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost([FromForm] CreatePostDto dto, IFormFile? imageFile)
        {
            // --- Manual Date Parsing Removed ---
            // Rely on model binding to parse dto.DatePublished

            if (!ModelState.IsValid)
            {
                // If model binding failed for DatePublished or other fields, return BadRequest
                return BadRequest(ModelState);
            }

            // Create the Post entity and map from DTO
            var post = new Post
            {
                Title = dto.Title,
                // Directly use the DateTime parsed by the model binder
                DatePublished = dto.DatePublished.ToUniversalTime(), // Store as UTC
                Timeline = dto.Timeline,
                Story = dto.Story,
                WorkCited = dto.WorkCited,
                CreatedAt = DateTime.UtcNow // Set creation time server-side
            };

            // Handle image upload if a file is provided
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    using var stream = imageFile.OpenReadStream();
                    var imageUrl = await _blobStorageService.UploadFileAsync(stream, uniqueFileName, imageFile.ContentType);
                    post.ImageURL = imageUrl;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading image: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading image.");
                }
            }
            else
            {
                post.ImageURL = null; 
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Return 201 Created with the location of the new resource
            return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
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

            var imagePrompt = $"Generate an image depicting the following historical scene: {request.CurrentText}";
            var imageUrl = await _openAiService.GenerateImageAsync(imagePrompt);

            if (imageUrl == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate image suggestion from AI service.");
            }

            return Ok(imageUrl.ToString());
        }
    }

    public class SuggestionRequest
    {
        public string CurrentText { get; set; } = string.Empty;
    }
}
