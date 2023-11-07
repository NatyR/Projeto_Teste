using Portal.API.Common.Entities.Integrations;
using Portal.API.Common.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Common.Repositories
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
