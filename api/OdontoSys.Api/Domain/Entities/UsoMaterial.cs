using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public class UsoMaterial
{
    [Key]
    public long IdUsoMaterial { get; set; }

    public decimal Quantidade { get; set; }

    [ForeignKey("Consulta")]
    public long IdConsulta { get; set; }
    public Consulta Consulta { get; set; } = null!;

    [ForeignKey("ItemEstoque")]
    public long IdItemEstoque { get; set; }
    public ItemEstoque ItemEstoque { get; set; } = null!;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}