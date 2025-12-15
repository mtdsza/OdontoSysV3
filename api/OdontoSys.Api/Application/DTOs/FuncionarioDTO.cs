using System.ComponentModel.DataAnnotations;
using OdontoSys.Api.Domain.Entities;

namespace OdontoSys.Api.Application.DTOs;

public class FuncionarioDTO
{
    public long IdFuncionario { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(255)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    public string? Senha { get; set; } 


    [StringLength(13)]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; } = true;

    public DateOnly? DataContratacao { get; set; }

    public decimal? SalarioBase { get; set; }

    [Required]
    public TipoFuncionario Tipo { get; set; }
}