using System;
using Sitecore.Web.UI;
using System.Collections.Specialized;
using System.Web.UI;
using Blade.Views;

namespace Blade.Razor
{
	/// <summary>
	/// This is a Sitecore Rendering Type class that tells Sitecore how to handle a Razor view as a Rendering when it comes across one
	/// </summary>
	/// <remarks>
	/// Must be registered in the renderingControls section of the Sitecore configuration. A template that derives from an existing Rendering template
	/// must also be created in Sitecore with a name matching the entry in the renderingControls config.
	/// </remarks>
	public class SitecoreRazorRenderingType : RenderingType
	{
		public override Control GetControl(NameValueCollection parameters, bool assert)
		{
			var viewPath = parameters["viewPath"];

			var control = GetControl(viewPath);

			Sitecore.Diagnostics.Assert.IsNotNull(control, "Resolved Razor control was null");

			((WebControl)control).Parameters = parameters["properties"];
			
			return control;
		}

		/// <summary>
		/// Gets a Control that will render a given Razor view path
		/// </summary>
		public static Control GetControl(string viewPath)
		{
			Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(viewPath, "ViewPath cannot be empty. The Rendering item in Sitecore needs to have a view path set.");

			Type viewModelType = RazorViewCache.GetViewModelType(viewPath);

			var renderingType = typeof(RazorViewShim<>).MakeGenericType(viewModelType);

			var shim = Activator.CreateInstance(renderingType) as IRazorViewShim;
			var template = RazorViewCache.GetCompiledTemplate(viewPath);

			// make the template aware of its parent control
			((IBladeTemplateMetadata)template).RenderingShim = shim;

			shim.Template = template;
			shim.ViewPath = viewPath;

			return shim as Control;
		}
	}
}
