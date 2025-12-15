using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class PacienteController : ApiControllerBase
{
    private readonly IPacienteService _service;

    public PacienteController(IPacienteService service)
    {
        _service = service;
    }

    [HttpGet("buscartodos")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<PacienteDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarTodos()
    {
        var result = await _service.BuscarTodos();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(long id)
    {
        var result = await _service.BuscarPorId(id);
        return StatusCode(result.StatusCode, result);
    }
    
    [HttpGet("buscarpornome/{nome}")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<PacienteDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPorNome(string nome)
    {
        var result = await _service.BuscarPorNome(nome);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("inserir")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Inserir([FromBody] PacienteDTO dto)
    {
        var result = await _service.Inserir(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar")]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromBody] PacienteDTO dto)
    {
        var result = await _service.Atualizar(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("excluir/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<PacienteDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id)
    {
        var result = await _service.Excluir(id);
        return StatusCode(result.StatusCode, result);
    }
}