using Accounts.API.Common.Dto;
using Accounts.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Interfaces.Repositories
{
    public interface ILimitRequestRepository
    {
        Task<LimitRequest> Create(LimitRequest limitRequest);
        Task<LimitRequest> GetById(long id);
        Task<ResponseDto<LimitRequest>> GetAllPaged(long convenio_id, string status, int limit, int skip, string search, string order);
        Task Approve(long id, long userId);
        Task Reject(long id, long userId);
    }
}
