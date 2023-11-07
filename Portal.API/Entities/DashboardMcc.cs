using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Entities
{
    public class DashboardMcc
    {
        public int IdGroup { get; set; }
        public int IdConvenio { get; set; }
        public string Description { get; set; }
        public decimal Percentage { get; set; }
    }
}
