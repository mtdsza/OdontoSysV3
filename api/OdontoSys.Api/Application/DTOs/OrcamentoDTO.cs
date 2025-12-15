using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class OrcamentoDTO
{
    public long IdOrcamento { get; set; }
    public DateOnly DataEmissao { get; set; }

    [Required]
    public long IdPaciente { get; set; }
    public string? NomePaciente { get; set; }

    [Required]
    public long IdDentista { get; set; }
    public string? NomeDentista { get; set; }

    public decimal ValorTotal { get; set; }
    public bool Aprovado { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "O orçamento deve ter pelo menos 1 procedimento.")]
    public List<OrcamentoItemDTO> Itens { get; set; } = new();
}