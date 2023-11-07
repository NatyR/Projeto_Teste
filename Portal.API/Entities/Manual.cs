using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Entities
{
    public class Manual
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ManualType { get; set; }

        public string Url { get; set; }
        public string Description { get; set; }

        public int Order { get; set; }
        public string Status { get; set; }
    }
}
