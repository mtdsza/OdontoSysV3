using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

public class EstoqueController : ApiControllerBase
{
    private readonly IEstoqueService _service;

    public EstoqueController(IEstoqueService service)
    {
        _service = service;
    }

    [HttpGet("listar")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<ItemEstoqueDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var result = await _service.BuscarTodos();
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("criar-item")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] ItemEstoqueDTO dto)
    {
        var result = await _service.CriarItem(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("atualizar-item")]
    [ProducesResponseType(typeof(ApplicationResult<ItemEstoqueDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ItemEstoqueDTO>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromBody] ItemEstoqueDTO dto)
    {
        var result = await _service.AtualizarItem(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("excluir-item/{id}")]
    [ProducesResponseType(typeof(ApplicationResult<ItemEstoqueDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<ItemEstoqueDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApplicationResult<ItemEstoqueDTO>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Excluir(long id)
    {
        var result = await _service.ExcluirItem(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("movimentacao")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Movimentar([FromBody] MovimentacaoEstoqueDTO dto)
    {
        var result = await _service.RegistrarMovimentacao(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("registrar-uso")]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarUso([FromBody] RegistrarUsoDTO dto)
    {
        var result = await _service.RegistrarUsoEmConsulta(dto);
        return StatusCode(result.StatusCode, result);
    }
}