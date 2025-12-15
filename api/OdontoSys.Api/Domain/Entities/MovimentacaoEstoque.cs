using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public enum TipoMovimentacaoEstoque
{
    Entrada = 0,
    Perda = 1,
    Ajuste = 2
}

public class MovimentacaoEstoque
{
    [Key]
    public long IdMovimentacaoGeral { get; set; }

    public decimal Quantidade { get; set; }

    public TipoMovimentacaoEstoque Tipo { get; set; }

    [MaxLength(255)]
    public string? Justificativa { get; set; }

    [ForeignKey("ItemEstoque")]
    public long IdItemEstoque { get; set; }
    public ItemEstoque ItemEstoque { get; set; } = null!;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}