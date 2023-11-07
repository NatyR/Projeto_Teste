using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Entities
{
    public class Branch
    {
        public long Id { get; set; }
        public long ConvenioId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public long AddressId { get; set; }
        public string UseConvenioAddress { get; set; }
        public string StreetAddress { get; set; }
        public string AddressNumber { get; set; }
        public string AddressComplement { get; set; }
        public string Neighborhood { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string ZipCode { get; set; }
    }
}
