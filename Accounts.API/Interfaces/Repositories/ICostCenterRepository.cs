using Accounts.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Interfaces.Repositories
{
    public interface ICostCenterRepository
    {
        Task<List<CostCenter>> GetAllByConvenio(long convenio);
    }
}
