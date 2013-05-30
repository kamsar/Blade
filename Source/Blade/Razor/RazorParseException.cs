using System;
using RazorEngine.Templating;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Razor
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="Not desirable; provides invalid ways to initialize for intended purpose")]
	public class RazorParseException : Exception
	{
		public RazorParseException(string path, TemplateParsingException baseException) : base(GetCompileMessage(path, baseException)) { }

		private static string GetCompileMessage(string path, TemplateParsingException baseException)
		{
			string message = string.Format("Unable to parse template {0}:\n{1} (Line {2}:{3})", path, baseException.Message, baseException.Line, baseException.Column);

			return message;
		}

		protected RazorParseException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
