using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class ConsultaService : IConsultaService
{
    private readonly IGenericRepository<Consulta> _consultaRepo;
    private readonly IGenericRepository<Paciente> _pacienteRepo;
    private readonly IGenericRepository<Dentista> _dentistaRepo;
    private readonly IGenericRepository<Funcionario> _funcionarioRepo;

    public ConsultaService(
        IGenericRepository<Consulta> consultaRepo,
        IGenericRepository<Paciente> pacienteRepo,
        IGenericRepository<Dentista> dentistaRepo,
        IGenericRepository<Funcionario> funcionarioRepo)
    {
        _consultaRepo = consultaRepo;
        _pacienteRepo = pacienteRepo;
        _dentistaRepo = dentistaRepo;
        _funcionarioRepo = funcionarioRepo;
    }

    public async Task<ApplicationResult<long>> Agendar(ConsultaDTO dto)
    {
        if (dto.DataInicio < DateTime.Now.AddMinutes(-10))
            return ApplicationResult<long>.Failure("Não é possível agendar no passado.", 400);

        var paciente = await _pacienteRepo.GetByIdAsync(dto.IdPaciente);
        if (paciente == null) return ApplicationResult<long>.Failure("Paciente não encontrado.", 404);

        var dentista = await _dentistaRepo.GetByIdAsync(dto.IdDentista);
        if (dentista == null) return ApplicationResult<long>.Failure("Dentista não encontrado.", 404);

        var conflitos = await _consultaRepo.FindAsync(c => 
            c.IdDentista == dto.IdDentista && 
            c.DataInicio == dto.DataInicio && 
            c.Situacao != SituacaoConsulta.Cancelada);

        if (conflitos.Any())
            return ApplicationResult<long>.Failure("O dentista já possui um agendamento neste horário.", 409);

        var consulta = new Consulta
        {
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim ?? dto.DataInicio.AddMinutes(30),
            Descricao = dto.Descricao,
            Situacao = SituacaoConsulta.Agendada,
            IdPaciente = dto.IdPaciente,
            IdDentista = dto.IdDentista,
            Ativo = true
        };

        await _consultaRepo.AddAsync(consulta);
        return ApplicationResult<long>.Success(consulta.IdConsulta, 201, "Consulta agendada com sucesso!");
    }

    public async Task<ApplicationResult<ConsultaDTO>> Atualizar(ConsultaDTO dto)
    {
        var consulta = await _consultaRepo.GetByIdAsync(dto.IdConsulta);
        if (consulta == null) return ApplicationResult<ConsultaDTO>.Failure("Consulta não encontrada.", 404);

        consulta.DataInicio = dto.DataInicio;
        consulta.DataFim = dto.DataFim;
        consulta.Descricao = dto.Descricao;
        consulta.Situacao = dto.Situacao;
        consulta.Diagnostico = dto.Diagnostico;
        consulta.Prescricao = dto.Prescricao;
        
        await _consultaRepo.UpdateAsync(consulta);
        return ApplicationResult<ConsultaDTO>.Success(dto, 200, "Consulta atualizada.");
    }

    public async Task<ApplicationResult<ConsultaDTO>> Cancelar(long id)
    {
        var consulta = await _consultaRepo.GetByIdAsync(id);
        if (consulta == null) return ApplicationResult<ConsultaDTO>.Failure("Consulta não encontrada.", 404);

        consulta.Situacao = SituacaoConsulta.Cancelada;
        await _consultaRepo.UpdateAsync(consulta);

        return ApplicationResult<ConsultaDTO>.Success(new ConsultaDTO { IdConsulta = id }, 200, "Consulta cancelada.");
    }

    public async Task<ApplicationResult<ConsultaDTO>> BuscarPorId(long id)
    {
        var c = await _consultaRepo.GetByIdAsync(id);
        if (c == null) return ApplicationResult<ConsultaDTO>.Failure("Não encontrada", 404);

        return ApplicationResult<ConsultaDTO>.Success(await MapToDto(c));
    }

    public async Task<ApplicationResult<IEnumerable<ConsultaDTO>>> BuscarTodas()
    {
        var consultas = await _consultaRepo.GetAllAsync();
        var lista = new List<ConsultaDTO>();
        foreach (var c in consultas) lista.Add(await MapToDto(c));
        
        return ApplicationResult<IEnumerable<ConsultaDTO>>.Success(lista);
    }
    
    public async Task<ApplicationResult<IEnumerable<ConsultaDTO>>> BuscarPorDentista(long idDentista)
    {
        var consultas = await _consultaRepo.FindAsync(c => c.IdDentista == idDentista);
        var lista = new List<ConsultaDTO>();
        foreach (var c in consultas) lista.Add(await MapToDto(c));

        return ApplicationResult<IEnumerable<ConsultaDTO>>.Success(lista);
    }

    private async Task<ConsultaDTO> MapToDto(Consulta c)
    {
        var paciente = await _pacienteRepo.GetByIdAsync(c.IdPaciente);
        
        var dentista = await _dentistaRepo.GetByIdAsync(c.IdDentista);
        string nomeDentista = "Desconhecido";
        
        if (dentista != null)
        {
            var func = await _funcionarioRepo.GetByIdAsync(dentista.IdFuncionario);
            if (func != null) nomeDentista = func.Nome;
        }

        return new ConsultaDTO
        {
            IdConsulta = c.IdConsulta,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Descricao = c.Descricao,
            Situacao = c.Situacao,
            IdPaciente = c.IdPaciente,
            NomePaciente = paciente?.Nome ?? "Desconhecido",
            IdDentista = c.IdDentista,
            NomeDentista = nomeDentista,
            Diagnostico = c.Diagnostico,
            Prescricao = c.Prescricao
        };
    }
}