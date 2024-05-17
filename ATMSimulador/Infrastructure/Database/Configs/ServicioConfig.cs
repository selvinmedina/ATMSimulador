using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class ServicioConfig : IEntityTypeConfiguration<Servicio>
    {
        public void Configure(EntityTypeBuilder<Servicio> builder)
        {
            builder.ToTable("Servicios");

            builder.HasKey(s => s.ServicioId);
            builder.Property(s => s.ServicioId).ValueGeneratedOnAdd();

            builder.Property(s => s.NombreServicio)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(s => s.Descripcion)
                .HasMaxLength(255);
        }
    }
}
