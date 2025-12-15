using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class DentistaService : IDentistaService
{
    private readonly IGenericRepository<Dentista> _dentistaRepository;
    private readonly IGenericRepository<Funcionario> _funcionarioRepository;

    public DentistaService(
        IGenericRepository<Dentista> dentistaRepository, 
        IGenericRepository<Funcionario> funcionarioRepository)
    {
        _dentistaRepository = dentistaRepository;
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task<ApplicationResult<long>> Inserir(DentistaDTO dto)
    {
        var existente = await _funcionarioRepository.FindAsync(f => f.Email == dto.Email);
        if (existente.Any())
            return ApplicationResult<long>.Failure("Email já cadastrado.", 409);
        
        var funcionario = new Funcionario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Ativo = dto.Ativo,
            DataContratacao = dto.DataContratacao,
            SalarioBase = dto.SalarioBase,
            Tipo = TipoFuncionario.Dentista,
            UserId = null
        };

        await _funcionarioRepository.AddAsync(funcionario);
        
        var dentista = new Dentista
        {
            Cro = dto.Cro,
            IdFuncionario = funcionario.IdFuncionario
        };

        try 
        {
            await _dentistaRepository.AddAsync(dentista);
            return ApplicationResult<long>.Success(dentista.IdDentista, 201, "Dentista cadastrado com sucesso.");
        }
        catch
        {
            await _funcionarioRepository.DeleteAsync(funcionario.IdFuncionario);
            throw;
        }
    }

    public async Task<ApplicationResult<DentistaDTO>> Atualizar(DentistaDTO dto)
    {
        if (dto.IdDentista <= 0) 
            return ApplicationResult<DentistaDTO>.Failure("ID inválido.", 400);

        var dentista = await _dentistaRepository.GetByIdAsync(dto.IdDentista);
        if (dentista == null)
            return ApplicationResult<DentistaDTO>.Failure("Dentista não encontrado.", 404);

        var funcionario = await _funcionarioRepository.GetByIdAsync(dentista.IdFuncionario);
        if (funcionario == null)
            return ApplicationResult<DentistaDTO>.Failure("Dados do funcionário vinculado não encontrados.", 404);
        
        funcionario.Nome = dto.Nome;
        funcionario.Email = dto.Email;
        funcionario.Telefone = dto.Telefone;
        funcionario.Ativo = dto.Ativo;
        funcionario.DataContratacao = dto.DataContratacao;
        funcionario.SalarioBase = dto.SalarioBase;
        
        dentista.Cro = dto.Cro;

        await _funcionarioRepository.UpdateAsync(funcionario);
        await _dentistaRepository.UpdateAsync(dentista);

        return ApplicationResult<DentistaDTO>.Success(dto, 200, "Dados do dentista atualizados.");
    }

    public async Task<ApplicationResult<DentistaDTO>> Excluir(long id)
    {
        var dentista = await _dentistaRepository.GetByIdAsync(id);
        if (dentista == null)
            return ApplicationResult<DentistaDTO>.Failure("Dentista não encontrado.", 404);
        
        await _funcionarioRepository.DeleteAsync(dentista.IdFuncionario);

        return ApplicationResult<DentistaDTO>.Success(new DentistaDTO { IdDentista = id }, 200, "Dentista e dados funcionais removidos.");
    }

    public async Task<ApplicationResult<DentistaDTO>> BuscarPorId(long id)
    {
        var dentista = await _dentistaRepository.GetByIdAsync(id);
        if (dentista == null) 
            return ApplicationResult<DentistaDTO>.Failure("Dentista não encontrado", 404);

        var funcionario = await _funcionarioRepository.GetByIdAsync(dentista.IdFuncionario);
        
        return ApplicationResult<DentistaDTO>.Success(MapToDto(dentista, funcionario!));
    }

    public async Task<ApplicationResult<IEnumerable<DentistaDTO>>> BuscarTodos()
    {
        var dentistas = await _dentistaRepository.GetAllAsync();
        var listaDtos = new List<DentistaDTO>();

        foreach (var d in dentistas)
        {
            var f = await _funcionarioRepository.GetByIdAsync(d.IdFuncionario);
            if (f != null)
            {
                listaDtos.Add(MapToDto(d, f));
            }
        }

        return ApplicationResult<IEnumerable<DentistaDTO>>.Success(listaDtos);
    }
    
    public async Task<ApplicationResult<DentistaDTO>> BuscarPorUserId(string userId)
    {
        var funcionarios = await _funcionarioRepository.FindAsync(f => f.UserId == userId);
        var funcionario = funcionarios.FirstOrDefault();

        if (funcionario == null)
            return ApplicationResult<DentistaDTO>.Failure("Usuário não possui vínculo com nenhum funcionário.", 404);
        
        var dentistas = await _dentistaRepository.FindAsync(d => d.IdFuncionario == funcionario.IdFuncionario);
        var dentista = dentistas.FirstOrDefault();

        if (dentista == null)
            return ApplicationResult<DentistaDTO>.Failure("O funcionário vinculado não é um Dentista.", 403);

        return ApplicationResult<DentistaDTO>.Success(MapToDto(dentista, funcionario));
    }
    
    private static DentistaDTO MapToDto(Dentista d, Funcionario f)
    {
        return new DentistaDTO
        {
            IdDentista = d.IdDentista,
            Cro = d.Cro,
            IdFuncionario = f.IdFuncionario,
            Nome = f.Nome,
            Email = f.Email,
            Telefone = f.Telefone,
            Ativo = f.Ativo,
            DataContratacao = f.DataContratacao,
            SalarioBase = f.SalarioBase,
            Tipo = f.Tipo
        };
    }
}