using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class OrcamentoController : ApiControllerBase
{
    private readonly IOrcamentoService _service;

    public OrcamentoController(IOrcamentoService service)
    {
        _service = service;
    }

    [HttpPost("criar")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] OrcamentoDTO dto)
    {
        var result = await _service.CriarOrcamento(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar")]
    [ProducesResponseType(typeof(ApplicationResult<OrcamentoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<OrcamentoDTO>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<OrcamentoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromBody] OrcamentoDTO dto)
    {
        var result = await _service.AtualizarOrcamento(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationResult<OrcamentoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<OrcamentoDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(long id)
    {
        var result = await _service.BuscarPorId(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("por-paciente/{idPaciente}")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<OrcamentoDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPorPaciente(long idPaciente)
    {
        var result = await _service.BuscarPorPaciente(idPaciente);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("aprovar/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Aprovar(long id)
    {
        var result = await _service.AprovarOrcamento(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Excluir(long id)
    {
        var result = await _service.ExcluirOrcamento(id);
        return StatusCode(result.StatusCode, result);
    }
}