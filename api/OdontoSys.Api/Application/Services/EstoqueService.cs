using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IGenericRepository<ItemEstoque> _itemRepo;
    private readonly IGenericRepository<MovimentacaoEstoque> _movRepo;
    private readonly IGenericRepository<UsoMaterial> _usoRepo;
    private readonly IGenericRepository<Consulta> _consultaRepo;

    public EstoqueService(
        IGenericRepository<ItemEstoque> itemRepo,
        IGenericRepository<MovimentacaoEstoque> movRepo,
        IGenericRepository<UsoMaterial> usoRepo,
        IGenericRepository<Consulta> consultaRepo)
    {
        _itemRepo = itemRepo;
        _movRepo = movRepo;
        _usoRepo = usoRepo;
        _consultaRepo = consultaRepo;
    }

    public async Task<ApplicationResult<long>> CriarItem(ItemEstoqueDTO dto)
    {
        var item = new ItemEstoque
        {
            Descricao = dto.Descricao,
            EstoqueMin = dto.EstoqueMin,
            Quantidade = 0 
        };
        await _itemRepo.AddAsync(item);
        return ApplicationResult<long>.Success(item.IdItemEstoque, 201, "Material cadastrado.");
    }

    public async Task<ApplicationResult<ItemEstoqueDTO>> AtualizarItem(ItemEstoqueDTO dto)
    {
        var item = await _itemRepo.GetByIdAsync(dto.IdItemEstoque);
        if (item == null) return ApplicationResult<ItemEstoqueDTO>.Failure("Não encontrado", 404);

        item.Descricao = dto.Descricao;
        item.EstoqueMin = dto.EstoqueMin;
        item.DataAtualizacao = DateTime.Now;

        await _itemRepo.UpdateAsync(item);
        return ApplicationResult<ItemEstoqueDTO>.Success(dto, 200, "Dados atualizados.");
    }

    public async Task<ApplicationResult<ItemEstoqueDTO>> ExcluirItem(long id)
    {
        var item = await _itemRepo.GetByIdAsync(id);
        if (item == null) 
            return ApplicationResult<ItemEstoqueDTO>.Failure("Item de estoque não encontrado.", 404);

        try 
        {
            await _itemRepo.DeleteAsync(id);
            
            var dtoRetorno = new ItemEstoqueDTO 
            { 
                IdItemEstoque = item.IdItemEstoque, 
                Descricao = item.Descricao 
            };
            
            return ApplicationResult<ItemEstoqueDTO>.Success(dtoRetorno, 200, "Item excluído com sucesso.");
        }
        catch (Exception ex)
        {
            return ApplicationResult<ItemEstoqueDTO>.Failure($"Erro ao excluir item. Verifique se ele não possui movimentações ou usos vinculados. Detalhe: {ex.Message}", 500);
        }
    }

    public async Task<ApplicationResult<IEnumerable<ItemEstoqueDTO>>> BuscarTodos()
    {
        var lista = await _itemRepo.GetAllAsync();
        var dtos = lista.Select(i => new ItemEstoqueDTO
        {
            IdItemEstoque = i.IdItemEstoque,
            Descricao = i.Descricao,
            Quantidade = i.Quantidade,
            EstoqueMin = i.EstoqueMin
        });
        return ApplicationResult<IEnumerable<ItemEstoqueDTO>>.Success(dtos);
    }

    public async Task<ApplicationResult<long>> RegistrarMovimentacao(MovimentacaoEstoqueDTO dto)
    {
        var item = await _itemRepo.GetByIdAsync(dto.IdItemEstoque);
        if (item == null) return ApplicationResult<long>.Failure("Item não encontrado", 404);
        
        if (dto.Tipo == TipoMovimentacaoEstoque.Entrada)
        {
            item.Quantidade += dto.Quantidade;
        }
        else
        {
            item.Quantidade -= dto.Quantidade;
        }

        var mov = new MovimentacaoEstoque
        {
            IdItemEstoque = dto.IdItemEstoque,
            Quantidade = dto.Quantidade,
            Tipo = dto.Tipo,
            Justificativa = dto.Justificativa
        };

        await _movRepo.AddAsync(mov);
        await _itemRepo.UpdateAsync(item);

        return ApplicationResult<long>.Success(mov.IdMovimentacaoGeral, 201, "Estoque atualizado.");
    }

    public async Task<ApplicationResult<long>> RegistrarUsoEmConsulta(RegistrarUsoDTO dto)
    {
        var item = await _itemRepo.GetByIdAsync(dto.IdItemEstoque);
        if (item == null) return ApplicationResult<long>.Failure("Item não encontrado", 404);

        var consulta = await _consultaRepo.GetByIdAsync(dto.IdConsulta);
        if (consulta == null) return ApplicationResult<long>.Failure("Consulta não encontrada", 404);

        item.Quantidade -= dto.Quantidade;

        var uso = new UsoMaterial
        {
            IdConsulta = dto.IdConsulta,
            IdItemEstoque = dto.IdItemEstoque,
            Quantidade = dto.Quantidade
        };

        await _usoRepo.AddAsync(uso);
        await _itemRepo.UpdateAsync(item);

        return ApplicationResult<long>.Success(uso.IdUsoMaterial, 201, "Material consumido na consulta.");
    }
}