using Portal.API.Common.Dto;
using Portal.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal.API.Interfaces.Repositories
{
    public interface IAcessoRepository
    {
        public Task<Acesso> Add(Acesso acesso);
        public Task Delete(long id);
        public Task<Acesso> Get(long id);
        public Task<IEnumerable<Acesso>> GetAll();
        public Task<ResponseDto<Acesso>> GetAllPaged(int limit, int skip, string search, string order);
        public Task<Acesso> GetById(long id);
    }
}
