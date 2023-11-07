using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Entities
{
    public class CostCenter
    {
        public long Id { get; set; }
        public long ConvenioId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public long AddressId { get; set; }
        public string InternalCode { get; set; }
        public long BranchId { get; set; }
        public string UseConvenioAddress { get; set; }
        public string UseBranchAddress { get; set; }
    }
}
