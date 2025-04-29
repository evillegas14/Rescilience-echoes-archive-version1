# Resilience Echoes Archive (Version 1)

This project is a web application for creating and managing historical archive posts, featuring AI-powered suggestions for story content and images.

## Features

*   Create archive posts with title, date, timeline, story, and optional cited works.
*   Upload images associated with posts.
*   Get AI-driven story suggestions based on the current text.
*   Get AI-generated image suggestions based on the story.

## Prerequisites

*   .NET 8 SDK (or later)
*   Node.js and npm (if you plan to enhance the frontend)
*   For production: Azure Account (for Azure Blob Storage, Azure OpenAI, and SQL Database)

## Local Development Setup

The application now supports a local development mode that doesn't require Azure services:

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd Rescilience-echoes-archive-version1
    ```

2.  **Run the Backend API in Development mode:**
    *   From the `BackendApi` directory, run: `dotnet run`
    *   The API will be available at `http://localhost:5252`
    *   In development mode:
        * The application uses an in-memory database instead of SQL Server
        * Images are stored locally in a `LocalUploads` folder instead of Azure Blob Storage
        * A mock OpenAI service provides simulated AI responses instead of Azure OpenAI

3.  **Run the Frontend:**
    *   Simply open the `index.html` file in your web browser.

## Production Setup

For a production deployment, you'll need to configure Azure services:

1.  **Configure Backend API (`BackendApi/appsettings.json`):**
    *   **ConnectionStrings**: Update `DefaultConnection` with your database connection string.
    *   **BlobStorage**: Provide your Azure Blob Storage `AccountName` and `ContainerName`.
    *   **AzureOpenAI**: Set your Azure OpenAI `Endpoint`, `DeploymentName` (for text generation, e.g., gpt-4o-mini), and `DalleDeploymentName` (for image generation, e.g., dall-e-3).

2.  **Database Migrations:**
    *   Open a terminal in the root directory.
    *   Navigate to the BackendApi project: `cd BackendApi`
    *   Apply migrations: `dotnet ef database update`

## Usage

1.  Fill out the form fields (Title, Date, Story are required).
2.  As you type in the "Story" text area (after ~20 characters), AI suggestions will appear below it.
3.  Once you have sufficient story text, click "Suggest Image Based on Story" to get an AI-generated image preview.
4.  Optionally, upload your own image using the file input.
5.  Add any citations in the "Work Cited" field.
6.  Click "Submit Post".

## Project Structure

*   `index.html`: Main frontend page for creating posts.
*   `BackendApi/`: Contains the ASP.NET Core backend project.
    *   `Controllers/`: API endpoints (Posts).
    *   `Data/`: Database context (`ApplicationDbContext`).
    *   `Migrations/`: Entity Framework Core database migrations.
    *   `Models/`: Data models (Post).
    *   `Services/`: Business logic services:
        *   `BlobStorageService.cs`: Azure Blob Storage implementation.
        *   `LocalFileStorageService.cs`: Local file storage implementation for development.
        *   `OpenAiService.cs`: Azure OpenAI service implementation.
        *   `MockOpenAiService.cs`: Mock AI service for development.
    *   `Program.cs`: Application startup and configuration.
    *   `appsettings.json`: Configuration file.

