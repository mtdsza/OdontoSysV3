using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Application.DTOs;

public class PacienteDTO
{
    public long IdPaciente { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter 11 dígitos.")]
    public string Cpf { get; set; } = string.Empty;

    [Required]
    public DateOnly Nascimento { get; set; }

    [StringLength(13)]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(255)]
    public string? Email { get; set; }

    public bool Ativo { get; set; } = true;
    
    [StringLength(9)]
    public string? Cep { get; set; }
    
    [StringLength(255)]
    public string? Logradouro { get; set; }
    
    [StringLength(10)]
    public string? Numero { get; set; }
    
    [StringLength(100)]
    public string? Complemento { get; set; }
    
    [StringLength(100)]
    public string? Bairro { get; set; }
    
    [StringLength(100)]
    public string? Cidade { get; set; }
    
    [StringLength(2)]
    public string? Estado { get; set; }
    
    public string? CondicoesMedicas { get; set; }
    public string? ObservacoesGerais { get; set; }
}