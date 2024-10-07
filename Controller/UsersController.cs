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

    // Check if the user role is "Customer" and set IsActive to false
    if (user.Role == "Customer")
    {
        user.IsActive = false;
    }

    // Hash the password before saving the user
    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

    // Ensure that MongoDB auto-generates the Id
    await _context.Users.InsertOneAsync(user);

    // Return the created user with the auto-generated Id
    return Ok(user);
}

[Authorize]
[HttpGet("customers")]
public async Task<IActionResult> GetCustomers()
{
    var customers = await _context.Users.Find(u => u.Role == "Customer").ToListAsync();
    return Ok(customers);
}



[Authorize]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
{
    var existingUser = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    if (existingUser == null)
    {
        return NotFound();
    }

    // Update only the IsActive property
    existingUser.IsActive = user.IsActive;

    // If other fields are included, you might want to ignore them or validate them accordingly.
    // e.g., if (user.Username != null) existingUser.Username = user.Username;

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
