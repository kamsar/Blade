using System.Linq;
using System.Web.Mvc;

namespace Blade
{
	/// <summary>
	/// A presenter specialization that is capable of handling HTTP POST in a special fashion (e.g. a form submit)
	/// </summary>
	/// <typeparam name="T">The model type</typeparam>
	public interface IPostBackPresenter<in T>
	{
		/// <summary>
		/// Handles post back to the view. This is called after GetModel(), but may affect the values of the model.
		/// </summary>
		/// <param name="view">The view handling the UI</param>
		/// <param name="model">The model the view received (probably from GetModel())</param>
		/// <param name="controllerContext">(optional) a MVC ControllerContext to pass around the ModelState validation data in</param>
		void HandlePostBack(IView view, T model, ControllerContext controllerContext);
	}
}
