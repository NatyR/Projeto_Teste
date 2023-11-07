using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.API.Exceptions
{
    public class InvalidUserLoginException : Exception
    {
        public InvalidUserLoginException()
        {

        }
        public InvalidUserLoginException(string message) : base(message)
        {

        }
        public InvalidUserLoginException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
