using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSys.Api.Application;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;

namespace OdontoSys.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IGenericRepository<Paciente> _pacienteRepo;
    private readonly IGenericRepository<Consulta> _consultaRepo;
    private readonly IGenericRepository<MovimentacaoFinanceira> _financeiroRepo;
    private readonly IGenericRepository<Parcela> _parcelaRepo;

    public DashboardController(
        IGenericRepository<Paciente> pacienteRepo,
        IGenericRepository<Consulta> consultaRepo,
        IGenericRepository<MovimentacaoFinanceira> financeiroRepo,
        IGenericRepository<Parcela> parcelaRepo)
    {
        _pacienteRepo = pacienteRepo;
        _consultaRepo = consultaRepo;
        _financeiroRepo = financeiroRepo;
        _parcelaRepo = parcelaRepo;
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo()
    {
        var pacientes = await _pacienteRepo.FindAsync(p => p.Ativo);
        var totalPacientes = pacientes.Count();
        
        var hoje = DateTime.Today;
        var amanha = hoje.AddDays(1);
        var consultasHoje = await _consultaRepo.FindAsync(c => c.DataInicio >= hoje && c.DataInicio < amanha && c.Situacao != SituacaoConsulta.Cancelada);
        var qtdConsultasHoje = consultasHoje.Count();
        
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1);
        var movs = await _financeiroRepo.FindAsync(m => m.DataMovimentacao >= inicioMes && m.DataMovimentacao < fimMes && m.Tipo == TipoMovimentacao.Entrada);
        var faturamentoMes = movs.Sum(m => m.Valor);
        
        var parcelas = await _parcelaRepo.FindAsync(p => p.Status == StatusParcela.Pendente);
        var aReceber = parcelas.Sum(p => p.Valor);
        
        var proximasConsultas = (await _consultaRepo.FindAsync(c => c.DataInicio >= DateTime.Now && c.Situacao == SituacaoConsulta.Agendada))
            .OrderBy(c => c.DataInicio)
            .Take(5)
            .Select(c => new 
            {
                c.IdConsulta,
                Data = c.DataInicio,
                Paciente = c.Paciente?.Nome ?? "Carregando...",
                Dentista = c.Dentista?.Funcionario?.Nome ?? "Dentista"
            });

        return Ok(ApplicationResult<object>.Success(new 
        {
            TotalPacientes = totalPacientes,
            ConsultasHoje = qtdConsultasHoje,
            FaturamentoMes = faturamentoMes,
            TotalAReceber = aReceber,
            ProximasConsultas = proximasConsultas
        }));
    }
}