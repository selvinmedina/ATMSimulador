using ATMSimulador.Infrastructure.Database;
using EntityFramework.Infrastructure.Core.UnitOfWork;

namespace ATMSimulador.Infrastructure
{
    public class ApplicationUnitOfWork : UnitOfWork
    {
        public ApplicationUnitOfWork(ATMDbContext dbContext) : base(dbContext)
        {

        }
    }
}
