using Accounts.API.Common.Annotations.Validations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Accounts.API.Common.Dto.Account
{
    public class AccountCsvDto
    {
        public long Convenio { get; set; }

        public string AdmissionDate { get; set; }

        public string Email { get; set; }

        public string Cpf { get; set; }

        //public string Rg { get; set; }

        //public string RgIssuer { get; set; }
        
        public string BirthDate { get; set; }

        public string MothersName { get; set; }

        //public string FathersName { get; set; }

        public string Name { get; set; }

        //public string CardName { get; set; }

        public string Neighborhood { get; set; }

        public string AddressNumber { get; set; }

        public string Street { get; set; }

        public string CityName { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string AddressComplement { get; set; }

        public string PhoneNumber{ get; set; }

        public string Cnpj { get; set; }
        public string CostCenterCode { get; set; }
        public string CostCenterName { get; set; }

        public string BranchCode{ get; set; }
        public string BranchName { get; set; }

        public string CardLimit { get; set; }

        public string RegistrationNumber { get; set; }
        //public string Gender { get; set; }
        //public string DeliveryUnit { get; set; }
        //public string DeliveryToEmployee { get; set; }
        public string Nacionality { get; set; }
        public string Occupation { get; set; }
    }
}
