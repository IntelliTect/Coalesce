using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public sealed class CoalesceModelException : Exception
    {
        public CoalesceModelException() : base()
        {
        }

        public CoalesceModelException(string? message) : base(message)
        {
        }

        public CoalesceModelException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
