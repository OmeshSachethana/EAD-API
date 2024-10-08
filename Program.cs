/*
 * File: Program.cs
 * Description: This file configures the ASP.NET application, setting up services, middleware, 
 *              and authentication using JWT (JSON Web Tokens) for user authentication.
 * Author: Sachethana B. L. O.
 * Date: 02/10/2024
 * 
 * This file sets up the application's configuration, services, and middleware, including
 * JWT authentication, MongoDB context, CORS policy, and the necessary controllers.
 */

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Register JwtHelper as a singleton service to handle JWT operations
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtHelper>();

// Add authentication services to the application
builder.Services.AddAuthentication(options =>
{
    // Set the default authentication and challenge schemes to JWT Bearer
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // Configure JWT token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate the token's issuer
        ValidateIssuer = true,
        // Validate the token's audience
        ValidateAudience = true,
        // Validate the token's lifetime
        ValidateLifetime = true,
        // Validate the signing key
        ValidateIssuerSigningKey = true,
        // Get the valid issuer from configuration
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        // Get the valid audience from configuration
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        // Set the signing key from the secret key in configuration
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
    };
});

// Register MongoDB context as a singleton service
builder.Services.AddSingleton<MongoDbContext>();

// Configure CORS (Cross-Origin Resource Sharing) policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Allow requests from the specified frontend URL
        policy.WithOrigins("http://localhost:3000") // Your frontend URL
              .AllowAnyHeader() // Allow any headers in the request
              .AllowAnyMethod() // Allow any HTTP methods (GET, POST, etc.)
              .AllowCredentials(); // Optional, allows credentials such as cookies or authorization headers
    });
});

// Add MVC controllers to the service container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // For API exploration
builder.Services.AddSwaggerGen(); // For Swagger documentation

var app = builder.Build(); // Build the application

// Enable CORS for the application using the defined policy
app.UseCors("AllowFrontend");

// Enable authentication middleware to validate JWT tokens
app.UseAuthentication();  
// Enable authorization middleware to enforce access controls
app.UseAuthorization();

// Map controller routes to the application
app.MapControllers();

// Run the application
app.Run();
