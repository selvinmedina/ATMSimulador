using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Infrastructure.Database
{
    public class ATMDbContext : DbContext
    {
        public ATMDbContext(DbContextOptions<ATMDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ATMDbContext).Assembly);
        }
    }
}
