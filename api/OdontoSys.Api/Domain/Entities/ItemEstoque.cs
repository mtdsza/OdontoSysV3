using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Domain.Entities;

public class ItemEstoque
{
    [Key]
    public long IdItemEstoque { get; set; }

    [Required]
    [MaxLength(255)]
    public string Descricao { get; set; } = string.Empty;

    public decimal Quantidade { get; set; } = 0;

    public decimal EstoqueMin { get; set; } = 0;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}