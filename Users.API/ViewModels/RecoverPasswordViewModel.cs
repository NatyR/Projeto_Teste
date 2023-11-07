using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.API.ViewModels
{
    public class RecoverPasswordViewModel
    {
        public string Password { get; set; }
        public string Token { get; set; }
    }
}
