using System.ComponentModel.DataAnnotations;
using OdontoSys.Api.Domain.Entities;

namespace OdontoSys.Api.Application.DTOs;

public class ConsultaDTO
{
    public long IdConsulta { get; set; }

    [Required(ErrorMessage = "A data e hora de início são obrigatórias.")]
    public DateTime DataInicio { get; set; }

    public DateTime? DataFim { get; set; }

    [StringLength(500)]
    public string? Descricao { get; set; }
    
    public SituacaoConsulta Situacao { get; set; }

    [Required]
    public long IdPaciente { get; set; }
    public string? NomePaciente { get; set; }

    [Required]
    public long IdDentista { get; set; }
    public string? NomeDentista { get; set; }
    
    public string? Diagnostico { get; set; }
    public string? Prescricao { get; set; }
}