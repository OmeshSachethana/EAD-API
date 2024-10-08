/*
 * File: JwtSettings.cs
 * Description: This class contains the configuration settings for JWT (JSON Web Token) authentication.
 *              It holds the necessary information for generating and validating JWT tokens.
 * Author: Sachethana B. L. O.
 * Date: 02/10/2024
 * 
 * This class is used to configure the JWT authentication parameters, such as the secret key, issuer, 
 * audience, and token expiration settings.
 */

public class JwtSettings
{
    // The secret key used for signing the JWT token
    public string SecretKey { get; set; } 

    // The issuer of the JWT token, usually the application name or identifier
    public string Issuer { get; set; } 

    // The audience for which the JWT token is intended, typically the client or application
    public string Audience { get; set; } 

    // The duration in minutes for which the JWT token is valid before it expires
    public int ExpiryMinutes { get; set; } 
}
