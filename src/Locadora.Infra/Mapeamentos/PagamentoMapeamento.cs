using Locadora.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locadora.Infra.Mapeamentos;

public class PagamentoMapeamento : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("Pagamentos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Tipo).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Forma).IsRequired().HasConversion<string>().HasMaxLength(20);
    }
}
