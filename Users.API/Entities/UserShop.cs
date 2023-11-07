using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.API.Common.Enum.User;

namespace Users.API.Entities
{
    public class UserShop
    {
        public long ShopId { get; set; }
        public long ProfileId { get; set; }
        public long UserId { get; set; }

    }
}
