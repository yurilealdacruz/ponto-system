namespace Ponto.Data;

using Microsoft.EntityFrameworkCore;
using Ponto.Domain;

public class PontoDbContext : DbContext
{
    public PontoDbContext(DbContextOptions<PontoDbContext> options) : base(options) { }

    // Mapeamento das tabelas
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<RegistroPonto> RegistrosPonto { get; set; }
}