using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class ProcedimentoService : IProcedimentoService
{
    private readonly IGenericRepository<Procedimento> _repository;

    public ProcedimentoService(IGenericRepository<Procedimento> repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationResult<long>> Inserir(ProcedimentoDTO dto)
    {
        var procedimento = new Procedimento
        {
            Nome = dto.Nome,
            ValorPadrao = dto.ValorPadrao
        };

        await _repository.AddAsync(procedimento);
        return ApplicationResult<long>.Success(procedimento.IdProcedimento, 201, "Procedimento cadastrado.");
    }

    public async Task<ApplicationResult<ProcedimentoDTO>> Atualizar(ProcedimentoDTO dto)
    {
        var proc = await _repository.GetByIdAsync(dto.IdProcedimento);
        if (proc == null) return ApplicationResult<ProcedimentoDTO>.Failure("Não encontrado.", 404);

        proc.Nome = dto.Nome;
        proc.ValorPadrao = dto.ValorPadrao;
        proc.DataAtualizacao = DateTime.Now;

        await _repository.UpdateAsync(proc);
        return ApplicationResult<ProcedimentoDTO>.Success(dto, 200, "Atualizado.");
    }

    public async Task<ApplicationResult<ProcedimentoDTO>> Excluir(long id)
    {
        var proc = await _repository.GetByIdAsync(id);
        if (proc == null) return ApplicationResult<ProcedimentoDTO>.Failure("Não encontrado.", 404);

        await _repository.DeleteAsync(id);
        return ApplicationResult<ProcedimentoDTO>.Success(new ProcedimentoDTO { IdProcedimento = id }, 200, "Excluído.");
    }

    public async Task<ApplicationResult<IEnumerable<ProcedimentoDTO>>> BuscarTodos()
    {
        var lista = await _repository.GetAllAsync();
        var dtos = lista.Select(p => new ProcedimentoDTO 
        { 
            IdProcedimento = p.IdProcedimento, 
            Nome = p.Nome, 
            ValorPadrao = p.ValorPadrao 
        });
        return ApplicationResult<IEnumerable<ProcedimentoDTO>>.Success(dtos);
    }

    public async Task<ApplicationResult<ProcedimentoDTO>> BuscarPorId(long id)
    {
        var p = await _repository.GetByIdAsync(id);
        if (p == null) return ApplicationResult<ProcedimentoDTO>.Failure("Não encontrado", 404);

        return ApplicationResult<ProcedimentoDTO>.Success(new ProcedimentoDTO 
        { 
            IdProcedimento = p.IdProcedimento, 
            Nome = p.Nome, 
            ValorPadrao = p.ValorPadrao 
        });
    }
}