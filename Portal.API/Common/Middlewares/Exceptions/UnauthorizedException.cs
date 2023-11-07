using System;
using System.Runtime.Serialization;

namespace Portal.API.Common.Middlewares.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : this("Não autenticado.") { }
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
        protected UnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
