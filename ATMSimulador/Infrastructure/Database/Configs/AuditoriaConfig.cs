using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class AuditoriaConfig : IEntityTypeConfiguration<Auditoria>
    {
        public void Configure(EntityTypeBuilder<Auditoria> builder)
        {
            builder.ToTable("Auditoria");

            builder.HasKey(a => a.AuditoriaId);
            builder.Property(a => a.AuditoriaId).ValueGeneratedOnAdd();

            builder.Property(a => a.UsuarioId).IsRequired();
            builder.Property(a => a.TipoActividad)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.FechaActividad).IsRequired();

            builder.Property(a => a.Descripcion)
                .HasMaxLength(255);

            builder.HasOne(a => a.Usuario)
                .WithMany(u => u.Auditorias)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
