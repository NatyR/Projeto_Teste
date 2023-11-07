using Portal.API.Common.Dto;
using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IManualRepository
    {
        Task<List<Manual>> GetActiveManuals();
        Task<IEnumerable<Manual>> GetAll();
        Task<ResponseDto<Manual>> GetAllPaged(string manualType, int limit, int skip, string search, string order);
        Task<Manual> Get(long id);
        Task<Manual> Create(Manual manual);
        Task<Manual> Update(Manual manual);
        Task Delete(long id);
        Task Activate(long id);
        Task Deactivate(long id);
    }
}
