using System;

namespace CrypticEngine.Exception
{
    [Serializable()]
    public class NullCrypticObjectException : System.Exception
    {
        public NullCrypticObjectException() : base() { }
        public NullCrypticObjectException(string message) : base(message) { }
        public NullCrypticObjectException(string message, System.Exception inner) : base(message, inner) { }

        protected NullCrypticObjectException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
