using System.Linq.Expressions;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class PacienteService : IPacienteService
{
    private readonly IGenericRepository<Paciente> _repository;

    public PacienteService(IGenericRepository<Paciente> repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationResult<long>> Inserir(PacienteDTO dto)
    {
        if (dto == null)
            return ApplicationResult<long>.Failure("Dados inválidos", 400);

        var paciente = new Paciente
        {
            Nome = dto.Nome,
            Cpf = dto.Cpf,
            Nascimento = dto.Nascimento,
            Telefone = dto.Telefone,
            Email = dto.Email,
            Ativo = dto.Ativo,
            Cep = dto.Cep,
            Logradouro = dto.Logradouro,
            Numero = dto.Numero,
            Complemento = dto.Complemento,
            Bairro = dto.Bairro,
            Cidade = dto.Cidade,
            Estado = dto.Estado,
            CondicoesMedicas = dto.CondicoesMedicas,
            ObservacoesGerais = dto.ObservacoesGerais
        };

        try
        {
            await _repository.AddAsync(paciente);
            return ApplicationResult<long>.Success(paciente.IdPaciente, 201, "Paciente cadastrado com sucesso!");
        }
        catch (Exception ex)
        {
            return ApplicationResult<long>.Failure($"Erro ao salvar: {ex.Message}", 500);
        }
    }

    public async Task<ApplicationResult<PacienteDTO>> Atualizar(PacienteDTO dto)
    {
        if (dto == null || dto.IdPaciente <= 0)
            return ApplicationResult<PacienteDTO>.Failure("Dados inválidos", 400);

        var pacienteExistente = await _repository.GetByIdAsync(dto.IdPaciente);
        if (pacienteExistente == null)
            return ApplicationResult<PacienteDTO>.Failure("Paciente não encontrado.", 404);

        pacienteExistente.Nome = dto.Nome;
        pacienteExistente.Cpf = dto.Cpf;
        pacienteExistente.Nascimento = dto.Nascimento;
        pacienteExistente.Telefone = dto.Telefone;
        pacienteExistente.Email = dto.Email;
        pacienteExistente.Ativo = dto.Ativo;
        
        pacienteExistente.Cep = dto.Cep;
        pacienteExistente.Logradouro = dto.Logradouro;
        pacienteExistente.Numero = dto.Numero;
        pacienteExistente.Complemento = dto.Complemento;
        pacienteExistente.Bairro = dto.Bairro;
        pacienteExistente.Cidade = dto.Cidade;
        pacienteExistente.Estado = dto.Estado;
        pacienteExistente.CondicoesMedicas = dto.CondicoesMedicas;
        pacienteExistente.ObservacoesGerais = dto.ObservacoesGerais;
        
        pacienteExistente.DataAtualizacao = DateTime.Now;

        await _repository.UpdateAsync(pacienteExistente);

        return ApplicationResult<PacienteDTO>.Success(dto, 200, "Dados atualizados com sucesso.");
    }

    public async Task<ApplicationResult<PacienteDTO>> Excluir(long id)
    {
        if (id <= 0)
            return ApplicationResult<PacienteDTO>.Failure("ID inválido", 400);

        var paciente = await _repository.GetByIdAsync(id);
        if (paciente == null)
            return ApplicationResult<PacienteDTO>.Failure("Paciente não encontrado.", 404);

        await _repository.DeleteAsync(id);

        var dto = new PacienteDTO { IdPaciente = paciente.IdPaciente, Nome = paciente.Nome };
        return ApplicationResult<PacienteDTO>.Success(dto, 200, "Paciente excluído.");
    }

    public async Task<ApplicationResult<IEnumerable<PacienteDTO>>> BuscarTodos()
    {
        var pacientes = await _repository.GetAllAsync();

        var dtos = pacientes.Select(p => new PacienteDTO
        {
            IdPaciente = p.IdPaciente,
            Nome = p.Nome,
            Cpf = p.Cpf,
            Nascimento = p.Nascimento,
            Telefone = p.Telefone,
            Email = p.Email,
            Ativo = p.Ativo,
            Cidade = p.Cidade,
            Estado = p.Estado
        });

        return ApplicationResult<IEnumerable<PacienteDTO>>.Success(dtos);
    }

    public async Task<ApplicationResult<PacienteDTO>> BuscarPorId(long id)
    {
        var p = await _repository.GetByIdAsync(id);
        if (p == null)
            return ApplicationResult<PacienteDTO>.Failure("Paciente não encontrado", 404);

        var dto = new PacienteDTO
        {
            IdPaciente = p.IdPaciente,
            Nome = p.Nome,
            Cpf = p.Cpf,
            Nascimento = p.Nascimento,
            Telefone = p.Telefone,
            Email = p.Email,
            Ativo = p.Ativo,
            Cep = p.Cep,
            Logradouro = p.Logradouro,
            Numero = p.Numero,
            Complemento = p.Complemento,
            Bairro = p.Bairro,
            Cidade = p.Cidade,
            Estado = p.Estado,
            CondicoesMedicas = p.CondicoesMedicas,
            ObservacoesGerais = p.ObservacoesGerais
        };

        return ApplicationResult<PacienteDTO>.Success(dto);
    }

    public async Task<ApplicationResult<IEnumerable<PacienteDTO>>> BuscarPorNome(string nome)
    {
        var pacientes = await _repository.FindAsync(p => p.Nome.Contains(nome));

        var dtos = pacientes.Select(p => new PacienteDTO
        {
            IdPaciente = p.IdPaciente,
            Nome = p.Nome,
            Cpf = p.Cpf,
        });

        return ApplicationResult<IEnumerable<PacienteDTO>>.Success(dtos);
    }
}