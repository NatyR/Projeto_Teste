using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IConfigurationRepository
    {
        Task<Configuration> GetConfiguration();
        Task<Configuration> Update(Configuration configuration);
    }
}
