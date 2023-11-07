using Users.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Dto;
using Dapper.Oracle;
using System.Data;
using Users.API.Dto.Login;

namespace Users.API.Interfaces.Repositories
{
    public interface ILoginRepository
    {
        public Task<Login> Add(Login login);
        public Task Delete(long id);
        public Task<Login> Get(long id);
        public Task<IEnumerable<Login>> GetAll();
        public Task<ResponseDto<Login>> GetAllPaged(LoginFilterDto filterOptions, int limit, int skip, string order);
        public Task<Login> GetById(long id);
     
    }
}
