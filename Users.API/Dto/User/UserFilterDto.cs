using System;

namespace Users.API.Dto.User
{
    public class UserFilterDto
    {
        public string SearchTerm { get; set; }
        public DateTime? CreationStartDate { get; set; }

        public DateTime? CreationEndDate { get; set; }

        public int? UserType { get; set; }

        public int SistemaId { get; set; }

        public int[]? GroupId { get; set; }

        public int[]? ShopId { get; set; }
    }
}
