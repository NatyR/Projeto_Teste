using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Entities
{
    public class Menu
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long? ParentId { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }

        public long SistemaId { get; set; }

        public string Url { get; set; }

        public Menu Parent { get; set; }
        public Sistema Sistema { get; set; }
        public bool CanInsert { get; set; }
        public bool CanDelete { get; set; }
        public bool CanUpdate { get; set; }

    }
}
