using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        var result = await _authService.Register(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var result = await _authService.Login(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("alterar-senha")]
    [Authorize]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AlterarSenha([FromBody] ChangePasswordDTO dto)
    {
        var userId = User.FindFirst("UsuarioId")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.ChangePassword(userId, dto);
        return StatusCode(result.StatusCode, result);
    }
}