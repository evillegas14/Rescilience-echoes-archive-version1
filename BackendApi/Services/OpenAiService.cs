using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BackendApi.Services
{
    public class OpenAiService
    {
        private readonly OpenAIClient? _client;
        private readonly string? _textDeploymentName;
        private readonly string? _dalleDeploymentName;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiService>? _logger;

        public OpenAiService(IConfiguration configuration, ILogger<OpenAiService>? logger = null)
        {
            _configuration = configuration;
            _logger = logger;
            
            try 
            {
                var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("AzureOpenAI:Endpoint not configured.");
                _textDeploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName not configured.");
                _dalleDeploymentName = configuration["AzureOpenAI:DalleDeploymentName"] ?? throw new InvalidOperationException("AzureOpenAI:DalleDeploymentName not configured.");

                // First try authentication with API key
                var apiKey = configuration["AzureOpenAI:ApiKey"];
                
                if (!string.IsNullOrEmpty(apiKey))
                {
                    _logger?.LogInformation("Initializing Azure OpenAI client with API key authentication");
                    _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                }
                else
                {
                    // Fall back to DefaultAzureCredential (managed identity or other Azure AD credentials)
                    _logger?.LogInformation("Initializing Azure OpenAI client with DefaultAzureCredential");
                    _client = new OpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
                }
                
                _logger?.LogInformation("Successfully initialized Azure OpenAI client");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Failed to initialize Azure OpenAI client: {Message}", ex.Message);
                Console.WriteLine($"Warning: Failed to initialize Azure OpenAI client: {ex.Message}");
            }
        }

        public virtual async Task<string?> GetStorySuggestionAsync(string currentStoryText)
        {
            if (_client == null)
            {
                _logger?.LogError("Cannot generate story suggestion: OpenAIClient is not initialized");
                throw new InvalidOperationException("OpenAIClient is not initialized. Cannot generate story suggestion.");
            }

            if (_textDeploymentName == null)
            {
                _logger?.LogError("Cannot generate story suggestion: TextDeploymentName is not initialized");
                throw new InvalidOperationException("TextDeploymentName is not initialized.");
            }

            if (string.IsNullOrWhiteSpace(currentStoryText))
            {
                _logger?.LogWarning("Empty story text provided for suggestion");
                return null;
            }

            var prompt = $"Continue the following historical story snippet:\n\n{currentStoryText}\n\nContinuation:";

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _textDeploymentName,
                Messages = { new ChatRequestUserMessage(prompt) },
                MaxTokens = 100,
                Temperature = 0.7f,
            };

            try
            {
                _logger?.LogInformation("Requesting story suggestion from Azure OpenAI");
                var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
                var completion = response.Value.Choices[0].Message.Content;
                return completion?.Trim();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting story suggestion from Azure OpenAI: {Message}", ex.Message);
                Console.WriteLine($"Error getting story suggestion from Azure OpenAI: {ex.Message}");
                return null;
            }
        }

        public virtual async Task<Uri?> GenerateImageAsync(string prompt)
        {
            if (_client == null)
            {
                _logger?.LogError("Cannot generate image: OpenAIClient is not initialized");
                throw new InvalidOperationException("OpenAIClient is not initialized. Cannot generate image.");
            }

            if (_dalleDeploymentName == null)
            {
                _logger?.LogError("Cannot generate image: DalleDeploymentName is not initialized");
                throw new InvalidOperationException("DalleDeploymentName is not initialized.");
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                _logger?.LogWarning("Empty prompt provided for image generation");
                return null;
            }

            try
            {
                _logger?.LogInformation("Requesting image generation from Azure OpenAI with prompt: {PromptExcerpt}", 
                    prompt.Length > 50 ? prompt.Substring(0, 50) + "..." : prompt);
                
                // Using the updated ImageGenerations API for DALL-E 3
                Response<ImageGenerations> imageGenerations = await _client.GetImageGenerationsAsync(
                    new ImageGenerationOptions()
                    {
                        DeploymentName = _dalleDeploymentName,
                        Prompt = prompt,
                        Size = ImageSize.Size1024x1024,
                        NumberOfImages = 1,
                        Quality = ImageGenerationQuality.Standard,
                        Style = ImageGenerationStyle.Natural,
                        ResponseFormat = ImageGenerationResponseFormat.Url
                    });

                // Get the image URL from the response
                Uri imageUri = imageGenerations.Value.Data[0].Url;
                _logger?.LogInformation("Successfully generated image");
                return imageUri;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating image from Azure OpenAI: {Message}", ex.Message);
                Console.WriteLine($"Error generating image from Azure OpenAI: {ex.Message}");
                return null;
            }
        }
    }
}
