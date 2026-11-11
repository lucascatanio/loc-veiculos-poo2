using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Contexto;

public class LocadoraDbContext(DbContextOptions<LocadoraDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocadoraDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(10, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(200);
    }
}
