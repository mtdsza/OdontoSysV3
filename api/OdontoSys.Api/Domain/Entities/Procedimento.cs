using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Domain.Entities;

public class Procedimento
{
    [Key]
    public long IdProcedimento { get; set; }

    [Required]
    [MaxLength(255)]
    public string Nome { get; set; } = string.Empty;

    [DataType(DataType.Currency)]
    public decimal ValorPadrao { get; set; } = 0.00m;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}