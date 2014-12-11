using System;

namespace LyncRPC
{
	internal static class LAssert
	{
		public delegate void Asserter(bool condition, string message);

		public static Asserter Pre = A<InvalidOperationException>();
		public static Asserter Post = A<InvalidOperationException>();

		private static Asserter A<T>() where T: Exception {
			// TODO: No way to construct T generic-ly with a message.
			return (condition, message) => {
				if (!condition) {
					throw new Exception(message);
				}
			};
		}
	}
}

