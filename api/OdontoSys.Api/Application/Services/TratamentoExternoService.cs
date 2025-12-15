using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;
public class TratamentoExternoService : ITratamentoExternoService
{
    private readonly IGenericRepository<TratamentoExterno> _tratamentoRepository;
    private readonly IGenericRepository<Paciente> _pacienteRepository;

    public TratamentoExternoService(
        IGenericRepository<TratamentoExterno> tratamentoRepository,
        IGenericRepository<Paciente> pacienteRepository)
    {
        _tratamentoRepository = tratamentoRepository;
        _pacienteRepository = pacienteRepository;
    }

    public async Task<ApplicationResult<long>> Inserir(TratamentoExternoDTO dto)
    {
        var paciente = await _pacienteRepository.GetByIdAsync(dto.IdPaciente);
        if (paciente == null)
            return ApplicationResult<long>.Failure("Paciente não encontrado.", 404);

        var tratamento = new TratamentoExterno
        {
            Descricao = dto.Descricao,
            DataRealizacao = dto.DataRealizacao,
            Local = dto.Local,
            Observacoes = dto.Observacoes,
            IdPaciente = dto.IdPaciente
        };

        await _tratamentoRepository.AddAsync(tratamento);
        return ApplicationResult<long>.Success(tratamento.IdTratamentoExterno, 201, "Histórico adicionado.");
    }

    public async Task<ApplicationResult<TratamentoExternoDTO>> Atualizar(TratamentoExternoDTO dto)
    {
        var tratamento = await _tratamentoRepository.GetByIdAsync(dto.IdTratamentoExterno);
        if (tratamento == null)
            return ApplicationResult<TratamentoExternoDTO>.Failure("Registro não encontrado.", 404);

        tratamento.Descricao = dto.Descricao;
        tratamento.DataRealizacao = dto.DataRealizacao;
        tratamento.Local = dto.Local;
        tratamento.Observacoes = dto.Observacoes;

        await _tratamentoRepository.UpdateAsync(tratamento);
        return ApplicationResult<TratamentoExternoDTO>.Success(dto, 200, "Histórico atualizado.");
    }

    public async Task<ApplicationResult<TratamentoExternoDTO>> Excluir(long id)
    {
        var tratamento = await _tratamentoRepository.GetByIdAsync(id);
        if (tratamento == null)
            return ApplicationResult<TratamentoExternoDTO>.Failure("Registro não encontrado.", 404);

        await _tratamentoRepository.DeleteAsync(id);
        return ApplicationResult<TratamentoExternoDTO>.Success(new TratamentoExternoDTO { IdTratamentoExterno = id }, 200, "Registro removido.");
    }

    public async Task<ApplicationResult<TratamentoExternoDTO>> BuscarPorId(long id)
    {
        var t = await _tratamentoRepository.GetByIdAsync(id);
        if (t == null) return ApplicationResult<TratamentoExternoDTO>.Failure("Não encontrado", 404);

        var p = await _pacienteRepository.GetByIdAsync(t.IdPaciente);

        var dto = new TratamentoExternoDTO
        {
            IdTratamentoExterno = t.IdTratamentoExterno,
            Descricao = t.Descricao,
            DataRealizacao = t.DataRealizacao,
            Local = t.Local,
            Observacoes = t.Observacoes,
            IdPaciente = t.IdPaciente,
            NomePaciente = p?.Nome ?? "Desconhecido"
        };

        return ApplicationResult<TratamentoExternoDTO>.Success(dto);
    }

    public async Task<ApplicationResult<IEnumerable<TratamentoExternoDTO>>> BuscarPorPaciente(long idPaciente)
    {
        var lista = await _tratamentoRepository.FindAsync(t => t.IdPaciente == idPaciente);
        
        var paciente = await _pacienteRepository.GetByIdAsync(idPaciente);
        var nomePaciente = paciente?.Nome ?? "Desconhecido";

        var dtos = lista.Select(t => new TratamentoExternoDTO
        {
            IdTratamentoExterno = t.IdTratamentoExterno,
            Descricao = t.Descricao,
            DataRealizacao = t.DataRealizacao,
            Local = t.Local,
            Observacoes = t.Observacoes,
            IdPaciente = t.IdPaciente,
            NomePaciente = nomePaciente
        });

        return ApplicationResult<IEnumerable<TratamentoExternoDTO>>.Success(dtos);
    }
}