using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class PagoConfig : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos");

            builder.HasKey(p => p.PagoId);
            builder.Property(p => p.PagoId).ValueGeneratedOnAdd();

            builder.Property(p => p.ServicioId).IsRequired();
            builder.Property(p => p.CuentaId).IsRequired();
            builder.Property(p => p.Monto)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.FechaPago)
                .IsRequired();

            builder.HasOne(p => p.Servicio)
                .WithMany(s => s.Pagos)
                .HasForeignKey(p => p.ServicioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Cuenta)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
