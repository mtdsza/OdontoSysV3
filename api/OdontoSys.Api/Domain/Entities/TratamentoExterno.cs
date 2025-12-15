using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public class TratamentoExterno
{
    [Key]
    public long IdTratamentoExterno { get; set; }

    [Required]
    [MaxLength(255)]
    public string Descricao { get; set; } = string.Empty;

    public DateOnly? DataRealizacao { get; set; }
    
    [MaxLength(255)]
    public string? Local { get; set; }
    
    public string? Observacoes { get; set; }
    [ForeignKey("Paciente")]
    public long IdPaciente { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}