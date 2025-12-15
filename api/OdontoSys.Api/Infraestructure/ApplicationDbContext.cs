using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OdontoSys.Api.Domain.Entities;

namespace OdontoSys.Api.Infraestructure;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Dentista> Dentistas { get; set; }
    public DbSet<Consulta> Consultas { get; set; }
    public DbSet<TratamentoExterno> TratamentosExternos { get; set; }
    public DbSet<Procedimento> Procedimentos { get; set; }
    public DbSet<Orcamento> Orcamentos { get; set; }
    public DbSet<OrcamentoItem> OrcamentoItens { get; set; }
    public DbSet<Parcela> ParcelasAReceber { get; set; }
    public DbSet<MovimentacaoFinanceira> MovimentacoesFinanceiras { get; set; }
    public DbSet<ItemEstoque> Estoque { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesGeraisEstoque { get; set; }
    public DbSet<UsoMaterial> UsoMateriaisConsulta { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Paciente>().HasIndex(p => p.Cpf).IsUnique();
        modelBuilder.Entity<Funcionario>().HasIndex(f => f.Email).IsUnique();
        modelBuilder.Entity<Dentista>().HasIndex(d => d.Cro).IsUnique();
    }
}