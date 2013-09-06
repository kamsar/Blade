using System;
using Blade.Razor;
using Sitecore.Diagnostics;
using Blade.Utility;
using System.Web.Compilation;
using System.Collections.Generic;

namespace Blade.Views
{
	/// <summary>
	/// As with UserControlView and WebControlView, this one plays host to views based on a Razor view (.cshtml)
	/// </summary>
	/// <typeparam name="TModel">The model type</typeparam>
	public class RazorViewShim<TModel> : WebControlView<TModel>, IRazorViewShim
		where TModel : class
	{
		public string ViewPath { get; set; }

		protected override void RenderModel(System.Web.UI.HtmlTextWriter writer)
		{
			Assert.IsNotNull(ViewPath, "ViewPath was null or empty, and must point to a valid virtual path to a Razor rendering.");

			var viewData = new Dictionary<string, object>();
			viewData[MetadataConstants.RenderingParametersViewDataKey] = RenderingParameters;

			var renderer = new ViewRenderer();
			var result = renderer.RenderPartialViewToString(ViewPath, Model, viewData);

			writer.Write(result);
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
