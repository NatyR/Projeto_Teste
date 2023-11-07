using Portal.API.Common.Dto;
using Portal.API.Dto.Banner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IBannerService
    {
        Task<IEnumerable<BannerDto>> GetAll();
        Task<IEnumerable<BannerDto>> GetActiveBanners();

        Task<ResponseDto<BannerDto>> GetAllPaged(int limit, int skip, string search, string order);
        Task<BannerDto> Get(long id);
        Task<BannerDto> Create(BannerAddDto banner);
        Task<BannerDto> Update(BannerUpdateDto banner);
        Task Delete(long id);

        Task Activate(long[] ids);
        Task Deactivate(long[] ids);

        string GetSignedUrl(BannerDto banner);
    }
}
