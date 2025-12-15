using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IPacienteService
{
    Task<ApplicationResult<long>> Inserir(PacienteDTO pacienteDto);
    
    Task<ApplicationResult<PacienteDTO>> Atualizar(PacienteDTO pacienteDto);
    
    Task<ApplicationResult<PacienteDTO>> Excluir(long id);
    
    Task<ApplicationResult<IEnumerable<PacienteDTO>>> BuscarTodos();
    
    Task<ApplicationResult<PacienteDTO>> BuscarPorId(long id);
    
    Task<ApplicationResult<IEnumerable<PacienteDTO>>> BuscarPorNome(string nome);
}