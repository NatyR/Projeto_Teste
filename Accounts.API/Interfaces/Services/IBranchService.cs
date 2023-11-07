using Accounts.API.Common.Dto.Branch;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Accounts.API.Interfaces.Services
{
    public interface IBranchService
    {
        Task<List<BranchDto>> GetAllByConvenio(long convenio);
        
    }
}
