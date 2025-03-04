using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

    [HttpPost("/Login")]
    public IActionResult Login([FromBody] LoginDTO dto)
    {
        try
        {
            User user = _userContext.Users.Where(x => x.email == dto.email).FirstOrDefault();
            if(user is null){
                Domain.User.LoginOrPasswordIncorrect();
            }
            if(!user.CheckPassword(dto.password)){
                Domain.User.LoginOrPasswordIncorrect();
            }
            return Ok(_tokenRepository.GenerateToken(DateTime.UtcNow.AddDays(1), user.email, "interface"));
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
        
    }


    [Authorize]
    [HttpPost("/generateToken")]
    public IActionResult GenerateToken([FromBody] GenerateTokenDTO dto){
        try
        {
            User user = _userContext.Users.Where(x => x.email == User.Identity.Name).FirstOrDefault();
            if(user is null){
                Domain.User.LoginOrPasswordIncorrect();
            }

            return Ok(_tokenRepository.GenerateToken(dto.expires, user.email, dto.owner, dto.tables));
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
    }

    [Authorize]
    [HttpGet("/CheckToken")]
    public IActionResult CheckToken(){
        var email = User.Identity.Name;
        var tables = _tokenRepository.GetAccess(User);
        var expClaim = _tokenRepository.GetExpiration(User); 
        if (expClaim != DateTime.MinValue && expClaim > DateTime.UtcNow)
        {
            return Ok(new {
                exp = expClaim,
                tables = tables
            });
        }

        return BadRequest();
    }

    [Authorize]
    [HttpPost("/ResetPassword")]
    public IActionResult ResetPassword([FromBody] ResetPasswordDTO dto){
        var email = User.Identity.Name;
        var user = _userContext.Users.Where(x => x.email == email).FirstOrDefault();
        if(user is null){
            return BadRequest("User not found");
        }
        user.SetPassword(dto.password);
        _userContext.SaveChanges();
        return Ok();
    }

    public record ResetPasswordDTO(string password);
    
}

public record LoginDTO(string email, string password);

public record GenerateTokenDTO(DateTime expires, string owner, List<string> tables);
