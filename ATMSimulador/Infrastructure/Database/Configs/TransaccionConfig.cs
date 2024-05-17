using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class TransaccionConfig : IEntityTypeConfiguration<Transaccion>
    {
        public void Configure(EntityTypeBuilder<Transaccion> builder)
        {
            throw new NotImplementedException();
        }
    }
}
