using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class ProcedimentoController : ApiControllerBase
{
    private readonly IProcedimentoService _service;

    public ProcedimentoController(IProcedimentoService service)
    {
        _service = service;
    }

    [HttpPost("inserir")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Inserir([FromBody] ProcedimentoDTO dto)
    {
        var result = await _service.Inserir(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar")]
    [ProducesResponseType(typeof(ApplicationResult<ProcedimentoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ProcedimentoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromBody] ProcedimentoDTO dto)
    {
        var result = await _service.Atualizar(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("excluir/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<ProcedimentoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ProcedimentoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id)
    {
        var result = await _service.Excluir(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("buscartodos")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ProcedimentoDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarTodos()
    {
        var result = await _service.BuscarTodos();
        return StatusCode(result.StatusCode, result);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationResult<ProcedimentoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ProcedimentoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(long id)
    {
        var result = await _service.BuscarPorId(id);
        return StatusCode(result.StatusCode, result);
    }
}