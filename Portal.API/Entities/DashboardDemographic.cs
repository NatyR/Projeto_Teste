using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.API.Entities
{
    public class DashboardDemographic
    {
        public int IdGroup { get; set; }
        public int IdConvenio { get; set; }
        public int Employees { get; set; }
        public int AgeFrom18To24 { get; set; }
        public int AgeFrom25To34 { get; set; }
        public int AgeFrom35To44 { get; set; }
        public int AgeFrom45To54 { get; set; }
        public int AgeFrom55To64 { get; set; }
        public int AgeFrom65To99 { get; set; }
        public int AgeUnknown { get; set; }
        public int SexMaleQuantity { get; set; }
        public int SexMalePercentage { get; set; }
        public int SexFemaleQuantity { get; set; }
        public int SexFemalePercentage { get; set; }
        public int SexOtherQuantity { get; set; }
        public int SexOtherPercentage { get; set; }
        public int SexUnknownQuantity { get; set; }
        public int SexUnknownPercentage { get; set; }
    }
}
