using System;
using System.Web;
using System.Web.UI;
using System.Collections.Specialized;

namespace Blade
{
	public interface IView
	{
		/// <summary>
		/// Data source path/query passed to the view from the rendering engine, if any
		/// </summary>
		string DataSource { get; }

		/// <summary>
		/// The Page the view lives on
		/// </summary>
		[Obsolete("The Page property inhibits testability. You should use the ViewContext property instead.")]
		Page Page { get; }

		/// <summary>
		/// The Sitecore rendering properties associated with the view, if any
		/// </summary>
		NameValueCollection ViewProperties { get; }

        /// <summary>
        /// The HttpContext that the view is being rendered under
        /// </summary>
        HttpContextBase ViewContext { get; }
	}
}
