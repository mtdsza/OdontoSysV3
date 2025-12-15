using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public enum SituacaoConsulta
{
    Agendada = 0,
    Realizada = 1,
    Cancelada = 2
}

public class Consulta
{
    [Key]
    public long IdConsulta { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    
    [MaxLength(500)]
    public string? Descricao { get; set; }
    
    public SituacaoConsulta Situacao { get; set; } = SituacaoConsulta.Agendada;

    [ForeignKey("Paciente")]
    public long IdPaciente { get; set; }
    public Paciente Paciente { get; set; } = null!;

    [ForeignKey("Dentista")]
    public long IdDentista { get; set; }
    public Dentista Dentista { get; set; } = null!;

    public bool Ativo { get; set; } = true;

    public string? Diagnostico { get; set; }
    public string? Prescricao { get; set; }
    public string? ObservacoesDaConsulta { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}