namespace OdontoSys.Api.Application.DTOs;

public class ParcelaDTO
{
    public long IdParcela { get; set; }
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public DateOnly DataVencimento { get; set; }
    public int Status { get; set; }
    public DateTime? DataPagamento { get; set; }
    public long IdOrcamento { get; set; }
}