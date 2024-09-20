using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly MongoDbContext _context;

    public UsersController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.Find(_ => true).ToListAsync();
        return Ok(users);
    }

    [HttpPost]
    [HttpPost]
public async Task<IActionResult> CreateUser(User user)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Ensure that MongoDB auto-generates the Id
    await _context.Users.InsertOneAsync(user);

    // Return the created user with the auto-generated Id
    return Ok(user);
}


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

        await _context.Users.ReplaceOneAsync(u => u.Id == id, existingUser);
        return Ok(existingUser);
    }

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
