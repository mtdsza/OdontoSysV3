using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class CriarUsuarioVinculadoDTO
{
    [Required]
    public long IdFuncionario { get; set; }

    [Required]
    [EmailAddress]
    public string EmailLogin { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public string Perfil { get; set; } = "Secretaria";
}