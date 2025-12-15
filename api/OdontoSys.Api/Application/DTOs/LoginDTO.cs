using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
}