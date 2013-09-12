using System.Linq;
using System.Web.Mvc;

namespace Blade
{
	/// <summary>
	/// A presenter specialization that is capable of handling XmlHttpRequest-originated requests in a special fashion
	/// </summary>
	/// <typeparam name="T">The model type</typeparam>
	public interface IXmlHttpRequestPresenter<in T>
	{
		/// <summary>
		/// Handles AJAX requests to the view (of any HTTP method). This is called after GetModel(), but may affect the values of the model.
		/// Most times this method will probably clear the output stream and say, return JSON or a HTML snippet.
		/// </summary>
		/// <param name="view">The view handling the UI</param>
		/// <param name="model">The model the view received (probably from GetModel())</param>
		/// <param name="controllerContext">(optional) a MVC ControllerContext to pass around the ModelState validation data in</param>
		void HandleXmlHttpRequest(IView view, T model, ControllerContext controllerContext);
	}
}
