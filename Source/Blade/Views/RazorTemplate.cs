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
		public string Path { get; set; }
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
