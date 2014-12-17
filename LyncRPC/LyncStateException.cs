using System;
using LyncRPC;

namespace LyncRPC
{
    public class LyncStateException: InvalidOperationException
    {
        public LyncStateException () : base ()
        {
        }

        public LyncStateException (string message) : base (message)
        {
        }

        public LyncStateException (string message, Exception innerException) : base (message, innerException)
        {
        }
    }
}

