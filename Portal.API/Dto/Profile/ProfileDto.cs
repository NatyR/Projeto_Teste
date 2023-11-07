using Portal.API.Dto.ProfileMenu;
using Portal.API.Dto.Sistema;
using System.Collections.Generic;

namespace Portal.API.Dto.Profile
{
    public class PortalProfileDto
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public long SistemaId { get; set; }
        public SistemaDto Sistema { get; set; }

        public List<ProfileMenuDto> ProfileMenu { get; set; }
        public string Observation { get; set; }

    }
}
