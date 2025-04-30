using BackendApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using BackendApi.Services; // Added for AuthService, BlobStorageService, etc.
using Microsoft.AspNetCore.Authentication.JwtBearer; // Added for JwtBearer
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using BackendApi.Models; // Added for potential User model usage in AuthService

var builder = WebApplication.CreateBuilder(args);

// *** Define CORS policy name ***
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// *******************************

// Add services to the container.

// *** Add CORS services ***
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
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

// Always register AuthService as Scoped
builder.Services.AddScoped<AuthService>();
// ***********************************

// *** Configure JWT Authentication ***
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "DEVELOPMENT_TEMPORARY_KEY_ONLY_FOR_LOCAL_USE");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = !builder.Environment.IsDevelopment(),
        ValidIssuer = jwtSettings["Issuer"] ?? "http://localhost",
        ValidateAudience = !builder.Environment.IsDevelopment(),
        ValidAudience = jwtSettings["Audience"] ?? "http://localhost",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
// *********************************

// *** Add Authorization Services ***
builder.Services.AddAuthorization();
// ********************************

builder.Services.AddControllers();

var app = builder.Build();

// *** Initialize Default User ***
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var authService = services.GetRequiredService<AuthService>();
        await authService.InitializeDefaultUserAsync();
        Console.WriteLine("Default user initialization checked/completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during default user initialization: {ex.Message}");
    }
}
// ******************************

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

// *** Enable CORS ***
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
    
    Console.WriteLine($"Serving local files from: {uploadPath}");
}
// *******************************************************

// *** Enable Authentication and Authorization ***
app.UseAuthentication();
app.UseAuthorization();
// *********************************************

app.MapControllers();

app.Run();
