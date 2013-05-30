using System;
using Sitecore.Web.UI;
using Blade.Razor;
using Blade.Utility;
using Sitecore.Diagnostics;
using RazorEngine.Templating;

namespace Blade.Views
{
	/// <summary>
	/// Allows statically binding a Razor template as if it were a WebControl
	/// </summary>
	public class RazorTemplate : WebControl
	{
		public string Path { get; set; }
		public object Model { get; set; }

		protected override void DoRender(System.Web.UI.HtmlTextWriter output)
		{
			using (new RenderingDiagnostics(output, Path + " (statically bound)", Cacheable, VaryByData, VaryByDevice, VaryByLogin, VaryByParm, VaryByQueryString, VaryByUser, ClearOnIndexUpdate, GetCachingID()))
			{
				// get the template for this path
				var template = RazorViewCache.GetCompiledTemplate(Path);

				Assert.IsNotNull(template, "Path was null or empty, and must point to a valid virtual path to a Razor rendering.");

				if (Model != null) // if we have a non-null model we reflect the template and set its model via reflection if appropriate
				{
					var templateType = typeof(RazorRendering<>).MakeGenericType(Model.GetType());

					if (!templateType.IsInstanceOfType(template))
						throw new ArgumentException("Razor template was not convertible to expected type " + templateType.Name + ". The @inherits class needs to derive from RazorRendering<T> to use an explicit model.");

					var property = template.GetType().GetProperty("Model");
					property.SetValue(template, Model, null);
				}

				output.Write(template.Run(new ExecuteContext()));
			}
		}
	}
}
