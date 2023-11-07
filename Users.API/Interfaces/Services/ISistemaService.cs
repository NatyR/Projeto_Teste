using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Dto.Sistema;

namespace Users.API.Interfaces.Services
{
    public interface ISistemaService
    {
        Task<IEnumerable<SistemaDto>> GetAll();
        Task<SistemaDto> Get(long id);
        Task<SistemaDto> Add(SistemaAddDto sistema);
        Task<SistemaDto> Update(SistemaUpdateDto sistema);
        Task Delete(long id);
    }
}