using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;
public class OrcamentoService : IOrcamentoService
{
    private readonly IGenericRepository<Orcamento> _orcamentoRepo;
    private readonly IGenericRepository<OrcamentoItem> _itemRepo;
    private readonly IGenericRepository<Procedimento> _procedimentoRepo;
    private readonly IGenericRepository<Paciente> _pacienteRepo;
    private readonly IGenericRepository<Dentista> _dentistaRepo;

    public OrcamentoService(
        IGenericRepository<Orcamento> orcamentoRepo,
        IGenericRepository<OrcamentoItem> itemRepo,
        IGenericRepository<Procedimento> procedimentoRepo,
        IGenericRepository<Paciente> pacienteRepo,
        IGenericRepository<Dentista> dentistaRepo)
    {
        _orcamentoRepo = orcamentoRepo;
        _itemRepo = itemRepo;
        _procedimentoRepo = procedimentoRepo;
        _pacienteRepo = pacienteRepo;
        _dentistaRepo = dentistaRepo;
    }

    public async Task<ApplicationResult<long>> CriarOrcamento(OrcamentoDTO dto)
    {
        var paciente = await _pacienteRepo.GetByIdAsync(dto.IdPaciente);
        if (paciente == null) return ApplicationResult<long>.Failure("Paciente inválido", 404);
        
        var dentista = await _dentistaRepo.GetByIdAsync(dto.IdDentista);
        if (dentista == null) return ApplicationResult<long>.Failure("Dentista inválido", 404);

        var orcamento = new Orcamento
        {
            IdPaciente = dto.IdPaciente,
            IdDentista = dto.IdDentista,
            DataEmissao = DateOnly.FromDateTime(DateTime.Now),
            Aprovado = false,
            Ativo = true,
            ValorTotal = 0
        };

        await _orcamentoRepo.AddAsync(orcamento);

        decimal totalGeral = 0;

        foreach (var itemDto in dto.Itens)
        {
            var proc = await _procedimentoRepo.GetByIdAsync(itemDto.IdProcedimento);
            if (proc == null) continue; 

            decimal valorFinal = itemDto.ValorUnitario ?? proc.ValorPadrao;

            var item = new OrcamentoItem
            {
                IdOrcamento = orcamento.IdOrcamento,
                IdProcedimento = itemDto.IdProcedimento,
                Quantidade = itemDto.Quantidade,
                ValorUnitario = valorFinal
            };

            await _itemRepo.AddAsync(item);
            totalGeral += (valorFinal * itemDto.Quantidade);
        }

        orcamento.ValorTotal = totalGeral;
        await _orcamentoRepo.UpdateAsync(orcamento);

        return ApplicationResult<long>.Success(orcamento.IdOrcamento, 201, "Orçamento criado.");
    }

    public async Task<ApplicationResult<OrcamentoDTO>> AtualizarOrcamento(OrcamentoDTO dto)
    {
        var orcamento = await _orcamentoRepo.GetByIdAsync(dto.IdOrcamento);
        if (orcamento == null) 
            return ApplicationResult<OrcamentoDTO>.Failure("Orçamento não encontrado.", 404);

        if (orcamento.Aprovado)
            return ApplicationResult<OrcamentoDTO>.Failure("Não é possível alterar um orçamento já aprovado.", 400);

        orcamento.IdPaciente = dto.IdPaciente; 
        orcamento.IdDentista = dto.IdDentista;

        var itensAntigos = await _itemRepo.FindAsync(i => i.IdOrcamento == dto.IdOrcamento);
        foreach (var itemAntigo in itensAntigos)
        {
            await _itemRepo.DeleteAsync(itemAntigo.IdOrcamentoItem);
        }

        decimal totalGeral = 0;

        if (dto.Itens != null && dto.Itens.Any())
        {
            foreach (var itemDto in dto.Itens)
            {
                var proc = await _procedimentoRepo.GetByIdAsync(itemDto.IdProcedimento);
                if (proc == null) continue;

                decimal valorFinal = itemDto.ValorUnitario ?? proc.ValorPadrao;

                var novoItem = new OrcamentoItem
                {
                    IdOrcamento = orcamento.IdOrcamento,
                    IdProcedimento = itemDto.IdProcedimento,
                    Quantidade = itemDto.Quantidade,
                    ValorUnitario = valorFinal
                };

                await _itemRepo.AddAsync(novoItem);
                totalGeral += (valorFinal * itemDto.Quantidade);
            }
        }
        else
        {
             return ApplicationResult<OrcamentoDTO>.Failure("O orçamento deve ter pelo menos um item.", 400);
        }

        orcamento.ValorTotal = totalGeral;
        await _orcamentoRepo.UpdateAsync(orcamento);

        dto.ValorTotal = totalGeral;
        dto.Aprovado = false;

        return ApplicationResult<OrcamentoDTO>.Success(dto, 200, "Orçamento atualizado com sucesso.");
    }

    public async Task<ApplicationResult<OrcamentoDTO>> BuscarPorId(long id)
    {
        var orc = await _orcamentoRepo.GetByIdAsync(id);
        if (orc == null) return ApplicationResult<OrcamentoDTO>.Failure("Não encontrado", 404);

        var itens = await _itemRepo.FindAsync(i => i.IdOrcamento == id);
        
        var dto = new OrcamentoDTO
        {
            IdOrcamento = orc.IdOrcamento,
            DataEmissao = orc.DataEmissao,
            ValorTotal = orc.ValorTotal,
            IdPaciente = orc.IdPaciente,
            IdDentista = orc.IdDentista,
            Aprovado = orc.Aprovado,
            Itens = new List<OrcamentoItemDTO>()
        };

        foreach (var item in itens)
        {
            var proc = await _procedimentoRepo.GetByIdAsync(item.IdProcedimento);
            dto.Itens.Add(new OrcamentoItemDTO
            {
                IdOrcamentoItem = item.IdOrcamentoItem,
                IdProcedimento = item.IdProcedimento,
                NomeProcedimento = proc?.Nome ?? "N/A",
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario
            });
        }

        return ApplicationResult<OrcamentoDTO>.Success(dto);
    }

    public async Task<ApplicationResult<IEnumerable<OrcamentoDTO>>> BuscarPorPaciente(long idPaciente)
    {
        var lista = await _orcamentoRepo.FindAsync(o => o.IdPaciente == idPaciente);
        var dtos = lista.Select(o => new OrcamentoDTO 
        { 
            IdOrcamento = o.IdOrcamento,
            ValorTotal = o.ValorTotal,
            DataEmissao = o.DataEmissao,
            Aprovado = o.Aprovado
        });
        
        return ApplicationResult<IEnumerable<OrcamentoDTO>>.Success(dtos);
    }

    public async Task<ApplicationResult<long>> AprovarOrcamento(long id)
    {
        var orc = await _orcamentoRepo.GetByIdAsync(id);
        if (orc == null) return ApplicationResult<long>.Failure("Não encontrado", 404);

        if (orc.Aprovado) return ApplicationResult<long>.Failure("Já está aprovado.", 400);

        orc.Aprovado = true;
        await _orcamentoRepo.UpdateAsync(orc);

        return ApplicationResult<long>.Success(id, 200, "Orçamento aprovado!");
    }

    public async Task<ApplicationResult<long>> ExcluirOrcamento(long id)
    {
        var orcamento = await _orcamentoRepo.GetByIdAsync(id);
        if (orcamento == null) 
            return ApplicationResult<long>.Failure("Orçamento não encontrado.", 404);

        if (orcamento.Aprovado)
            return ApplicationResult<long>.Failure("Não é possível excluir um orçamento já aprovado. Operação negada.", 400);

        try
        {
            var itens = await _itemRepo.FindAsync(i => i.IdOrcamento == id);
            foreach (var item in itens)
            {
                await _itemRepo.DeleteAsync(item.IdOrcamentoItem);
            }

            await _orcamentoRepo.DeleteAsync(id);

            return ApplicationResult<long>.Success(id, 200, "Orçamento excluído com sucesso.");
        }
        catch (Exception ex)
        {
            return ApplicationResult<long>.Failure($"Erro ao excluir orçamento: {ex.Message}", 500);
        }
    }
}