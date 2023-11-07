using Portal.API.Common.Dto;
using Portal.API.Dto.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IProfileServicePortal
    {
        Task<IEnumerable<PortalProfileDto>> GetAll();
        Task<ResponseDto<PortalProfileDto>> GetAllPaged(int limit, int skip, string search, string order);
        Task<PortalProfileDto> Get(long id);
        Task<IEnumerable<PortalProfileDto>> GetBySistema(long sistemaId);
        Task<PortalProfileDto> Create(PortalProfileAddDto profile);
        Task<PortalProfileDto> Update(PortalProfileUpdateDto profile);
        Task Delete(long id);
    }
}
