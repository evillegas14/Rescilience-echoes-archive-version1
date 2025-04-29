using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendApi.Services
{
    public class LocalFileStorageService : BlobStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadDirectory;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger) 
            : base(configuration)
        {
            _environment = environment;
            _logger = logger;
            
            // Create a directory for local file storage within the web root
            _uploadDirectory = Path.Combine(_environment.ContentRootPath, "LocalUploads");
            
            // Ensure the upload directory exists
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
                _logger.LogInformation("Created local uploads directory: {Directory}", _uploadDirectory);
            }
        }

        public override async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            // Generate a unique file path to prevent collisions
            string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string filePath = Path.Combine(_uploadDirectory, uniqueFileName);

            _logger.LogInformation("Saving file locally to: {FilePath}", filePath);

            try
            {
                // Create a file stream to write the uploaded file to disk
                using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
                {
                    // Copy the uploaded file to the file stream
                    await fileStream.CopyToAsync(fileStreamWriter);
                }

                // Create a relative URL path that can be accessed by the client
                // This will be served through a static files middleware we'll configure later
                string fileUrl = $"/LocalUploads/{uniqueFileName}";
                
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file locally: {Error}", ex.Message);
                throw;
            }
        }
    }
}