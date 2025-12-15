using OdontoSys.Api.Domain.Entities;

namespace OdontoSys.Api.Application.DTOs;

public class MovimentacaoDTO
{
    public long IdMovimentacao { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoMovimentacao Tipo { get; set; }
    public DateTime DataMovimentacao { get; set; }
}