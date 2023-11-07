using Portal.API.Common.Dto;
using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IBannerRepository
    {
        Task<List<Banner>> GetActiveBanners();

        Task<IEnumerable<Banner>> GetAll();
        Task<ResponseDto<Banner>> GetAllPaged(int limit, int skip, string search, string order);
        Task<Banner> Get(long id);
        Task<Banner> Create(Banner banner);
        Task<Banner> Update(Banner banner);
        Task Delete(long id);
        Task Activate(long id);
        Task Deactivate(long id);

    }
}
