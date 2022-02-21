using System;

namespace CrypticEngine.Exception
{
    [Serializable()]
    public class DuplicateCrypticObjectException : System.Exception
    {
        public DuplicateCrypticObjectException() : base() { }
        public DuplicateCrypticObjectException(string message) : base(message) { }
        public DuplicateCrypticObjectException(string message, System.Exception inner) : base(message, inner) { }

        protected DuplicateCrypticObjectException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
