/*
 * File: UsersController.cs
 * Description: This controller handles user-related operations such as login, user creation, 
 *              retrieval, updating, and deletion. It integrates with MongoDB for user data 
 *              storage and uses JWT for authentication.
 * Author: Sachethana B. L. O
 * Date: 02/10/2024
 * 
 * This file contains various API endpoints for managing users in the system using MongoDB as the database.
 * The endpoints are restricted by user roles: Vendor, Administrator, and Customer.
 */

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly JwtHelper _jwtHelper;

    // Constructor to initialize the MongoDbContext and JwtHelper services
    public UsersController(MongoDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    // Login endpoint to authenticate the user and generate a JWT token
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    {
        // Find user by email
        var user = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();

        // Check if user exists and password matches
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Check if the user is active
        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Your account is inactive. Please contact support." });
        }

        // Generate JWT token
        var token = _jwtHelper.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    // Endpoint to retrieve all users (requires authentication)
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        // Retrieve all users from the database
        var users = await _context.Users.Find(_ => true).ToListAsync();
        return Ok(users);
    }

    // Endpoint to create a new user
    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if the user role is "Customer" and set IsActive to false
        if (user.Role == "Customer")
        {
            user.IsActive = false;
        }

        // Hash the password before saving the user
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        // Set the CreatedAt and UpdatedAt timestamps
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.Users.InsertOneAsync(user);

        return Ok(user);
    }

    // Endpoint to retrieve all customers (requires authentication)
    [Authorize]
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        // Retrieve all users with the role "Customer"
        var customers = await _context.Users.Find(u => u.Role == "Customer").ToListAsync();
        return Ok(customers);
    }

    // Endpoint to update an existing user by ID (requires authentication)
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
    {
        // Find the existing user by ID
        var existingUser = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (existingUser == null)
        {
            return NotFound();
        }

        // Update IsActive property if passed
        existingUser.IsActive = user.IsActive;

        // Set the UpdatedAt timestamp to current time
        existingUser.UpdatedAt = DateTime.UtcNow;

        await _context.Users.ReplaceOneAsync(u => u.Id == id, existingUser);
        return Ok(existingUser);
    }

    // Endpoint to delete a user by ID (requires authentication)
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Delete the user from the database
        var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
        if (result.DeletedCount == 0)
        {
            return NotFound();
        }
        return Ok();
    }
}
