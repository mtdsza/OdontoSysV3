using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class ChangePasswordDTO
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;
}

public class AdminResetPasswordDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;
}