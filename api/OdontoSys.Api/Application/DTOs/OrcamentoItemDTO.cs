using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class OrcamentoItemDTO
{
    public long IdOrcamentoItem { get; set; }
    
    [Required]
    public long IdProcedimento { get; set; }
    public string? NomeProcedimento { get; set; }

    [Range(1, 100)]
    public int Quantidade { get; set; } = 1;

    public decimal? ValorUnitario { get; set; } 
    public decimal ValorTotalItem => (ValorUnitario ?? 0) * Quantidade;
}