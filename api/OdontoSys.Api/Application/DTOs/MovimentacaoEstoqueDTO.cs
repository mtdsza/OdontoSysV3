using System.ComponentModel.DataAnnotations;
using OdontoSys.Api.Domain.Entities;

namespace OdontoSys.Api.Application.DTOs;

public class MovimentacaoEstoqueDTO
{
    [Required]
    public long IdItemEstoque { get; set; }

    [Range(0.01, 99999, ErrorMessage = "Quantidade deve ser maior que zero.")]
    public decimal Quantidade { get; set; }

    public TipoMovimentacaoEstoque Tipo { get; set; }
    public string? Justificativa { get; set; }
}