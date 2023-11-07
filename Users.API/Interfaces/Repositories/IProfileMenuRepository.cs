using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Entities;

namespace Users.API.Interfaces.Repositories
{
    public interface IProfileMenuRepository
    {
        Task<IEnumerable<ProfileMenu>> GetByProfile(long profileId);
        Task<ProfileMenu> Add(ProfileMenu profileMenu);
        Task Delete(long profileId);
    }
}
