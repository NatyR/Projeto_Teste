using Portal.API.Common.Dto;
using Portal.API.Dto.Manual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IManualService
    {
        Task<IEnumerable<ManualDto>> GetAll();
        Task<IEnumerable<ManualDto>> GetActiveManuals();

        Task<ResponseDto<ManualDto>> GetAllPaged(string manualType, int limit, int skip, string search, string order);
        Task<ManualDto> Get(long id);
        string GetSignedUrl(ManualDto manual);
        Task<ManualDto> Create(ManualAddDto manual);
        Task<ManualDto> Update(ManualUpdateDto manual);
        Task Delete(long id);
        Task Activate(long[] ids);
        Task Deactivate(long[] ids);
    }
}
