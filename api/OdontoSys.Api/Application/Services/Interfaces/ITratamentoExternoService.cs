using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface ITratamentoExternoService
{
    Task<ApplicationResult<long>> Inserir(TratamentoExternoDTO dto);
    Task<ApplicationResult<TratamentoExternoDTO>> Atualizar(TratamentoExternoDTO dto);
    Task<ApplicationResult<TratamentoExternoDTO>> Excluir(long id);
    Task<ApplicationResult<TratamentoExternoDTO>> BuscarPorId(long id);
    Task<ApplicationResult<IEnumerable<TratamentoExternoDTO>>> BuscarPorPaciente(long idPaciente);
}