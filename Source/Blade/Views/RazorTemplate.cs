using System.Web.UI;
using Sitecore.Web.UI;
using Blade.Razor;
using Blade.Utility;
using Sitecore.Diagnostics;

namespace Blade.Views
{
	/// <summary>
	/// Allows statically binding a Razor template as if it were a WebControl
	/// </summary>
	public class RazorTemplate : WebControl
	{
		/// <summary>
		/// Virtual path to the Razor file to render
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// The Model for the Razor view. The model will be null if this is unset.
		/// </summary>
		public object Model { get; set; }

		protected override void DoRender(HtmlTextWriter output)
		{
			using (new RenderingDiagnostics(output, Path + " (statically bound)", Cacheable, VaryByData, VaryByDevice, VaryByLogin, VaryByParm, VaryByQueryString, VaryByUser, ClearOnIndexUpdate, GetCachingID()))
			{
				Assert.IsNotNull(Path, "Path was null or empty, and must point to a valid virtual path to a Razor rendering.");

				var renderer = new ViewRenderer();
				var result = renderer.RenderPartialViewToString(Path, Model);
				
				output.Write(result);
			}
		}
	}
}
