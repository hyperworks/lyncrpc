using System;

namespace LyncRPC
{
    internal static class LAssert
    {
        public delegate void Asserter (bool condition,string message);

        public static Asserter Pre = A (msg => new InvalidOperationException (msg));
        public static Asserter Arg = A (msg => new ArgumentException (msg));
        public static Asserter Lync = A (msg => new LyncStateException (msg));
        public static Asserter Post = A (msg => new InvalidOperationException (msg));

        private static Asserter A<T> (Func<string, T> builder) where T: Exception
        {
            // TODO: No way to construct T generic-ly with a message.
            return (condition, message) => {
                if (!condition) {
                    throw builder (message);
                }
            };
        }
    }
}

