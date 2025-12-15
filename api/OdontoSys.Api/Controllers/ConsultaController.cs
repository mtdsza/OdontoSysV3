using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class ConsultaController : ApiControllerBase
{
    private readonly IConsultaService _service;

    public ConsultaController(IConsultaService service)
    {
        _service = service;
    }

    [HttpPost("agendar")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Agendar([FromBody] ConsultaDTO dto)
    {
        var result = await _service.Agendar(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar")]
    [ProducesResponseType(typeof(ApplicationResult<ConsultaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ConsultaDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromBody] ConsultaDTO dto)
    {
        var result = await _service.Atualizar(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("cancelar/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<ConsultaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ConsultaDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(long id)
    {
        var result = await _service.Cancelar(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationResult<ConsultaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ConsultaDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(long id)
    {
        var result = await _service.BuscarPorId(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("buscartodos")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ConsultaDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarTodas()
    {
        var result = await _service.BuscarTodas();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("buscar-por-dentista/{idDentista}")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ConsultaDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPorDentista(long idDentista)
    {
        var result = await _service.BuscarPorDentista(idDentista);
        return StatusCode(result.StatusCode, result);
    }
}