using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

[ApiController]
[Route("api/funcionario")]
public class FuncionarioController : ApiControllerBase
{
    private readonly IFuncionarioService _service;

    public FuncionarioController(IFuncionarioService service)
    {
        _service = service;
    }

    [HttpGet("buscartodos")]
    [Authorize]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<FuncionarioDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BuscarTodos()
    {
        var result = await _service.BuscarTodos();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApplicationResult<FuncionarioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<FuncionarioDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BuscarPorId(long id)
    {
        var result = await _service.BuscarPorId(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("inserir")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Inserir([FromBody] FuncionarioDTO dto)
    {
        var result = await _service.Inserir(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApplicationResult<FuncionarioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<FuncionarioDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApplicationResult<FuncionarioDTO>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Atualizar([FromBody] FuncionarioDTO dto)
    {
        var emailLogado = User.FindFirst(ClaimTypes.Name)?.Value;

        if (!string.IsNullOrEmpty(emailLogado))
        {
            var funcionarioAlvo = await _service.BuscarPorId(dto.IdFuncionario);
            
            if (funcionarioAlvo.IsSuccess && funcionarioAlvo.Data != null && funcionarioAlvo.Data.Email == emailLogado)
            {
                if (dto.Ativo == false)
                {
                    return StatusCode(403, ApplicationResult<FuncionarioDTO>.Failure("Você não pode desativar seu próprio usuário.", 403));
                }
            }
        }

        var result = await _service.Atualizar(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("excluir/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApplicationResult<FuncionarioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<FuncionarioDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Excluir(long id)
    {
        var funcionarioAlvo = await _service.BuscarPorId(id);
        
        if (funcionarioAlvo.IsSuccess && funcionarioAlvo.Data != null)
        {
            var emailLogado = User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(emailLogado) && funcionarioAlvo.Data.Email == emailLogado)
            {
                return StatusCode(403, ApplicationResult<string>.Failure("Você não pode excluir seu próprio usuário.", 403));
            }
        }

        var result = await _service.Excluir(id);
        return StatusCode(result.StatusCode, result);
    }
}