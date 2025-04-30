using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BackendApi.Services
{
    public class OpenAiService
    {
        private readonly OpenAIClient? _client; // Made nullable
        private readonly string? _textDeploymentName; // Made nullable
        private readonly string? _dalleDeploymentName; // Made nullable

        public OpenAiService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureOpenAI:Endpoint"];
            _textDeploymentName = configuration["AzureOpenAI:DeploymentName"];
            _dalleDeploymentName = configuration["AzureOpenAI:DalleDeploymentName"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(_textDeploymentName) || string.IsNullOrEmpty(_dalleDeploymentName))
            {
                Console.WriteLine("Warning: Azure OpenAI configuration (Endpoint, DeploymentName, or DalleDeploymentName) is missing. OpenAI features will be disabled.");
                _client = null; // Ensure client is null if config is missing
                return; // Exit constructor
            }

            try
            {
                _client = new OpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to initialize Azure OpenAI client: {ex.Message}. OpenAI features will be disabled.");
                _client = null; // Ensure client is null on exception
            }
        }

        // Mark return type as nullable to match potential null return
        public virtual async Task<string?> GetStorySuggestionAsync(string currentStoryText)
        {
            if (_client == null || string.IsNullOrEmpty(_textDeploymentName))
            {
                Console.WriteLine("OpenAI client or text deployment name not initialized. Cannot get story suggestion.");
                return null; // Return null if not initialized
            }

            if (string.IsNullOrWhiteSpace(currentStoryText))
            {
                return null;
            }

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _textDeploymentName, // Use deployment name from config
                Messages =
                {
                    new ChatRequestSystemMessage("You are an AI assistant helping a user write historical narratives. Provide a concise suggestion (1-2 sentences) on how to improve or continue the following text, focusing on historical accuracy, engagement, or narrative flow."),
                    new ChatRequestUserMessage(currentStoryText),
                },
                MaxTokens = 100, // Limit response length
                Temperature = 0.7f, // Adjust creativity
            };

            try
            {
                var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
                var suggestion = response.Value.Choices[0].Message.Content;
                return suggestion?.Trim(); // Trim whitespace and return
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting story suggestion from Azure OpenAI: {ex.Message}");
                return null; // Return null on error
            }
        }

        // Mark return type as nullable
        public virtual async Task<Uri?> GenerateImageAsync(string prompt)
        {
            if (_client == null || string.IsNullOrEmpty(_dalleDeploymentName))
            {
                Console.WriteLine("OpenAI client or DALL-E deployment name not initialized. Cannot generate image.");
                return null; // Return null if not initialized
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return null;
            }

            var imageGenerationOptions = new ImageGenerationOptions()
            {
                DeploymentName = _dalleDeploymentName,
                Prompt = prompt,
                Size = ImageSize.Size1024x1024, // Or other supported sizes
                Quality = ImageGenerationQuality.Standard, // Or 'hd'
                Style = ImageGenerationStyle.Natural
            };

            try
            {
                var response = await _client.GetImageGenerationsAsync(imageGenerationOptions);
                var imageUrl = response.Value.Data[0].Url; // Get the URL of the first generated image
                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating image from Azure OpenAI: {ex.Message}");
                return null; // Return null on error
            }
        }
    }
}
