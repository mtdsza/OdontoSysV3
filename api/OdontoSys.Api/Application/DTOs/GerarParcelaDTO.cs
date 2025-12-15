using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class GerarParcelasDTO
{
    [Required]
    public long IdOrcamento { get; set; }
    
    [Range(1, 48)]
    public int QuantidadeParcelas { get; set; } = 1;
    
    public DateOnly PrimeiroVencimento { get; set; }
}