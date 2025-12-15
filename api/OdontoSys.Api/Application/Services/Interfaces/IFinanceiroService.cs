using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IFinanceiroService
{
    Task<ApplicationResult<IEnumerable<ParcelaDTO>>> GerarParcelas(GerarParcelasDTO dto);
    
    Task<ApplicationResult<IEnumerable<ParcelaDTO>>> BuscarParcelasPorOrcamento(long idOrcamento);
    
    Task<ApplicationResult<string>> PagarParcela(long idParcela);
    
    Task<ApplicationResult<string>> CancelarParcelasDoOrcamento(long idOrcamento);
    
    Task<ApplicationResult<IEnumerable<MovimentacaoDTO>>> BuscarMovimentacoes();
    
    Task<ApplicationResult<long>> RegistrarDespesa(MovimentacaoDTO dto);
}