using System;

namespace Accounts.API.Common.Dto.Shop
{
    public class GroupLimitDto
    {
        public DateTime EventDate { get; set; }

        public decimal PreviousLimit { get; set; }
        public decimal ChangeValue { get; set; }
        public decimal NewLimit { get; set; }
    }
}
