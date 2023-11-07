using Accounts.API.Common.Enum;
using Castle.Core.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Entities
{
    public class Solicitation
    {
        public long Id { get; set; }
        public SolicitationTypeEnum SolicitationType { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public long UserId { get; set; }
        public string Status { get; set; }
        public string Observation { get; set; } 
        public long? ClientId { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopDocument { get; set; }
        public string Name { get; set; }
        public string Cpf { get; set; }
        public decimal PreviousLimit { get; set; }
        public decimal NewLimit { get; set; }
        public int BlockTypeId { get; set; }
        public string BlockType { get; set; }
        public string AccountStatus { get; set; }
        public string CardStatus { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
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
        public Solicitation()
        {
            RequestedAt = DateTime.Now;
            Status = "PENDENTE";
            Observation = "";
        }
        public Solicitation(SolicitationTypeEnum type, long userId, long? customerId, long shopId, string customerName, string customerCpf, decimal customerCardLimit, decimal customerNewLimit, string customerAccountStatus, string customerCardStatus, string reasonDescription,string observation)
        {
            SolicitationType = type;
            UserId = userId;
            ClientId = customerId;
            ShopId = shopId;
            Name = customerName;
            Cpf = customerCpf;
            PreviousLimit = customerCardLimit;
            NewLimit = customerNewLimit;
            AccountStatus = customerAccountStatus;
            CardStatus = customerCardStatus;
            BlockType = reasonDescription;
            RequestedAt = DateTime.Now;
            Status = "PENDENTE";
            Observation = observation;
        }
        public Solicitation(SolicitationTypeEnum type, long userId, long? customerId, long shopId, string customerName, string customerCpf, decimal customerCardLimit, decimal customerNewLimit, string customerAccountStatus, string customerCardStatus, string reasonDescription)
        {
            SolicitationType = type;
            UserId = userId;
            ClientId = customerId;
            ShopId = shopId;
            Name = customerName;
            Cpf = customerCpf;
            PreviousLimit = customerCardLimit;
            NewLimit = customerNewLimit;
            AccountStatus = customerAccountStatus;
            CardStatus = customerCardStatus;
            BlockType = reasonDescription;
            RequestedAt = DateTime.Now;
            Status = "PENDENTE";
            Observation = "";
        }
        public Solicitation(SolicitationTypeEnum type, long userId, long shopId, string groupName, decimal oldLimit, decimal newLimit)
        {
            SolicitationType = type;
            UserId = userId;
            ShopId = shopId;
            Name = groupName;
            PreviousLimit = oldLimit;
            NewLimit = newLimit;
            RequestedAt = DateTime.Now;
            Status = "PENDENTE";
            Observation = "";
        }

    }



}
