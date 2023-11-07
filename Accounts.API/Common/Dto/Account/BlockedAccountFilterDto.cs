using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Common.Dto.Account
{
    public class BlockedAccountFilterDto
    {
        public int[] BlockTypeId { get; set; }
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int[]? GroupId { get; set; }

        public int[]? ShopId { get; set; }

    }
}
