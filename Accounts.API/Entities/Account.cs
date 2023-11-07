using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Entities
{
    public class Account
    {
        public long Id { get; set; }
        public long IdConta
        {
            get { return this.Id; }
        }
        public string Gender { get; set; }
        public int? TimeEmployed { get; set; }
        public string Rg { get; set; }
        public int? TimeResidence { get; set; }
        public string BirthState { get; set; }
        public string BirthCity { get; set; }
        public string Email { get; set; }
        public int? ScholarshipId { get; set; }
        public int? MaritalStatusId { get; set; }
        public string Cpf { get; set; }
        public int? OccupationId { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? ResidenceTypeId { get; set; }
        public string RgIssuer { get; set; }
        public DateTime? RgIssuedDate { get; set; }
        public string Nationality { get; set; }
        public int? ItfSent { get; set; }
        public string IssuePlace { get; set; }
        public string Cnh { get; set; }
        public string MothersName { get; set; }
        public string ContractImpStatus { get; set; }
        public DateTime? ContractImpLastDate { get; set; }
        public long Convenio { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int? BlockTypeId { get; set; }
        public long OriginalConvenio { get; set; }
        public long HomeAddressId { get; set; }
        public string Neighborhood { get; set; }
        public string AddressNumber { get; set; }
        public string Street { get; set; }
        public string CityName { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string AddressComplement { get; set; }
        public string PhoneNumber { get; set; }
        public string CostCenter { get; set; }
        public string Branch { get; set; }

        public long? BranchId { get; set; }
        public long? CostCenterId { get; set; }
        public string RegistrationNumber { get; set; }
        public decimal CardLimit { get; set; }
        public DateTime CardLimitUpdatedAt { get; set; }
        private string _CardNumber;
        public string CardNumber
        {
            get { return !String.IsNullOrEmpty(_CardNumber) && _CardNumber.Length >= 12 ? "****" + _CardNumber.Substring(4, 8) + "****" : ""; }

            set { _CardNumber = value; }
        }
        public string CardStatus { get; set; }
        public string Reason { get; set; }
    }
}
