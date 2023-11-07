using Users.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Dto;
using Dapper.Oracle;
using System.Data;
using Users.API.Dto.Acesso;

namespace Users.API.Interfaces.Repositories
{
    public interface IAcessoRepository
    {
        public Task<Acesso> Add(Acesso acesso);
        public Task Delete(long id);
        public Task<Acesso> Get(long id);
        public Task<IEnumerable<Acesso>> GetAll();
        public Task<ResponseDto<AcessoDto>> GetAllPaged(AcessoFilterDto filterOptions, int limit, int skip, string order);
        public Task<List<AcessoDto>> GetAll(AcessoFilterDto filterOptions);
    }
}
