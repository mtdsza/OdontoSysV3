using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OdontoSys.Api.Domain.Entities;

public class Dentista
{
    [Key]
    public long IdDentista { get; set; }

    [Required]
    [MaxLength(15)]
    public string Cro { get; set; } = string.Empty;

    [ForeignKey("Funcionario")]
    public long IdFuncionario { get; set; }
    public Funcionario Funcionario { get; set; } = null!;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;

}