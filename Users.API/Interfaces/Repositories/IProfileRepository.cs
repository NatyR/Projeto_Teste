using Users.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Users.API.Interfaces.Repositories
{
    public interface IProfileRepository
    {
      
        Task<IEnumerable<Profile>> GetAll();
        Task<IEnumerable<Profile>> GetBySistema(long sistemaId);
        Task<Profile> Get(long id);
        Task<Profile> Add(Profile profile);
        Task<Profile> Update(Profile profile);
        Task Delete(long id);
    }
}