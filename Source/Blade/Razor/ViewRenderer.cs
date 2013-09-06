using System.Collections.Generic;
using System.Linq;
using System;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;

namespace Blade.Razor
{
	/// <summary>
	/// Class that renders MVC views to a string using the
	/// standard MVC View Engine to render the view. 
	/// </summary>
	/// <remarks>
	/// All credit due to Rick Strahl for this: https://github.com/RickStrahl/WestwindToolkit/blob/master/Westwind.Web.Mvc/Utils/ViewRenderer.cs
	/// Also this lovely series of blog posts:
	/// http://www.west-wind.com/weblog/posts/2013/Jul/15/Rendering-ASPNET-MVC-Razor-Views-outside-of-MVC-revisited
	/// http://www.west-wind.com/weblog/posts/2012/May/30/Rendering-ASPNET-MVC-Views-to-String
	/// </remarks>
	internal class ViewRenderer
	{
		/// <summary>
		/// Required Controller Context
		/// </summary>
		protected ControllerContext Context { get; set; }

		/// <summary>
		/// Initializes the ViewRenderer with a Context.
		/// </summary>
		/// <param name="controllerContext">
		/// If you are running within the context of an ASP.NET MVC request pass in
		/// the controller's context. 
		/// Only leave out the context if no context is otherwise available.
		/// </param>
		public ViewRenderer(ControllerContext controllerContext = null)
		{
			// Create a known controller from HttpContext if no context is passed
			if (controllerContext == null)
			{
				if (HttpContext.Current != null)
					controllerContext = CreateController<EmptyController>().ControllerContext;
				else
					throw new InvalidOperationException(
						"ViewRenderer must run in the context of an ASP.NET " +
						"Application and requires HttpContext.Current to be present.");
			}
			Context = controllerContext;
		}

		/// <summary>
		/// Renders a full MVC view to a string. Will render with the full MVC
		/// View engine including running _ViewStart and merging into _Layout        
		/// </summary>
		/// <param name="viewPath">
		/// The path to the view to render. Either in same controller, shared by 
		/// name or as fully qualified ~/ path including extension
		/// </param>
		/// <param name="model">The model to render the view with</param>
		/// <param name="viewData">Any viewData key-value pairs to send</param>
		/// <returns>String of the rendered view or null on error</returns>
		public string RenderViewToString(string viewPath, object model = null, Dictionary<string, object> viewData = null)
		{
			return RenderViewToStringInternal(viewPath, model, false, viewData);
		}


		/// <summary>
		/// Renders a partial MVC view to string. Use this method to render
		/// a partial view that doesn't merge with _Layout and doesn't fire
		/// _ViewStart.
		/// </summary>
		/// <param name="viewPath">
		/// The path to the view to render. Either in same controller, shared by 
		/// name or as fully qualified ~/ path including extension
		/// </param>
		/// <param name="model">The model to pass to the viewRenderer</param>
		/// <param name="viewData">Any viewData key-value pairs to send</param>
		/// <returns>String of the rendered view or null on error</returns>
		public string RenderPartialViewToString(string viewPath, object model = null, Dictionary<string, object> viewData = null)
		{
			return RenderViewToStringInternal(viewPath, model, true, viewData);
		}

		/// <summary>
		/// Internal method that handles rendering of either partial or 
		/// or full views.
		/// </summary>
		/// <param name="viewPath">
		/// The path to the view to render. Either in same controller, shared by 
		/// name or as fully qualified ~/ path including extension
		/// </param>
		/// <param name="model">Model to render the view with</param>
		/// <param name="partial">Determines whether to render a full or partial view</param>
		/// <param name="viewData">Any viewData key-value pairs to send</param>
		/// <returns>String of the rendered view</returns>
		private string RenderViewToStringInternal(string viewPath, object model, bool partial = false, Dictionary<string, object> viewData = null)
		{
			// first find the ViewEngine for this view
			ViewEngineResult viewEngineResult;
			if (partial)
				viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
			else
				viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

			if (viewEngineResult == null)
				throw new FileNotFoundException("Unable to find a view matching " + viewPath);
			
			if (viewEngineResult.View == null)
			{
				throw new FileNotFoundException("Unable to find a view matching " + viewPath + "\nTried: \n" + string.Join("\n", viewEngineResult.SearchedLocations));
			}

			// get the view and attach the model to view data
			var view = viewEngineResult.View;
			Context.Controller.ViewData.Clear();
			Context.Controller.ViewData.Model = model;
			if (viewData != null)
			{
				foreach (var pair in viewData)
				{
					Context.Controller.ViewData.Add(pair);
				}
			}

			string result;

			using (var sw = new StringWriter())
			{
				var ctx = new ViewContext(Context, view,
										  Context.Controller.ViewData,
										  Context.Controller.TempData,
										  sw);
				view.Render(ctx, sw);
				result = sw.ToString();
			}

			return result;
		}


		/// <summary>
		/// Creates an instance of an MVC controller from scratch 
		/// when no existing ControllerContext is present       
		/// </summary>
		/// <typeparam name="T">Type of the controller to create</typeparam>
		/// <returns>Controller Context for T</returns>
		/// <exception cref="InvalidOperationException">thrown if HttpContext not available</exception>
		public static T CreateController<T>(RouteData routeData = null)
					where T : Controller, new()
		{
			// create a disconnected controller instance
			T controller = new T();

			// get context wrapper from HttpContext if available
			HttpContextBase wrapper;
			if (HttpContext.Current != null)
				wrapper = new HttpContextWrapper(HttpContext.Current);
			else
				throw new InvalidOperationException("Can't create Controller Context if no active HttpContext instance is available.");

			if (routeData == null)
				routeData = new RouteData();

			// add the controller routing if not existing
			if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
				routeData.Values.Add("controller", controller.GetType().Name
															.ToLower()
															.Replace("controller", ""));

			controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
			return controller;
		}

	}

	/// <summary>
	/// Empty MVC Controller instance used to 
	/// instantiate and provide a new ControllerContext
	/// for the ViewRenderer
	/// </summary>
	public class EmptyController : Controller
	{
	}
}