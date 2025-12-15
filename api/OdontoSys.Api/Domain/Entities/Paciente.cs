using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Domain.Entities;

public class Paciente
{
    [Key]
    public long IdPaciente { get; set; }

    [Required]
    [MaxLength(255)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(11)]
    public string Cpf { get; set; } = string.Empty;

    public DateOnly Nascimento { get; set; }

    [MaxLength(13)]
    public string? Telefone { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    public bool Ativo { get; set; } = true;
    
    [MaxLength(9)]
    public string? Cep { get; set; }
    [MaxLength(255)]
    public string? Logradouro { get; set; }
    [MaxLength(10)]
    public string? Numero { get; set; }
    [MaxLength(100)]
    public string? Complemento { get; set; }
    [MaxLength(100)]
    public string? Bairro { get; set; }
    [MaxLength(100)]
    public string? Cidade { get; set; }
    [MaxLength(2)]
    public string? Estado { get; set; }
    
    public string? CondicoesMedicas { get; set; }
    public string? ObservacoesGerais { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
    
}