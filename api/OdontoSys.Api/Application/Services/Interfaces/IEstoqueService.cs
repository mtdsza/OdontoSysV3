using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IEstoqueService
{
    Task<ApplicationResult<long>> CriarItem(ItemEstoqueDTO dto);
    Task<ApplicationResult<ItemEstoqueDTO>> AtualizarItem(ItemEstoqueDTO dto); 
    
    Task<ApplicationResult<ItemEstoqueDTO>> ExcluirItem(long id);

    Task<ApplicationResult<IEnumerable<ItemEstoqueDTO>>> BuscarTodos();
    
    Task<ApplicationResult<long>> RegistrarMovimentacao(MovimentacaoEstoqueDTO dto);
    Task<ApplicationResult<long>> RegistrarUsoEmConsulta(RegistrarUsoDTO dto);
}