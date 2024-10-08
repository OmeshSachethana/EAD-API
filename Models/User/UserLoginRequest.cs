/*
 * File: UserLoginRequest.cs
 * Description: This class represents the request model for user login, containing 
 *              the necessary properties for authenticating a user.
 * Author: Sachethana B. L. O
 * Date: 02/10/2024
 * 
 */

public class UserLoginRequest
{
    // The email address of the user
    public string Email { get; set; } 

    // The password of the user
    public string Password { get; set; } 
}
