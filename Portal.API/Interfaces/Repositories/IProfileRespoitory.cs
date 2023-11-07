using Portal.API.Common.Dto;
using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IProfileRepositoryPortal
    {
        Task<IEnumerable<ProfilePortal>> GetAll();
        Task<ResponseDto<ProfilePortal>> GetAllPaged(int limit, int skip, string search, string order);
        Task<IEnumerable<ProfilePortal>> GetBySistema(long sistemaId);
        Task<ProfilePortal> Get(long id);
        Task<ProfilePortal> Create(ProfilePortal profile);
        Task<ProfilePortal> Update(ProfilePortal profile);
        Task Delete(long id);
    }
}
