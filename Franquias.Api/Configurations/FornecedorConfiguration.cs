using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.RazaoSocial).IsRequired().HasMaxLength(160);
        builder.Property(f => f.Cnpj).IsRequired().HasMaxLength(14);
        builder.Property(f => f.Telefone).HasMaxLength(20);
        builder.Property(f => f.Email).HasMaxLength(160);
        builder.Property(f => f.Ativo).HasDefaultValue(true);
        builder.HasIndex(f => f.Cnpj).IsUnique();
    }
}
