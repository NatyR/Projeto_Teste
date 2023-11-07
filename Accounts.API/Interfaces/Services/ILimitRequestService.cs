using Accounts.API.Common.Dto;
using Accounts.API.Common.Dto.LimitRequest;
using Accounts.API.Common.Dto.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Accounts.API.Interfaces.Services
{
    public interface ILimitRequestService
    {
        Task<ResponseDto<LimitRequestDto>> GetAllPaged(long convenio_id, string status, int limit, int skip, string search, string order);
        Task Approve(long[] ids, UserDtoAccounts user);
        Task Reject(long[] ids, UserDtoAccounts user);
    }
}
