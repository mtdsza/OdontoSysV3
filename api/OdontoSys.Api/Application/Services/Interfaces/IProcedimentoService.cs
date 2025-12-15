using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IProcedimentoService
{
    Task<ApplicationResult<long>> Inserir(ProcedimentoDTO dto);
    Task<ApplicationResult<ProcedimentoDTO>> Atualizar(ProcedimentoDTO dto);
    Task<ApplicationResult<ProcedimentoDTO>> Excluir(long id);
    Task<ApplicationResult<IEnumerable<ProcedimentoDTO>>> BuscarTodos();
    Task<ApplicationResult<ProcedimentoDTO>> BuscarPorId(long id);
}