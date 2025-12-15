using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class TratamentoExternoController : ApiControllerBase
{
    private readonly ITratamentoExternoService _service;

    public TratamentoExternoController(ITratamentoExternoService service)
    {
        _service = service;
    }

    [HttpPost("inserir")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inserir([FromBody] TratamentoExternoDTO dto)
    {
        var result = await _service.Inserir(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar")]
    [ProducesResponseType(typeof(ApplicationResult<TratamentoExternoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<TratamentoExternoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromBody] TratamentoExternoDTO dto)
    {
        var result = await _service.Atualizar(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("excluir/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<TratamentoExternoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<TratamentoExternoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id)
    {
        var result = await _service.Excluir(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationResult<TratamentoExternoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<TratamentoExternoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(long id)
    {
        var result = await _service.BuscarPorId(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("buscar-por-paciente/{idPaciente}")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<TratamentoExternoDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPorPaciente(long idPaciente)
    {
        var result = await _service.BuscarPorPaciente(idPaciente);
        return StatusCode(result.StatusCode, result);
    }
}