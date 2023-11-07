using Users.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Dto;

namespace Users.API.Interfaces.Repositories
{
    public interface IUserShopRepository
    {
        Task<UserShop> Add(UserShop userShop);
        Task<List<UserShop>> GetByUserId(long userId);
        Task Delete(long userId);
    }
}
