using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IFuncionarioService
{
    Task<ApplicationResult<long>> Inserir(FuncionarioDTO dto);
    Task<ApplicationResult<FuncionarioDTO>> Atualizar(FuncionarioDTO dto);
    Task<ApplicationResult<FuncionarioDTO>> Excluir(long id);
    Task<ApplicationResult<IEnumerable<FuncionarioDTO>>> BuscarTodos();
    Task<ApplicationResult<FuncionarioDTO>> BuscarPorId(long id);
}