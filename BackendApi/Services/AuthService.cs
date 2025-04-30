using BackendApi.Data;
using BackendApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace BackendApi.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Placeholder for user registration logic
        public async Task<User?> RegisterAsync(string username, string password)
        {
             if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return null; // Username already exists
            }

            var passwordHash = HashPassword(password);
            var user = new User { Username = username, PasswordHash = passwordHash };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // Placeholder for user login logic
        public async Task<string?> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

             // Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "DEVELOPMENT_TEMPORARY_KEY_ONLY_FOR_LOCAL_USE"); // Use a fallback for development
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                    // Add other claims as needed
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiration
                Issuer = _configuration["Jwt:Issuer"] ?? "http://localhost", // Use fallback
                Audience = _configuration["Jwt:Audience"] ?? "http://localhost", // Use fallback
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Method to initialize a default user if none exists
        public async Task InitializeDefaultUserAsync()
        {
            if (!await _context.Users.AnyAsync())
            {
                Console.WriteLine("No users found. Creating default user 'admin' with password 'password'.");
                var defaultUsername = "admin";
                var defaultPassword = "password"; // CHANGE THIS IN PRODUCTION!
                var passwordHash = HashPassword(defaultPassword);

                var defaultUser = new User
                {
                    Username = defaultUsername,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(defaultUser);
                await _context.SaveChangesAsync();
                Console.WriteLine("Default user 'admin' created successfully.");
            }
            else
            {
                 Console.WriteLine("Users already exist in the database. Skipping default user creation.");
            }
        }

         private string HashPassword(string password)
        {
            // Use a proper password hashing library like BCrypt.Net or ASP.NET Core Identity's PasswordHasher
            // This is a simplified example for demonstration ONLY. DO NOT USE IN PRODUCTION.
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Use the same hashing method to compare
            // This is a simplified example for demonstration ONLY. DO NOT USE IN PRODUCTION.
            var enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHash;
        }
    }
}
