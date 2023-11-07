using Portal.API.Common.Dto;
using Portal.API.Dto.Menu;
using Portal.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IMenuRepository
    {
        //Task<List<Menu>> GetUserMenuByConvenio();
        Task<List<Menu>> GetBySistemaId(long id);
        Task<List<Menu>> GetByUserAndConvenio(long user_id, long sistema_id, long profile_id, long? convenio_id);
        Task<ResponseDto<Menu>> GetAllPaged(int limit, int skip, string order, MenuFilterDto filter);
        Task<Menu> Get(long id);
        Task<Menu> Create(Menu menu);
        Task<Menu> Update(Menu menu);
        Task Delete(long id);
    }
}
