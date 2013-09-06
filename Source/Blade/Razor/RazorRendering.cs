using System.Collections.Specialized;
using System.Web.Mvc;

namespace Blade.Razor
{
	/// <summary>
	/// This is the standard base class from which Razor templates inherit. Any properties defined here will be available to Razor.
	/// </summary>
	/// <typeparam name="TModel">The type of model this template renders (may be "dynamic" if necessary)</typeparam>
	public abstract class RazorRendering<TModel> : WebViewPage<TModel>
	{
		/// <summary>
		/// Any parameters set on the rendering in Sitecore. Not applicable to partial views.
		/// </summary>
		public NameValueCollection RenderingParameters { get { return (ViewData[MetadataConstants.RenderingParametersViewDataKey] as NameValueCollection) ?? new NameValueCollection(); } }

		/// <summary>
		/// Checks if the page is in inline edit mode (by an author)
		/// </summary>
		public bool IsEditing { get { return Sitecore.Context.PageMode.IsPageEditor; } }

		/// <summary>
		/// Checks if the page is being viewed in preview mode
		/// </summary>
		protected bool IsPreviewing { get { return Sitecore.Context.PageMode.IsPreview; } }
	}
}
