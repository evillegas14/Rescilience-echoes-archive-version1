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
        private readonly OpenAIClient _client;
        private readonly string _textDeploymentName;
        private readonly string _dalleDeploymentName;

        public OpenAiService(IConfiguration configuration)
        {
            try 
            {
                var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("AzureOpenAI:Endpoint not configured.");
                _textDeploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName not configured.");
                _dalleDeploymentName = configuration["AzureOpenAI:DalleDeploymentName"] ?? throw new InvalidOperationException("AzureOpenAI:DalleDeploymentName not configured.");

                _client = new OpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to initialize Azure OpenAI client: {ex.Message}");
            }
        }

        public virtual async Task<string?> GetStorySuggestionAsync(string currentStoryText)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("OpenAIClient is not initialized. Cannot generate story suggestion.");
            }

            if (string.IsNullOrWhiteSpace(currentStoryText))
            {
                return null;
            }

            var prompt = $"Continue the following historical story snippet:\n\n{currentStoryText}\n\nContinuation:";

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _textDeploymentName,
                Messages =
                {
                    new ChatRequestUserMessage(prompt)
                },
                MaxTokens = 100,
                Temperature = 0.7f,
            };

            try
            {
                var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
                var completion = response.Value.Choices[0].Message.Content;
                return completion?.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting story suggestion from Azure OpenAI: {ex.Message}");
                return null;
            }
        }

        public virtual async Task<Uri?> GenerateImageAsync(string prompt)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("OpenAIClient is not initialized. Cannot generate image.");
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return null;
            }

            var imageGenerationOptions = new ImageGenerationOptions()
            {
                DeploymentName = _dalleDeploymentName,
                Prompt = prompt,
                Size = ImageSize.Size1024x1024,
                Quality = ImageGenerationQuality.Standard,
                Style = ImageGenerationStyle.Natural
            };

            try
            {
                var response = await _client.GetImageGenerationsAsync(imageGenerationOptions);
                var imageUrl = response.Value.Data[0].Url;
                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating image from Azure OpenAI: {ex.Message}");
                return null;
            }
        }
    }
}
