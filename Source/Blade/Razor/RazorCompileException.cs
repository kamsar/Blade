using System;
using System.Linq;
using RazorEngine.Templating;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Razor
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="Not desirable; provides invalid ways to initialize for intended purpose")]
	public class RazorCompileException : Exception
	{
		public RazorCompileException(string path, TemplateCompilationException baseException) : base(GetCompileMessage(path, baseException)) { }

		private static string GetCompileMessage(string path, TemplateCompilationException baseException)
		{
			string message = string.Format("Unable to compile template {0}:\n{1}", path, string.Join("\n\n", baseException.Errors.Where(x => !x.IsWarning).Select(x => string.Format("{0} at\n{1}:{2}", x.ErrorText, x.FileName, x.Line))));

			return message;
		}
		
		protected  RazorCompileException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
