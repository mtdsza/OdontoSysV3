using System.ComponentModel.DataAnnotations;

namespace OdontoSys.Api.Domain.Entities;

public enum TipoFuncionario
{
    Dentista = 0,
    Secretaria = 1,
    Admin = 2,
    Auxiliar = 3,
    Outro = 4
}

public class Funcionario
{
    [Key]
    public long IdFuncionario { get; set; }

    [Required]
    [MaxLength(255)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(13)]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; } = true;

    public DateOnly? DataContratacao { get; set; }
    
    public decimal? SalarioBase { get; set; }

    public TipoFuncionario Tipo { get; set; }
    
    [MaxLength(450)] 
    public string? UserId { get; set; } 
    
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}