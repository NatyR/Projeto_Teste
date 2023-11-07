using System;

namespace Users.API.Dto.Login
{
    public class LoginFilterDto
    {
        public string SearchTerm { get; set; }
        public DateTime? LoginStartDate { get; set; }

        public DateTime? LoginEndDate { get; set; }
    }
}
