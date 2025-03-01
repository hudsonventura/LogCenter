using Microsoft.AspNetCore.Mvc;
using server.Repositories;

namespace server.Controllers;

[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    UserContext _userContext;
    public AuthenticationController(UserContext userContext)
    {
        _userContext = userContext;
    }

    [HttpGet("/Login")]
    public IActionResult Index()
    {
        var users = _userContext.Users.FirstOrDefault();
        return Ok(users);
    }
}
