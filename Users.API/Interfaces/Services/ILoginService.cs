using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Common.Dto;
using Users.API.Dto.Login;

namespace Users.API.Interfaces.Services
{
    public interface ILoginService
    {
        Task<ResponseDto<LoginDto>> GetAllPaged(LoginFilterDto filterOptions, int limit, int skip, string order);
        Task<LoginDto> Get(long id);
    }
}
