using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class FinanceiroController : ApiControllerBase
{
    private readonly IFinanceiroService _service;

    public FinanceiroController(IFinanceiroService service)
    {
        _service = service;
    }

    [HttpPost("gerar-parcelas")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ParcelaDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ParcelaDTO>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ParcelaDTO>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ParcelaDTO>>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GerarParcelas([FromBody] GerarParcelasDTO dto)
    {
        var result = await _service.GerarParcelas(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("pagar-parcela/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PagarParcela(long id)
    {
        var result = await _service.PagarParcela(id);
        return StatusCode(result.StatusCode, result);
    }
    
    [HttpPut("cancelar-parcelas/{idOrcamento}")]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelarParcelas(long idOrcamento)
    {
        var result = await _service.CancelarParcelasDoOrcamento(idOrcamento);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("parcelas/{idOrcamento}")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ParcelaDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarParcelas(long idOrcamento)
    {
        var result = await _service.BuscarParcelasPorOrcamento(idOrcamento);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("fluxo-caixa")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<MovimentacaoDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FluxoCaixa()
    {
        var result = await _service.BuscarMovimentacoes();
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("registrar-despesa")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarDespesa([FromBody] MovimentacaoDTO dto)
    {
        dto.Tipo = TipoMovimentacao.Saida;
        var result = await _service.RegistrarDespesa(dto);
        return StatusCode(result.StatusCode, result);
    }
}