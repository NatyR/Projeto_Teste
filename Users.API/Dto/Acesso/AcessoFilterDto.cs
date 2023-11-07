using System;

namespace Users.API.Dto.Acesso
{
    public class AcessoFilterDto
    {
        public string SearchTerm { get; set; }
        public DateTime? AccessStartDate { get; set; }

        public DateTime? AccessEndDate { get; set; }
    }
}
