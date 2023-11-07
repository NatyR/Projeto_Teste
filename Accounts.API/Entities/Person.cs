using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Entities
{
    public class Person
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int? ItfSent { get; set; }
    }
}
