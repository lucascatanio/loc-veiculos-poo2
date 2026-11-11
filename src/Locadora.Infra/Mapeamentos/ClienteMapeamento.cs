using Locadora.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locadora.Infra.Mapeamentos;

public class ClienteMapeamento : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(120);
        builder.Property(c => c.Cpf).IsRequired().HasMaxLength(11);
        builder.Property(c => c.Cnh).IsRequired().HasMaxLength(11);
        builder.Property(c => c.Telefone).IsRequired().HasMaxLength(11);
        builder.Property(c => c.Email).HasMaxLength(150);
        builder.Property(c => c.Ativo).IsRequired();

        builder.HasIndex(c => c.Cpf).IsUnique();
    }
}
