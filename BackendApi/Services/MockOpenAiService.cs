using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendApi.Services
{
    public class MockOpenAiService : OpenAiService
    {
        private readonly ILogger<MockOpenAiService> _logger;
        private readonly Random _random = new Random();

        public MockOpenAiService(IConfiguration configuration, ILogger<MockOpenAiService> logger) 
            : base(configuration)
        {
            _logger = logger;
            _logger.LogInformation("Using Mock OpenAI Service for local development");
        }

        public override async Task<string?> GetStorySuggestionAsync(string currentText)
        {
            _logger.LogInformation("Mock OpenAI Service generating story suggestion");
            
            // Simple mock suggestions based on the text length
            await Task.Delay(500); // Simulate network delay
            
            string[] suggestions = new string[] 
            {
                "Consider adding details about how this event impacted the local community.",
                "You might want to elaborate on the historical significance of this moment.",
                "Try including a quote from a historical figure who witnessed this event.",
                "Adding a description of the weather or setting could help readers visualize the scene.",
                "Consider explaining how this connects to other historical events from the same period.",
                "The narrative could benefit from describing the emotional impact on those involved."
            };
            
            return suggestions[_random.Next(suggestions.Length)];
        }

        public override async Task<Uri?> GenerateImageAsync(string prompt)
        {
            _logger.LogInformation("Mock OpenAI Service generating image for prompt: {Prompt}", prompt);
            
            // Simulate network delay
            await Task.Delay(1000);
            
            // Return placeholder images based on keyword detection in the prompt
            string imageUrl;
            
            if (prompt.Contains("war") || prompt.Contains("battle") || prompt.Contains("conflict"))
            {
                imageUrl = "https://placehold.co/600x400/gray/white?text=Historical+Battle+Scene";
            }
            else if (prompt.Contains("president") || prompt.Contains("leader") || prompt.Contains("king"))
            {
                imageUrl = "https://placehold.co/600x400/darkblue/white?text=Historical+Leader";
            }
            else if (prompt.Contains("invention") || prompt.Contains("discovery") || prompt.Contains("technology"))
            {
                imageUrl = "https://placehold.co/600x400/orange/white?text=Historical+Innovation";
            }
            else
            {
                imageUrl = "https://placehold.co/600x400/darkgreen/white?text=Historical+Scene";
            }
            
            return new Uri(imageUrl);
        }
    }
}