using System;

namespace Infra.Eevents
{
    public class InvalidEventSchemaException : Exception
    {
        public InvalidEventSchemaException() : base()
        {
        }

        public InvalidEventSchemaException(string message) : base(message)
        {
        }

        public InvalidEventSchemaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
