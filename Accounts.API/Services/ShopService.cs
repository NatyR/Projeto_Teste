using Accounts.API.Common.Dto.Shop;
using Accounts.API.Interfaces.Repositories;
using Accounts.API.Interfaces.Services;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.API.Services
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMapper _mapper;

        public ShopService(IShopRepository shopRepository, IMapper mapper)
        {
            _shopRepository = shopRepository;
            _mapper = mapper;
        }
        public async Task<List<ShopDto>> GetAllByGroup(long group)
        {
            var shops = await _shopRepository.GetAllByGroup(group);
            return _mapper.Map<List<ShopDto>>(shops);
        }

        public async Task<List<ShopDto>> GetAllByGroups(long[] group)
        {
            var shops = await _shopRepository.GetAllByGroups(group);
            return _mapper.Map<List<ShopDto>>(shops);
        }

          public async Task<List<ShopDto>> GetAllByShop(long shop)
        {
            var shops = await _shopRepository.GetAllByShop(shop);
            return _mapper.Map<List<ShopDto>>(shops);
        }

        public async Task<List<GroupLimitDto>> GetLimitsByGroup(long group)
        {
            var data = await _shopRepository.GetLimitsByGroup(group);
            return _mapper.Map<List<GroupLimitDto>>(data);
        }

        public async Task<ShopDto> GetByCnpj(string cnpj)
        {
            return _mapper.Map<ShopDto>(await _shopRepository.GetByCnpj(cnpj));
        }

        public async Task<ShopDto> GetById(long id)
        {
            return _mapper.Map<ShopDto>(await _shopRepository.GetById(id));
        }
    }
}
