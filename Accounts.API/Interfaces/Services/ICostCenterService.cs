using Accounts.API.Common.Dto.CostCenter;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Accounts.API.Interfaces.Services
{
    public interface ICostCenterService
    {
        Task<List<CostCenterDto>> GetAllByConvenio(long convenio);
        
    }
}
