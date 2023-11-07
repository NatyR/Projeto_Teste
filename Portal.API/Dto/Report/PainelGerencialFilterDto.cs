using System;

namespace Portal.API.Dto.Report
{
    public class PainelGerencialFilterDto
    {
        public string SearchTerm { get; set; }
        public DateTime? SolicitationStartDate { get; set; }

        public DateTime? SolicitationEndDate { get; set; }
        public int[]? GroupId { get; set; }

        public int[]? ShopId { get; set; }
    }
}
