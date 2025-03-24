using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using server.Domain;
using server.Repositories;

namespace server.Controllers;


/// <summary>
/// Authentication controller
/// </summary>
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


    /// <summary>
    /// Login to LogCenter
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
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
            return Ok(_tokenRepository.GenerateToken(DateTime.UtcNow.AddDays(1), user.email, user.name, "interface"));
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        } 
    }
    public record LoginDTO(string email, string password);

    /// <summary>
    /// Generate  a token to a sistem. Here you are logged and genrating a token to a system log here
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("/generateToken")]
    public IActionResult GenerateToken([FromBody] GenerateTokenDTO dto){
        try
        {
            User user = _userContext.Users.Where(x => x.email == User.Identity.Name).FirstOrDefault();
            if(user is null){
                Domain.User.LoginOrPasswordIncorrect();
            }

            return Ok(_tokenRepository.GenerateToken(dto.expires, user.email, dto.name, dto.tables));
        }
        catch (System.Exception error)
        {
            return BadRequest(error.Message);
        }
    }
    public record GenerateTokenDTO(DateTime expires, string name, List<string> tables);



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
    [HttpPost("/ChangePassword")]
    public IActionResult ChangePassword([FromBody] ChangePasswordDTO dto){
        var email = User.Identity.Name;
        var user = _userContext.Users.Where(x => x.email == email).FirstOrDefault();
        if(user is null){
            return BadRequest("User not found");
        }
        user.SetPassword(dto.password);
        _userContext.SaveChanges();
        return Ok();
    }
    public record ChangePasswordDTO(string password);



    [Authorize]
    [HttpPost("/CreateUser")]
    public IActionResult ChangeEmail([FromBody] CreateUserDTO dto){
        User newUser = new User(){
            email = dto.email,
            name = dto.name
        };
        newUser.SetPassword(dto.password);
        _userContext.Users.Add(newUser);
        _userContext.SaveChanges();
        return Ok();
    }
    public record CreateUserDTO(string email, string name, string password);
    
}
