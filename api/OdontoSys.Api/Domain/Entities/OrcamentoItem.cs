using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public class OrcamentoItem
{
    [Key]
    public long IdOrcamentoItem { get; set; }

    [DataType(DataType.Currency)]
    public decimal ValorUnitario { get; set; }

    public int Quantidade { get; set; } = 1;

    [ForeignKey("Orcamento")]
    public long IdOrcamento { get; set; }

    [ForeignKey("Procedimento")]
    public long IdProcedimento { get; set; }
    public Procedimento Procedimento { get; set; } = null!;
}