using ATMSimulador.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMSimulador.Infrastructure.Database.Configs
{
    public class CuentaConfig : IEntityTypeConfiguration<Cuenta>
    {
        public void Configure(EntityTypeBuilder<Cuenta> builder)
        {
            throw new NotImplementedException();
        }
    }
}
