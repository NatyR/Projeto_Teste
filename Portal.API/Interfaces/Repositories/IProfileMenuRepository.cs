using System.Collections.Generic;
using System.Threading.Tasks;
using Portal.API.Entities;
namespace Portal.API.Interfaces.Repositories
{
    public interface IProfileMenuPortalRepository
    {
        Task<IEnumerable<ProfileMenu>> GetByProfile(long profileId);
        Task<ProfileMenu> Add(ProfileMenu profileMenu);
        Task Delete(long profileId);
    }
}
