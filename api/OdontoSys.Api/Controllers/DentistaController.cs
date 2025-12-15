using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class DentistaController : ApiControllerBase
{
    private readonly IDentistaService _service;

    public DentistaController(IDentistaService service)
    {
        _service = service;
    }
    
    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMeuPerfil()
    {
        var userId = User.FindFirst("UsuarioId")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _service.BuscarPorUserId(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("inserir")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] DentistaDTO dto)
    {
        var result = await _service.Inserir(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar")]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromBody] DentistaDTO dto)
    {
        var result = await _service.Atualizar(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("excluir/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id)
    {
        var result = await _service.Excluir(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<DentistaDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(long id)
    {
        var result = await _service.BuscarPorId(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("buscartodos")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<DentistaDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarTodos()
    {
        var result = await _service.BuscarTodos();
        return StatusCode(result.StatusCode, result);
    }
}