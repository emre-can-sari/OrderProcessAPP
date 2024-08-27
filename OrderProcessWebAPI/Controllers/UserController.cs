using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderProcess.Business.Services;
using OrderProcess.Entities.Entities;
using SQLitePCL;

namespace OrderProcessWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto registerDto)
    {
        if (registerDto == null)
        {
            return BadRequest("Invalid client request");
        }

        var user = _userService.register(registerDto);

        if (user == null)
        {
            return BadRequest("User registration failed");
        }

        return Ok();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null)
        {
            return BadRequest("Invalid client request");
        }

        var tokenDto = _userService.login(loginDto);
        if (tokenDto == null)
        {
            return Unauthorized("Invalid email or password");
        }

        return Ok(tokenDto);
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public IActionResult GetAllUser()
    {
        var user = _userService.GetAllUser();
        if (user == null) return BadRequest();

        return Ok(user);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public IActionResult GetUserById(int id)
    {
        var user = _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }


    [HttpPut]
    [Authorize(Roles = "admin")]
    public IActionResult UpdateUser([FromBody] UpdateUserDTO updateUserDTO)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = _userService.UpdateUser(updateUserDTO);
            return Ok(user);
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Invalid user data.");
        }
        catch (InvalidOperationException)
        {
            return NotFound("User not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public IActionResult DeleteUser(int id)
    {
        var user = _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }

        _userService.DeleteUser(id);
        return NoContent();
    }
}