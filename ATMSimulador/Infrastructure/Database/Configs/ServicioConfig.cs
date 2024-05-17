using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class ServicioConfig : IEntityTypeConfiguration<Servicio>
    {
        public void Configure(EntityTypeBuilder<Servicio> builder)
        {
            throw new NotImplementedException();
        }
    }
}
