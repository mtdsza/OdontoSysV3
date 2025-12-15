using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IDentistaService
{
    Task<ApplicationResult<long>> Inserir(DentistaDTO dto);
    Task<ApplicationResult<DentistaDTO>> BuscarPorId(long id);
    Task<ApplicationResult<IEnumerable<DentistaDTO>>> BuscarTodos();
    Task<ApplicationResult<DentistaDTO>> Atualizar(DentistaDTO dto);
    Task<ApplicationResult<DentistaDTO>> Excluir(long id);
    Task<ApplicationResult<DentistaDTO>> BuscarPorUserId(string userId);
}