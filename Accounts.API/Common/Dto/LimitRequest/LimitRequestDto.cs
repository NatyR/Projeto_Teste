using System;

namespace Accounts.API.Common.Dto.LimitRequest
{
    public class LimitRequestDto
    {
        public long Id { get; set; }

        public long ShopId { get; set; }
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }

        public string Cpf { get; set; }
        public decimal PreviousLimit { get; set; }
        public decimal NewLimit { get; set; }
        public long UserId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StatusChangedAt { get; set; }
        public long ApproverId { get; set; }
    }
}
