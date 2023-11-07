using Accounts.API.Common.Dto.Branch;
using Accounts.API.Common.Dto.Shop;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Accounts.API.Interfaces.Services
{
    public interface IShopService
    {
        Task<List<ShopDto>> GetAllByGroup(long group);
        Task<List<ShopDto>> GetAllByGroups(long[] group);
        Task<List<ShopDto>> GetAllByShop(long shop);
        Task<List<GroupLimitDto>> GetLimitsByGroup(long group);
        Task<ShopDto> GetByCnpj(string cnpj);
        Task<ShopDto> GetById(long id);

    }
}
