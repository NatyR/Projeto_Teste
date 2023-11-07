using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Entities
{
    public class Shop
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int IdGroup { get; set; }
        public string GroupName { get; set; }
        public int ClosingDay { get; set; }

        public string StreetAddress { get; set; }
        public string AddressNumber { get; set; }
        public string AddressComplement { get; set; }
        public string Neighborhood { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string ZipCode { get; set; }
        public string Cnpj { get; set; }
        public decimal Limit { get; set; }
        public decimal AvailableLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public DateTime NextDateExpiration { get; set; }
    }
}
