using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class FuncionarioService : IFuncionarioService
{
    private readonly IGenericRepository<Funcionario> _repository;
    
    public FuncionarioService(IGenericRepository<Funcionario> repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationResult<long>> Inserir(FuncionarioDTO dto)
    {
        var existentes = await _repository.FindAsync(f => f.Email == dto.Email);
        if (existentes.Any())
            return ApplicationResult<long>.Failure("Já existe um funcionário cadastrado com este e-mail de contato.", 409);

        var funcionario = new Funcionario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Ativo = dto.Ativo,
            DataContratacao = dto.DataContratacao,
            SalarioBase = dto.SalarioBase,
            Tipo = dto.Tipo,
            UserId = null
        };

        await _repository.AddAsync(funcionario);
        return ApplicationResult<long>.Success(funcionario.IdFuncionario, 201, "Ficha de funcionário criada com sucesso.");
    }

    public async Task<ApplicationResult<FuncionarioDTO>> Atualizar(FuncionarioDTO dto)
    {
        var funcionario = await _repository.GetByIdAsync(dto.IdFuncionario);
        if (funcionario == null)
            return ApplicationResult<FuncionarioDTO>.Failure("Funcionário não encontrado.", 404);
        
        funcionario.Nome = dto.Nome;
        funcionario.Email = dto.Email;
        funcionario.Telefone = dto.Telefone;
        funcionario.Ativo = dto.Ativo;
        funcionario.Tipo = dto.Tipo;
        funcionario.SalarioBase = dto.SalarioBase;
        funcionario.DataContratacao = dto.DataContratacao;

        await _repository.UpdateAsync(funcionario);
        return ApplicationResult<FuncionarioDTO>.Success(dto, 200, "Dados do funcionário atualizados.");
    }

    public async Task<ApplicationResult<FuncionarioDTO>> Excluir(long id)
    {
        var funcionario = await _repository.GetByIdAsync(id);
        if (funcionario == null)
            return ApplicationResult<FuncionarioDTO>.Failure("Funcionário não encontrado.", 404);

        await _repository.DeleteAsync(id);
        
        return ApplicationResult<FuncionarioDTO>.Success(new FuncionarioDTO { IdFuncionario = id }, 200, "Ficha do funcionário removida.");
    }

    public async Task<ApplicationResult<IEnumerable<FuncionarioDTO>>> BuscarTodos()
    {
        var lista = await _repository.GetAllAsync();
        var dtos = lista.Select(f => new FuncionarioDTO
        {
            IdFuncionario = f.IdFuncionario,
            Nome = f.Nome,
            Email = f.Email,
            Telefone = f.Telefone,
            Ativo = f.Ativo,
            DataContratacao = f.DataContratacao,
            SalarioBase = f.SalarioBase,
            Tipo = f.Tipo
        });
        return ApplicationResult<IEnumerable<FuncionarioDTO>>.Success(dtos);
    }

    public async Task<ApplicationResult<FuncionarioDTO>> BuscarPorId(long id)
    {
        var f = await _repository.GetByIdAsync(id);
        if (f == null) return ApplicationResult<FuncionarioDTO>.Failure("Não encontrado", 404);

        return ApplicationResult<FuncionarioDTO>.Success(new FuncionarioDTO
        {
            IdFuncionario = f.IdFuncionario,
            Nome = f.Nome,
            Email = f.Email,
            Telefone = f.Telefone,
            Ativo = f.Ativo,
            Tipo = f.Tipo,
            SalarioBase = f.SalarioBase,
            DataContratacao = f.DataContratacao
        });
    }
}