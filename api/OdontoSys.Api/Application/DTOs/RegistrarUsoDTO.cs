using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class RegistrarUsoDTO
{
    [Required]
    public long IdConsulta { get; set; }
    
    [Required]
    public long IdItemEstoque { get; set; }
    
    [Range(0.01, 99999)]
    public decimal Quantidade { get; set; }
}