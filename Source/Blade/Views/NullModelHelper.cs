using System;
using System.Web.UI;

namespace Blade.Views
{
	/// <summary>
	/// Global definition of how to render a control when the model is null (across web controls, user controls, and razor controls)
	/// </summary>
	internal static class NullModelHelper
	{
		internal static void RenderNullModelMessage(HtmlTextWriter writer, string viewName, string dataSource, Type presenterType, Type modelType)
		{
			writer.AddAttribute("style", "border: 1px dashed red; padding: .5em;");
			writer.RenderBeginTag("div");

			writer.Write("<p><strong>Hidden View:</strong> {0}</p>", viewName);

			writer.Write("<p>View was automatically hidden because its model type <span style=\"font-family: monospace\">{0}</span> could not be resolved. Possible causes:</p>", modelType.Name);

			writer.RenderBeginTag("ul");

			if (!string.IsNullOrWhiteSpace(dataSource))
				writer.Write("<li>The view's data source value, <span style=\"font-family: monospace\">{0}</span>, may not point to valid item(s).</li>", dataSource);

			writer.Write("<li>The presenter, <span style=\"font-family: monospace\">{0}</span>, returned null for the model. This would usually indicate either the data source cannot be converted to the model type, or that no data exists to display.</li>", presenterType.Name);

			writer.RenderEndTag(); // ul

			writer.Write("<p><em>This message is only displayed in preview or edit mode, and will not appear to end users.</em></p>");

			writer.RenderEndTag(); // div
		}
	}
}
