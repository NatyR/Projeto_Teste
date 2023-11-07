
using Portal.API.Dto.Sistema;

namespace Portal.API.Dto.Menu
{
    public class MenuDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long? ParentId { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }

        public long SistemaId { get; set; }

        public string Url { get; set; }

        public MenuDto Parent { get; set; }

        public SistemaDto Sistema { get; set; }
        public bool CanInsert { get; set; }
        public bool CanDelete { get; set; }
        public bool CanUpdate { get; set; }

    }
}
