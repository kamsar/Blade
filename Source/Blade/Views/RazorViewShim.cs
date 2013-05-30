using System;
using Blade.Razor;
using RazorEngine.Templating;
using Sitecore.Diagnostics;
using Blade.Utility;

namespace Blade.Views
{
	/// <summary>
	/// As with UserControlView and WebControlView, this one plays host to views based on a Razor view (.cshtml)
	/// </summary>
	/// <typeparam name="TModel">The model type</typeparam>
	public class RazorViewShim<TModel> : WebControlView<TModel>, IRazorViewShim
		where TModel : class
	{
		public ITemplate Template { get; set; }
		public string ViewPath { get; set; }

		protected override void RenderModel(System.Web.UI.HtmlTextWriter writer)
		{
			Assert.IsNotNull(Template, "ViewPath was null or empty, and must point to a valid virtual path to a Razor rendering.");

			var modelTemplate = Template as RazorRendering<TModel>;

			Assert.IsNotNull(modelTemplate, "Razor template was not convertible to expected type " + typeof(RazorRendering<TModel>).Name + ". The @inherits class needs to derive from RazorRendering<T>.");

// ReSharper disable PossibleNullReferenceException
			modelTemplate.RenderingParameters = RenderingParameters;
// ReSharper restore PossibleNullReferenceException
			if (!RazorUtility.TrySetModel(Template, Model))
				throw new ArgumentException("Model of type " + typeof(TModel) + " could not be set on template " + ViewPath);

			writer.Write(Template.Run(new ExecuteContext()));
		}

		protected override void RenderWhenModelIsNull(System.Web.UI.HtmlTextWriter writer)
		{
			if (IsEditing || IsPreviewing)
			{
				NullModelHelper.RenderNullModelMessage(writer, ViewPath, DataSource, PresenterFactory.GetPresenter<TModel>().GetType(), typeof(TModel));
			}
		}

		protected override void DoRender(System.Web.UI.HtmlTextWriter output)
		{
			using (new RenderingDiagnostics(output, ViewPath, Cacheable, VaryByData, VaryByDevice, VaryByLogin, VaryByParm, VaryByQueryString, VaryByUser, ClearOnIndexUpdate, GetCachingID()))
			{
				if (Model != null)
					RenderModel(output);
				else
					RenderWhenModelIsNull(output);
			}
		}

		protected override string GetCachingID()
		{
			return ViewPath;
		}
	}
}
