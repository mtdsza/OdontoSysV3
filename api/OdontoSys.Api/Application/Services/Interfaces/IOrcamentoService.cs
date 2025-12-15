using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IOrcamentoService
{
    Task<ApplicationResult<long>> CriarOrcamento(OrcamentoDTO dto);
    Task<ApplicationResult<OrcamentoDTO>> AtualizarOrcamento(OrcamentoDTO dto);
    Task<ApplicationResult<OrcamentoDTO>> BuscarPorId(long id);
    Task<ApplicationResult<IEnumerable<OrcamentoDTO>>> BuscarPorPaciente(long idPaciente);
    Task<ApplicationResult<long>> AprovarOrcamento(long id);
    Task<ApplicationResult<long>> ExcluirOrcamento(long id);
}