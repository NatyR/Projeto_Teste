using Portal.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface ISistemaRepository
    {

        Task<IEnumerable<Sistema>> GetAll();
        Task<Sistema> Get(long id);
        Task<Sistema> Add(Sistema sistema);
        Task<Sistema> Update(Sistema sistema);
        Task Delete(long id);
    }
}