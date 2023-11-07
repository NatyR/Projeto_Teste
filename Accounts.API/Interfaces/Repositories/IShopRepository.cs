using Accounts.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Interfaces.Repositories
{
    public interface IShopRepository
    {
        Task<List<Shop>> GetAllByGroup(long group);
        Task<List<Shop>> GetAllByGroups(long[] group);
        Task<List<Shop>> GetAllByShop(long shop);
        Task<List<GroupLimit>> GetLimitsByGroup(long group);
        Task<Shop> GetByCnpj(string cnpj);
        Task<Shop> GetById(long id);
    }
}
