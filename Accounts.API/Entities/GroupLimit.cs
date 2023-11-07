using System;

namespace Accounts.API.Entities
{
    public class GroupLimit
    {
        public DateTime EventDate { get; set; }

        public decimal PreviousLimit { get; set; }
        public decimal ChangeValue { get; set; }
        public decimal NewLimit { get; set; }

    }
}
