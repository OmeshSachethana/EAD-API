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
    
    // Verify the user exists and the password matches using BCrypt
    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
    {
        return Unauthorized();
    }

    // Generate JWT token using JwtHelper
    var token = _jwtHelper.GenerateJwtToken(user);
    Console.WriteLine($"User found: {user.Username}, Role: {user.Role}, ID: {user.Id}");
    return Ok(new { Token = token });
}


    [Authorize] // Protect this endpoint so that only authenticated users can access it
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.Find(_ => true).ToListAsync();
        return Ok(users);
    }


[HttpPost]
public async Task<IActionResult> CreateUser(User user)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Hash the password before saving the user
    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

    // Ensure that MongoDB auto-generates the Id
    await _context.Users.InsertOneAsync(user);

    // Return the created user with the auto-generated Id
    return Ok(user);
}

[Authorize]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(string id, User user)
{
    var existingUser = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    if (existingUser == null)
    {
        return NotFound();
    }

    existingUser.Username = user.Username;
    existingUser.Email = user.Email;
    existingUser.Role = user.Role;
    existingUser.IsActive = user.IsActive;

    // If the password is updated, hash the new password
    if (!string.IsNullOrEmpty(user.Password))
    {
        existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
    }

    await _context.Users.ReplaceOneAsync(u => u.Id == id, existingUser);
    return Ok(existingUser);
}


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
        if (result.DeletedCount == 0)
        {
            return NotFound();
        }
        return Ok();
    }
}
