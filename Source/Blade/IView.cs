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
		Page Page { get; }

		/// <summary>
		/// The Sitecore rendering properties associated with the view, if any
		/// </summary>
		NameValueCollection ViewProperties { get; }
	}
}
