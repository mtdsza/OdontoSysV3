using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class ItemEstoqueDTO
{
    public long IdItemEstoque { get; set; }

    [Required]
    public string Descricao { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }
    public decimal EstoqueMin { get; set; }
}