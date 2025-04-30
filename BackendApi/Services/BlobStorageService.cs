using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System; // For Uri
using System.IO; // For Stream
using System.Threading.Tasks;

namespace BackendApi.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient? _blobServiceClient; // Made nullable
        private readonly string? _containerName; // Made nullable

        public BlobStorageService(IConfiguration configuration)
        {
            // Allow for null _blobServiceClient in derived classes that don't use Azure
            if (configuration["BlobStorage:AccountName"] != null)
            {
                var accountName = configuration["BlobStorage:AccountName"];
                // Use null-conditional operator ?. and null-coalescing operator ??
                _containerName = configuration["BlobStorage:ContainerName"]; 

                if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(_containerName))
                {
                    Console.WriteLine("Warning: BlobStorage AccountName or ContainerName is not configured. Blob Storage operations will fail.");
                    _blobServiceClient = null; // Ensure it's null if config is missing
                    _containerName = null;
                    return; // Exit constructor early if config is invalid
                }

                try
                {
                    // Use DefaultAzureCredential for passwordless authentication
                    var blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");
                    _blobServiceClient = new BlobServiceClient(blobServiceUri, new DefaultAzureCredential());
                }
                catch (Exception ex)
                {
                    // Log the exception but allow the service to initialize
                    // This allows the application to start even without Azure credentials
                    Console.WriteLine($"Warning: Failed to initialize Azure Blob Storage: {ex.Message}");
                    _blobServiceClient = null; // Ensure it's null on exception
                    _containerName = null; // Ensure container name is also null if client fails
                }
            }
            else
            {
                Console.WriteLine("Warning: BlobStorage:AccountName not found in configuration. Blob Storage operations will fail.");
                _blobServiceClient = null; // Explicitly set to null if AccountName is missing
                _containerName = null;
            }
        }

        public virtual async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            // Add null checks for client and container name
            if (_blobServiceClient == null || string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("BlobServiceClient or ContainerName is not initialized. Check configuration and logs.");
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure the container exists (optional, depends on setup)
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer); // Or Blob if you want public read access

            // Get a reference to the blob
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload the file stream
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            // Return the URL of the uploaded blob
            return blobClient.Uri.ToString();
        }

        // Optional: Add methods for deleting, listing blobs etc. if needed
    }
}
