using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Common.Dto.Account
{
    public class BlockedAccountDto
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

        private string _CardNumber;
        public string CardNumber
        {
            get => CardNumberObfuscation();

            set { _CardNumber = value; }
        }

        public string CardNumberObfuscation()
        {
            return !String.IsNullOrEmpty(_CardNumber) && _CardNumber.Length >= 16 ? "****." 
                                                      + _CardNumber.Substring(4, 4) 
                                                      + "." 
                                                      + _CardNumber.Substring(8, 4) 
                                                      + ".****" : "";
        }

    }
}
