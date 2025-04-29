using BackendApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using BackendApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// *** Define CORS policy name ***
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// *******************************

// Add services to the container.

// *** Add CORS services with enhanced configuration ***
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5252",      // For dev API server
                                            "http://localhost",            // For local file access
                                            "file://")                     // For direct file access
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .SetIsOriginAllowedToAllowWildcardSubdomains();
                      });
});
// *************************

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** Add DbContext configuration ***
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Use SQL Server database if connection string is provided
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Use SQLite in-memory database if no connection string is provided
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("LocalDevelopmentDb"));
    
    Console.WriteLine("Using in-memory database for local development.");
}
// **********************************

// *** Register Application Services based on environment ***
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("Running in Development mode with local service implementations.");
    // Use local implementations for development
    builder.Services.AddSingleton<BlobStorageService, LocalFileStorageService>();
    builder.Services.AddSingleton<OpenAiService, MockOpenAiService>();
}
else
{
    // Use Azure implementations for production
    builder.Services.AddSingleton<BlobStorageService>();
    builder.Services.AddSingleton<OpenAiService>();
}
// ***********************************

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// *** Enable CORS - Apply before other middleware ***
app.UseCors(MyAllowSpecificOrigins);
// *******************

// *** Serve local uploads as static files in Development ***
if (app.Environment.IsDevelopment())
{
    var uploadPath = Path.Combine(app.Environment.ContentRootPath, "LocalUploads");
    if (!Directory.Exists(uploadPath))
    {
        Directory.CreateDirectory(uploadPath);
    }
    
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadPath),
        RequestPath = "/LocalUploads"
    });
    
    app.UseStaticFiles(); // Also serve regular static files
    
    Console.WriteLine($"Serving local files from: {uploadPath}");
}
// *******************************************************

// Authentication has been completely removed
// Authorization middleware is not needed either since we removed [Authorize] attributes
// app.UseAuthorization();

app.MapControllers();

app.Run();
