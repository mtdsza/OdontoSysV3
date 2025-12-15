using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;
public interface IConsultaService
{
    Task<ApplicationResult<long>> Agendar(ConsultaDTO dto);
    Task<ApplicationResult<ConsultaDTO>> Atualizar(ConsultaDTO dto);
    Task<ApplicationResult<ConsultaDTO>> Cancelar(long id);
    
    Task<ApplicationResult<ConsultaDTO>> BuscarPorId(long id);
    Task<ApplicationResult<IEnumerable<ConsultaDTO>>> BuscarTodas();
    Task<ApplicationResult<IEnumerable<ConsultaDTO>>> BuscarPorDentista(long idDentista);
}