using Accounts.API.Common.Entities;
using Accounts.API.Common.Repositories.Interfaces;

namespace Accounts.API.Common.Repositories
{
    public class IntegrationRepositoryBase : IIntegrationRepositoryBase
    {
        public void Add(IntegrationLog log)
        {
            //using (var context = new LogContext())
            //{
            //    context.IntegrationLogs.Add(log);
            //    context.SaveChanges();
            //}
        }
    }
}
