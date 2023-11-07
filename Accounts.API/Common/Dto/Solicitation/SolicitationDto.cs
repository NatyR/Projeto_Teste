using Accounts.API.Common.Enum;
using System;

namespace Accounts.API.Common.Dto.Solicitation
{
    public class SolicitationDto
    {
        public long Id { get; set; }
        public SolicitationTypeEnum SolicitationType { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public long UserId { get; set; }
        public string Status { get; set; }
        public string Observation { get; set; }
        public long ClientId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopDocument { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Cpf { get; set; }
        public decimal PreviousLimit { get; set; }
        public decimal NewLimit { get; set; }
        public int BlockTypeId { get; set; }
        public string BlockType { get; set; }
        public string AccountStatus { get; set; }
        public string CardStatus { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string SolicitationTypeDescription { get; set; }

        public string Rg { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string AddressNumber { get; set; }
        public string AddressComplement { get; set; }
        public string Neighborhood { get; set; }
        public string CityName { get; set; }
        public string State { get; set; }
        public long? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public long? BranchId { get; set; }
        public string BranchName { get; set; }


    }
}
