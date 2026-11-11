using Locadora.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locadora.Infra.Mapeamentos;

public class VeiculoMapeamento : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("Veiculos");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Placa).IsRequired().HasMaxLength(7);
        builder.Property(v => v.Marca).IsRequired().HasMaxLength(60);
        builder.Property(v => v.Modelo).IsRequired().HasMaxLength(60);
        builder.Property(v => v.Cor).IsRequired().HasMaxLength(30);
        builder.Property(v => v.Ano).IsRequired();
        builder.Property(v => v.KmAtual).IsRequired();
        builder.Property(v => v.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.Ativo).IsRequired();

        builder.HasIndex(v => v.Placa).IsUnique();

        builder.HasOne(v => v.Categoria)
            .WithMany()
            .HasForeignKey(v => v.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
