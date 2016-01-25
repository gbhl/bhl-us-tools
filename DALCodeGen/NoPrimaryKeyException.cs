using System;

namespace DALCodeGen
{
    public class NoPrimaryKeyException : Exception
    {
        public NoPrimaryKeyException()
        {
        }

        public NoPrimaryKeyException(string message) : base(message)
        {
        }

        public NoPrimaryKeyException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
