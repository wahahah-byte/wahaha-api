using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Handlers;
using wahaha.API.Handlers.Auth;
using wahaha.API.Models.DTOs;

namespace wahaha.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRequestHandler<RegisterUserRequest, AuthResponseDto> _register;
    private readonly IRequestHandler<LoginUserRequest, AuthResponseDto> _login;

    public AuthController(
        IRequestHandler<RegisterUserRequest, AuthResponseDto> register,
        IRequestHandler<LoginUserRequest, AuthResponseDto> login)
    {
        _register = register;
        _login = login;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        => (await _register.HandleAsync(new RegisterUserRequest(dto))).ToActionResult();

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        => (await _login.HandleAsync(new LoginUserRequest(dto))).ToActionResult();
}
