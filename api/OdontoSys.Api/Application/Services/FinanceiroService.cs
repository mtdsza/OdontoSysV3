using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class FinanceiroService : IFinanceiroService
{
    private readonly IGenericRepository<Parcela> _parcelaRepo;
    private readonly IGenericRepository<MovimentacaoFinanceira> _movimentacaoRepo;
    private readonly IGenericRepository<Orcamento> _orcamentoRepo;

    public FinanceiroService(
        IGenericRepository<Parcela> parcelaRepo,
        IGenericRepository<MovimentacaoFinanceira> movimentacaoRepo,
        IGenericRepository<Orcamento> orcamentoRepo)
    {
        _parcelaRepo = parcelaRepo;
        _movimentacaoRepo = movimentacaoRepo;
        _orcamentoRepo = orcamentoRepo;
    }

    public async Task<ApplicationResult<IEnumerable<ParcelaDTO>>> GerarParcelas(GerarParcelasDTO dto)
    {
        var orcamento = await _orcamentoRepo.GetByIdAsync(dto.IdOrcamento);
        if (orcamento == null) return ApplicationResult<IEnumerable<ParcelaDTO>>.Failure("Orçamento não encontrado", 404);

        if (!orcamento.Aprovado) return ApplicationResult<IEnumerable<ParcelaDTO>>.Failure("Orçamento ainda não foi aprovado.", 400);
        
        var existentes = await _parcelaRepo.FindAsync(p => p.IdOrcamento == dto.IdOrcamento && p.Status != StatusParcela.Cancelada);
        if (existentes.Any()) return ApplicationResult<IEnumerable<ParcelaDTO>>.Failure("Parcelas ativas já existem para este orçamento.", 409);

        decimal valorParcela = Math.Round(orcamento.ValorTotal / dto.QuantidadeParcelas, 2);
        decimal diferenca = orcamento.ValorTotal - (valorParcela * dto.QuantidadeParcelas);

        var novasParcelas = new List<Parcela>();

        for (int i = 1; i <= dto.QuantidadeParcelas; i++)
        {
            decimal valorFinal = (i == dto.QuantidadeParcelas) ? valorParcela + diferenca : valorParcela;

            var p = new Parcela
            {
                IdOrcamento = orcamento.IdOrcamento,
                Valor = valorFinal,
                Descricao = $"Parcela {i}/{dto.QuantidadeParcelas}",
                DataVencimento = dto.PrimeiroVencimento.AddMonths(i - 1),
                Status = StatusParcela.Pendente
            };
            await _parcelaRepo.AddAsync(p);
            novasParcelas.Add(p);
        }

        return await BuscarParcelasPorOrcamento(dto.IdOrcamento);
    }

    public async Task<ApplicationResult<string>> PagarParcela(long idParcela)
    {
        var parcela = await _parcelaRepo.GetByIdAsync(idParcela);
        if (parcela == null) return ApplicationResult<string>.Failure("Parcela não encontrada", 404);

        if (parcela.Status == StatusParcela.Paga) 
            return ApplicationResult<string>.Failure("Parcela já está paga.", 400);
            
        if (parcela.Status == StatusParcela.Cancelada) 
            return ApplicationResult<string>.Failure("Não é possível pagar uma parcela cancelada.", 400);

        parcela.Status = StatusParcela.Paga;
        parcela.DataPagamento = DateTime.Now;
        await _parcelaRepo.UpdateAsync(parcela);

        var mov = new MovimentacaoFinanceira
        {
            Descricao = $"Pagamento: {parcela.Descricao} (Orc #{parcela.IdOrcamento})",
            Valor = parcela.Valor,
            Tipo = TipoMovimentacao.Entrada,
            IdParcelaPaga = parcela.IdParcela,
            DataMovimentacao = DateTime.Now
        };
        await _movimentacaoRepo.AddAsync(mov);

        return ApplicationResult<string>.Success("Pagamento realizado com sucesso.");
    }

    public async Task<ApplicationResult<string>> CancelarParcelasDoOrcamento(long idOrcamento)
    {
        var parcelas = await _parcelaRepo.FindAsync(p => p.IdOrcamento == idOrcamento);

        if (!parcelas.Any())
            return ApplicationResult<string>.Failure("Não há parcelas para este orçamento.", 404);
        
        if (parcelas.Any(p => p.Status == StatusParcela.Paga))
            return ApplicationResult<string>.Failure("Não é possível cancelar as parcelas pois existem pagamentos confirmados. Realize o estorno manual ou contate o suporte.", 400);
        
        foreach (var parcela in parcelas)
        {
            if (parcela.Status != StatusParcela.Cancelada)
            {
                parcela.Status = StatusParcela.Cancelada;
                await _parcelaRepo.UpdateAsync(parcela);
            }
        }

        return ApplicationResult<string>.Success("Parcelas canceladas com sucesso. Agora é possível gerar novas condições de pagamento.", 200);
    }

    public async Task<ApplicationResult<IEnumerable<ParcelaDTO>>> BuscarParcelasPorOrcamento(long idOrcamento)
    {
        var lista = await _parcelaRepo.FindAsync(p => p.IdOrcamento == idOrcamento);
        
        var dtos = lista.OrderBy(p => p.DataVencimento).Select(p => new ParcelaDTO
        {
            IdParcela = p.IdParcela,
            Descricao = p.Descricao,
            Valor = p.Valor,
            DataVencimento = p.DataVencimento,
            Status = (int)p.Status,
            DataPagamento = p.DataPagamento,
            IdOrcamento = p.IdOrcamento
        });
        return ApplicationResult<IEnumerable<ParcelaDTO>>.Success(dtos);
    }

    public async Task<ApplicationResult<IEnumerable<MovimentacaoDTO>>> BuscarMovimentacoes()
    {
        var lista = await _movimentacaoRepo.GetAllAsync();
        var dtos = lista.OrderByDescending(m => m.DataMovimentacao).Select(m => new MovimentacaoDTO
        {
            IdMovimentacao = m.IdMovimentacao,
            Descricao = m.Descricao,
            Valor = m.Valor,
            Tipo = m.Tipo,
            DataMovimentacao = m.DataMovimentacao
        });
        return ApplicationResult<IEnumerable<MovimentacaoDTO>>.Success(dtos);
    }

    public async Task<ApplicationResult<long>> RegistrarDespesa(MovimentacaoDTO dto)
    {
        if (dto.Tipo != TipoMovimentacao.Saida) 
            return ApplicationResult<long>.Failure("Use este método apenas para registrar Saídas/Despesas.", 400);

        var mov = new MovimentacaoFinanceira
        {
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            Tipo = TipoMovimentacao.Saida,
            DataMovimentacao = DateTime.Now
        };

        await _movimentacaoRepo.AddAsync(mov);
        return ApplicationResult<long>.Success(mov.IdMovimentacao, 201, "Despesa registrada.");
    }
}