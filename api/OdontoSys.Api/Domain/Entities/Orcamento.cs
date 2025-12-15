using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public class Orcamento
{
    [Key]
    public long IdOrcamento { get; set; }

    public DateOnly DataEmissao { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    [DataType(DataType.Currency)]
    public decimal ValorTotal { get; set; } = 0;

    [ForeignKey("Paciente")]
    public long IdPaciente { get; set; }
    public Paciente Paciente { get; set; } = null!;

    [ForeignKey("Dentista")]
    public long IdDentista { get; set; }
    public Dentista Dentista { get; set; } = null!;

    public bool Ativo { get; set; } = true;
    public bool Aprovado { get; set; } = false;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public ICollection<OrcamentoItem> Itens { get; set; } = new List<OrcamentoItem>();
}