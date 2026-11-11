using Locadora.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locadora.Infra.Mapeamentos;

public class LocacaoMapeamento : IEntityTypeConfiguration<Locacao>
{
    public void Configure(EntityTypeBuilder<Locacao> builder)
    {
        builder.ToTable("Locacoes");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.ObservacaoRetirada).HasMaxLength(500);
        builder.Property(l => l.ObservacaoDevolucao).HasMaxLength(500);

        builder.HasIndex(l => l.Status);

        builder.HasOne(l => l.Cliente)
            .WithMany()
            .HasForeignKey(l => l.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Veiculo)
            .WithMany()
            .HasForeignKey(l => l.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Pagamentos)
            .WithOne()
            .HasForeignKey("LocacaoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Pagamentos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
