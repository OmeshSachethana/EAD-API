/*
 * File: JwtHelper.cs
 * Description: This class provides functionality for generating JWT tokens for user authentication.
 *              It uses settings defined in the JwtSettings class for token creation.
 * Author: Sachethana B. L. O
 * Date: 02/10/2024
 */

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens; 
using System; 
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text; 

public class JwtHelper
{
    private readonly JwtSettings _jwtSettings; // Holds JWT configuration settings

    // Constructor to initialize JwtHelper with JwtSettings
    public JwtHelper(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value; // Get the value of the JwtSettings
    }

    // Method to generate a JWT token based on user information
    public string GenerateJwtToken(User user)
    {
        // Create a security key using the secret key from JwtSettings
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        // Define the signing credentials using the security key and HMAC SHA256 algorithm
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Define the claims to include in the token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (user ID)
            new Claim(ClaimTypes.Name, user.Username), // User's username
            new Claim(ClaimTypes.Email, user.Email), // User's email address
            new Claim(ClaimTypes.Role, user.Role) // User's role
        };

        // Create the JWT token with the specified parameters
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer, // Token issuer
            audience: _jwtSettings.Audience, // Token audience
            claims: claims, // Claims included in the token
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes), // Token expiration time
            signingCredentials: credentials); // Signing credentials

        // Return the generated token as a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
