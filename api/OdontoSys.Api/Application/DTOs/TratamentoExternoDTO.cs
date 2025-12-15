using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class TratamentoExternoDTO
{
    public long IdTratamentoExterno { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(255)]
    public string Descricao { get; set; } = string.Empty;

    public DateOnly? DataRealizacao { get; set; }
    
    [StringLength(255)]
    public string? Local { get; set; }
    
    public string? Observacoes { get; set; }

    [Required]
    public long IdPaciente { get; set; }
    
    public string? NomePaciente { get; set; }
}