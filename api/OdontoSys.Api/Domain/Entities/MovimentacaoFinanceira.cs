using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public enum TipoMovimentacao
{
    Entrada = 0,
    Saida = 1
}

public class MovimentacaoFinanceira
{
    [Key]
    public long IdMovimentacao { get; set; }

    [Required]
    [MaxLength(255)]
    public string Descricao { get; set; } = string.Empty;

    [DataType(DataType.Currency)]
    public decimal Valor { get; set; }

    public TipoMovimentacao Tipo { get; set; }

    [ForeignKey("Parcela")]
    public long? IdParcelaPaga { get; set; }
    public Parcela? Parcela { get; set; }

    public DateTime DataMovimentacao { get; set; } = DateTime.Now;
}