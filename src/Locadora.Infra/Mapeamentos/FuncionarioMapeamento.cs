using Locadora.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locadora.Infra.Mapeamentos;

public class FuncionarioMapeamento : IEntityTypeConfiguration<Funcionario>
{
    // Hash BCrypt fixo de "admin123": gerar dentro do HasData mudaria a migration a cada geração.
    private const string SenhaHashAdmin = "$2a$11$/DikLFg7l6Z8oqbZmlakgObqpEEW/TTrwdwk1g.dbopnsnuFPYE8y";

    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("Funcionarios");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Nome).IsRequired().HasMaxLength(120);
        builder.Property(f => f.Email).IsRequired().HasMaxLength(150);
        builder.Property(f => f.SenhaHash).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Ativo).IsRequired();

        builder.HasIndex(f => f.Email).IsUnique();

        builder.HasData(new
        {
            Id = 1,
            Nome = "Administrador",
            Email = "admin@locadora.local",
            SenhaHash = SenhaHashAdmin,
            Ativo = true,
        });
    }
}
