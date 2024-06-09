using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class CuentaConfig : IEntityTypeConfiguration<Cuenta>
    {
        public void Configure(EntityTypeBuilder<Cuenta> builder)
        {
            builder.ToTable("Cuentas");

            builder.HasKey(c => c.CuentaId);
            builder.Property(c => c.CuentaId).ValueGeneratedOnAdd();

            builder.Property(c => c.UsuarioId).IsRequired();
            builder.Property(c => c.NumeroCuenta)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Saldo)
                .IsRequired()
                .HasColumnType("varbinary(26)");

            builder.Property(c => c.Activa)
                .IsRequired();

            builder.HasIndex(c => c.NumeroCuenta)
                .IsUnique();

            builder.HasOne(c => c.Usuario)
                .WithMany(u => u.Cuentas)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar la relación con Transacciones
            builder.HasMany(c => c.Transacciones)
                .WithOne(t => t.Cuenta)
                .HasForeignKey(t => t.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
