using Example.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Example.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private static int _id = 0;
    private static readonly List<User> _users = new();
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_users);
    }
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var user = _users.FirstOrDefault(x => x.Id == id);
        if (user is null) return NotFound();
        return Ok(user);
    }
    [HttpPost]
    public IActionResult Create(User user)
    {
        _id += 1;
        user.Id = _id;
        _users.Add(user);
        return Ok(user);
    }
    [HttpPut("{id}")]
    public IActionResult Update(int id, User user)
    {
        var currentUser = _users.FirstOrDefault(x => x.Id == id);
        if (currentUser is null) return NotFound();
        currentUser.Name = user.Name;
        currentUser.Email = user.Email;

        int index = _users.FindIndex(x => x.Id == id);
        _users[index] = currentUser;
        return Ok(currentUser);
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var currentUser = _users.FirstOrDefault(x => x.Id == id);
        if (currentUser is null) return NotFound();
        _users.Remove(currentUser);
        return Ok();
    }
}