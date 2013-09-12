using System;

namespace Blade
{
	[Serializable]
	public class PresenterResolutionException : Exception
	{
		public PresenterResolutionException() { }
		public PresenterResolutionException(string message) : base(message) { }
		public PresenterResolutionException(string message, Exception inner) : base(message, inner) { }
		protected PresenterResolutionException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
