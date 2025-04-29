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
        private readonly BlobServiceClient? _blobServiceClient;
        private readonly string? _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            // Allow for null _blobServiceClient in derived classes that don't use Azure
            if (configuration["BlobStorage:AccountName"] != null)
            {
                var accountName = configuration["BlobStorage:AccountName"];
                _containerName = configuration["BlobStorage:ContainerName"] ?? throw new InvalidOperationException("BlobStorage:ContainerName not configured.");

                if (string.IsNullOrEmpty(accountName))
                {
                    throw new InvalidOperationException("BlobStorage:AccountName not configured.");
                }

                // Construct the Blob service endpoint URL
                var blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");

                try
                {
                    // Use DefaultAzureCredential for passwordless authentication
                    _blobServiceClient = new BlobServiceClient(blobServiceUri, new DefaultAzureCredential());
                }
                catch (Exception ex)
                {
                    // Log the exception but allow the service to initialize
                    // This allows the application to start even without Azure credentials
                    Console.WriteLine($"Warning: Failed to initialize Azure Blob Storage: {ex.Message}");
                    // The service will throw exceptions when methods are called if not in a derived class
                }
            }
        }

        public virtual async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            if (_blobServiceClient == null)
            {
                throw new InvalidOperationException("BlobServiceClient is not initialized. Cannot upload file to Azure.");
            }

            if (_containerName == null)
            {
                throw new InvalidOperationException("ContainerName is not initialized.");
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
