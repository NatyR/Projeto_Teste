using System;

namespace Accounts.API.Common.Dto.Solicitation
{
    public class SolicitationFilterDto
    {
        public string SearchTerm { get; set; }
        public DateTime? SolicitationStartDate { get; set; }

        public DateTime? SolicitationEndDate { get; set; }

        public string SolicitationStatus { get; set; }

        public int? SolicitationType { get; set; }

        public int? UserType { get; set; }

        public int[]? GroupId { get; set; }

        public int[]? ShopId { get; set; }
    }
}
