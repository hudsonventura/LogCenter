using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Domain;
using server.Repositories;

namespace server.Controllers;

[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    UserContext _userContext;
    private TokenRepository _tokenRepository;
    public AuthenticationController(UserContext userContext,TokenRepository tokenRepository)
    {
        _userContext = userContext;
        _tokenRepository = tokenRepository;
    }

    [HttpGet("/Login")]
    public IActionResult Login([FromBody] LoginDTO loginDTO)
    {
        try
        {
            User user = _userContext.Users.Where(x => x.email == loginDTO.email).FirstOrDefault();
            if(user is null){
                Domain.User.LoginOrPasswordIncorrect();
            }
            if(!user.CheckPassword(loginDTO.password)){
                Domain.User.LoginOrPasswordIncorrect();
            }
            return Ok(_tokenRepository.GenerateToken(user.email, "teste"));
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
        
    }

    [Authorize]
    [HttpGet("/Logged")]
    public IActionResult Logged(){
        var username = User.Identity.Name;
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        return Ok(username);
    }

}

public record LoginDTO(string email, string password);