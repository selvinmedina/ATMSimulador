using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.UsuarioId);
            builder.Property(u => u.UsuarioId).ValueGeneratedOnAdd();

            builder.Property(u => u.NombreUsuario)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.HashContrasena)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.Pin)
                .IsRequired()
                .HasMaxLength(4);

            // Relación uno a muchos con Cuentas
            builder.HasMany(u => u.Cuentas)
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
