using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class DentistaDTO : FuncionarioDTO
{
    public long IdDentista { get; set; }

    [Required(ErrorMessage = "O CRO é obrigatório.")]
    [StringLength(15, ErrorMessage = "Tamanho máximo do CRO é 15.")]
    public string Cro { get; set; } = string.Empty;
}