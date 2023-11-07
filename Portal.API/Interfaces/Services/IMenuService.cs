using Portal.API.Common.Dto;
using Portal.API.Dto.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Services
{
    public interface IMenuService
    {
        Task<List<GroupMenuDto>> GetBySistemaId(long id);
        Task<List<MenuDto>> GetGroupsBySistemaId(long sistema_id);
        Task<List<GroupMenuDto>> GetByUserAndConvenio(long user_id,long sistema_id,long profile_id, long? convenio_id);

        Task<ResponseDto<MenuDto>> GetAllPaged(int limit, int skip, string order, MenuFilterDto filter);
        Task<MenuDto> Get(long id);
        Task<MenuDto> Create(MenuAddDto menu);
        Task<MenuDto> Update(MenuUpdateDto menu);
        Task Delete(long id);
    }
}
