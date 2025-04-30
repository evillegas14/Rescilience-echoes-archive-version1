# Resilience Echoes Archive (Version 1)

This project is a web application designed for creating, managing, and exploring historical archive posts. It leverages AI to provide content suggestions and generate relevant images, enriching the archival experience.

## Features

*   **Post Management:** Create, view, update, and delete archive posts.
*   **User Authentication:** Secure user login and registration (implementation details in `AuthService.cs`).
*   **AI Content Assistance:** Utilizes Azure OpenAI to generate story suggestions and descriptive text for posts.
*   **AI Image Generation:** Leverages Azure OpenAI (DALL-E) to create relevant images based on post content.
*   **Flexible File Storage:** Supports both local file storage (for development) and Azure Blob Storage (for production) to handle image uploads.
*   **Database:** Uses Entity Framework Core with SQL Server for data persistence.

## Technology Stack

*   **Backend:** ASP.NET Core 8
*   **API:** RESTful Web API
*   **Database:** Entity Framework Core 8, Microsoft SQL Server
*   **AI Services:** Azure OpenAI (GPT models for text, DALL-E for images)
*   **Cloud Storage:** Azure Blob Storage
*   **Authentication:** Custom implementation (see `AuthService.cs`)
*   **Language:** C#

## Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
*   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (for local development, e.g., Express or Developer edition)
*   An Azure Subscription (for production deployment and Azure services)
    *   Azure OpenAI Service resource
    *   Azure Blob Storage account
*   Git

## Configuration

Application settings are managed in `BackendApi/appsettings.json` and environment-specific overrides like `BackendApi/appsettings.Development.json`.

Key configuration values needed:

*   **`ConnectionStrings:DefaultConnection`**: Your SQL Server connection string.
*   **`Azure:OpenAiEndpoint`**: The endpoint URL for your Azure OpenAI resource.
*   **`Azure:OpenAiKey`**: The API key for your Azure OpenAI resource.
*   **`Azure:OpenAiDeploymentName`**: The deployment name for your text generation model (e.g., gpt-35-turbo, gpt-4).
*   **`Azure:OpenAiImageDeploymentName`**: The deployment name for your image generation model (e.g., dall-e-3).
*   **`Azure:StorageConnectionString`**: The connection string for your Azure Blob Storage account.
*   **`Azure:StorageContainerName`**: The name of the blob container to store uploads.
*   **`Jwt:Key`**: A secret key for signing JWT tokens.
*   **`Jwt:Issuer`**: The issuer name for JWT tokens.
*   **`Jwt:Audience`**: The audience name for JWT tokens.

**Note:** Use User Secrets or environment variables for sensitive data like API keys and connection strings in production.

## Local Development Setup

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd Rescilience-echoes-archive-version1
    ```
2.  **Configure `appsettings.Development.json`:**
    *   Set the `ConnectionStrings:DefaultConnection` to your local SQL Server instance.
    *   Optionally, configure Azure service settings if you want to test with live Azure resources, or rely on the mock services (`MockOpenAiService`, `LocalFileStorageService`) if configured in `Program.cs` for development.
    *   Set JWT configuration values.
3.  **Apply Database Migrations:**
    *   Navigate to the backend project: `cd BackendApi`
    *   Run the EF Core migration command: `dotnet ef database update`
4.  **Run the Application:**
    *   Still in the `BackendApi` directory, run: `dotnet run`
5.  **Access the API:**
    *   The API will likely be running on `https://localhost:xxxx` or `http://localhost:yyyy` (check the console output).
    *   If Swagger is enabled (check `Program.cs`), you can access the Swagger UI at `/swagger`.

## Production Setup

1.  **Prerequisites:** Ensure you have an Azure SQL Database, Azure OpenAI service, and Azure Blob Storage account configured.
2.  **Configure `appsettings.json`:** Update the settings with your production Azure resource details and connection strings. **Crucially, manage sensitive keys and connection strings securely using Azure Key Vault or environment variables, not directly in `appsettings.json`.**
3.  **Publish the Application:**
    *   Publish the `BackendApi` project:
        ```bash
        cd BackendApi
        dotnet publish -c Release -o ./publish
        ```
    *   Deploy the contents of the `publish` folder to your chosen hosting environment (e.g., Azure App Service, Azure Kubernetes Service).
4.  **Apply Database Migrations:** Ensure migrations are applied to your production Azure SQL Database. This might be part of your CI/CD pipeline or done manually via connection tools.
5.  **Configure Hosting Environment:** Set up necessary environment variables (e.g., `ASPNETCORE_ENVIRONMENT=Production`, connection strings, API keys).

## API Overview

The backend exposes a RESTful API. Key controllers include:

*   **`PostsController`**: Handles CRUD operations for archive posts, including AI interactions and file uploads.
*   **(Potential) `AuthController`**: Handles user registration and login (or this logic might be within another controller or service).

Refer to the controller code (`BackendApi/Controllers/`) or the Swagger UI (if enabled) for detailed endpoint information.

## Project Structure

*   `index.html` / `login.html`: Basic frontend examples (potentially outdated or for testing).
*   `Rescilience-echoes-archive-version1.sln`: Visual Studio Solution file.
*   `BackendApi/`: Contains the ASP.NET Core backend project.
    *   `Controllers/`: API endpoints (e.g., `PostsController.cs`).
    *   `Data/`: Database context (`ApplicationDbContext.cs`).
    *   `Migrations/`: Entity Framework Core database migrations.
    *   `Models/`: Data models (e.g., `Post.cs`, `User.cs`, `CreatePostDto.cs`).
    *   `Services/`: Business logic services:
        *   `AuthService.cs`: Handles authentication logic.
        *   `BlobStorageService.cs`: Azure Blob Storage implementation.
        *   `LocalFileStorageService.cs`: Local file storage implementation.
        *   `OpenAiService.cs`: Azure OpenAI service implementation.
        *   `MockOpenAiService.cs`: Mock AI service for development/testing.
    *   `Properties/launchSettings.json`: Visual Studio debug launch profiles.
    *   `Program.cs`: Application startup, service registration, and middleware configuration.
    *   `appsettings.json`: Main configuration file.
    *   `appsettings.Development.json`: Development-specific configuration overrides.
    *   `BackendApi.csproj`: Project file defining dependencies and build settings.

