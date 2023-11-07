using System;

namespace Accounts.API.Entities
{
    public class BlockedAccount
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long BranchId { get; set; }
        public string BranchName { get; set; }
        public long CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string Cpf { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime BlockedAt { get; set; }
        public decimal CardLimit { get; set; }
        public string CardNumber { get; set; }
    }
}
