using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class TransaccionConfig : IEntityTypeConfiguration<Transaccion>
    {
        public void Configure(EntityTypeBuilder<Transaccion> builder)
        {
            builder.ToTable("Transacciones");

            builder.HasKey(t => t.TransaccionId);
            builder.Property(t => t.TransaccionId).ValueGeneratedOnAdd();

            builder.Property(t => t.CuentaId).IsRequired();

            builder.Property(t => t.TipoTransaccion)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Monto)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.FechaTransaccion)
                .IsRequired();

            builder.Property(t => t.Estado)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(t => t.Cuenta)
                .WithMany(c => c.Transacciones)
                .HasForeignKey(t => t.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
