using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Common.Dto;
using Users.API.Dto.User;

namespace Users.API.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAll();
        Task<ResponseDto<UserDto>> GetAllPaged(long? groupId, int sistema_id, int limit, int skip, string search, string order);
        Task<ResponseDto<UserDto>> GetAllPaged(UserFilterDto filter, int limit, int skip, string order);
        Task<List<UserDto>> GetAll(UserFilterDto filter);
        Task<List<UserMenuDto>> GetAllbyMenu(string searchTerm);
        Task<UserDto> Get(long id);
        Task<UserDto> Add(UserAddDto user, int userId);
        Task<UserDto> Update(UserUpdateDto user);
        Task<UserDto> UpdateStatus(UserStatusDto user);
        Task Activate(long[] ids);
        Task Deactivate(long[] ids);
        Task Cancel(long[] ids);
        Task Delete(long id);
    }
}