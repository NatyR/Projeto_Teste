using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Dto.Profile;

namespace Users.API.Interfaces.Services
{
    public interface IProfileService
    {
        Task<IEnumerable<ProfileDto>> GetAll();
        Task<IEnumerable<ProfileDto>> GetBySistema(long sistema);
        Task<ProfileDto> Get(long id);
        Task<ProfileDto> Add(ProfileAddDto profile);
        Task<ProfileDto> Update(ProfileUpdateDto profile);
        Task Delete(long id);
    }
}