using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public enum StatusParcela
{
    Pendente = 0,
    Paga = 1,
    Vencida = 2,
    Cancelada = 3
}

public class Parcela
{
    [Key]
    public long IdParcela { get; set; }

    [MaxLength(255)]
    public string? Descricao { get; set; }

    [DataType(DataType.Currency)]
    public decimal Valor { get; set; }

    public DateOnly DataVencimento { get; set; }
    
    public StatusParcela Status { get; set; } = StatusParcela.Pendente;

    public DateTime? DataPagamento { get; set; }

    [ForeignKey("Orcamento")]
    public long IdOrcamento { get; set; }
    public Orcamento Orcamento { get; set; } = null!;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}