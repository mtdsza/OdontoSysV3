using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class ProcedimentoDTO
{
    public long IdProcedimento { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(255)]
    public string Nome { get; set; } = string.Empty;

    [Range(0, 999999, ErrorMessage = "O valor deve ser positivo.")]
    public decimal ValorPadrao { get; set; }
}