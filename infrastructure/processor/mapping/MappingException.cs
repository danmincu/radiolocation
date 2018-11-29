using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Mapping
{
    [Serializable]
    [ExcludeFromCodeCoverage] // Nothing to test
    public class MappingException : Exception
    {
        public MappingException() : base() { }
        public MappingException(string message) : base(message) { }
        public MappingException(string message, Exception exception) : base(message, exception) { }
        protected MappingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
