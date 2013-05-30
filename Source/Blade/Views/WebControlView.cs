using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using Sitecore.Web.UI;
using Blade.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Views
{
	/// <summary>
	/// Provides basic logic to content rendering controls to assist in resolving their
	/// data source item and the entity version of their data source. Also by default provides
	/// and EditFrame implementation for the control in PageEdit mode.
	/// </summary>
	/// <remarks>
	/// By default when using this control,
	/// - ViewState is disabled
	/// - If the datasource cannot be converted to the generic entity type specified, the control will have its visibility set to false and Render() will not be called.
	/// - Cacheable is true with VaryByData enabled
	/// - An EditFrame will be rendered around the content of the control when in PageEdit mode
	/// 
	/// To disable any of these functionalities, implement a constructor in your derived class and set the properties as desired.
	/// </remarks>
	[ParseChildren(false)]
	public abstract class WebControlView<TModel> : WebControl, IView
		where TModel : class
	{
		private TModel _model;
		protected readonly IPresenterFactory PresenterFactory;

		protected WebControlView() : this(PresenterResolver.Current)
		{
		}

		protected WebControlView(IPresenterFactory presenterFactory)
		{
			PresenterFactory = presenterFactory;
		}

		/// <summary>
		/// Checks if the page is in inline edit mode (by an author)
		/// </summary>
		protected bool IsEditing { get { return Sitecore.Context.PageMode.IsPageEditor; } }

		/// <summary>
		/// Checks if the page is being viewed in preview mode
		/// </summary>
		protected bool IsPreviewing { get { return Sitecore.Context.PageMode.IsPreview; } } 

		/// <summary>
		/// This is the required entry point from Sitecore's WebControl, which enables support
		/// for Sitecore's output cache model.
		/// </summary>
		/// <param name="output">The HtmlTextWriter for the Page/Response</param>
		protected override void DoRender(HtmlTextWriter output)
		{
			using (new RenderingDiagnostics(output, GetType().FullName, Cacheable, VaryByData, VaryByDevice, VaryByLogin, VaryByParm, VaryByQueryString, VaryByUser, ClearOnIndexUpdate, GetCachingID()))
			{
				if (Model != null)
					RenderModel(output);
				else
					RenderWhenModelIsNull(output);
			}
		}

		/// <summary>
		/// Gets the current datasource of the control. This is either:
		/// 1. A custom datasource parameter set in the Sitecore UI
		/// 2. The current context entity
		/// 
		/// Returns null if the entity is null or of an incompatible type.
		/// </summary>
		/// <returns>an instance of the generic type.</returns>
		protected virtual TModel Model
		{
			get 
			{
				if (_model == null)
				{
					_model = PresenterFactory.GetPresenter<TModel>().GetModel(this);
				}

				return _model;
			}

			// used to inject arbitrary models for testing the view
			set
			{
				_model = value;
			}
		}

		/// <summary>
		/// Gets the value of the data source set in sitecore
		/// </summary>
		public override string DataSource
		{
			get
			{
				return base.DataSource;
			}
			set
			{
				base.DataSource = value;
				_model = null; // decache the model since the datasource got updated
			}
		}

		/// <summary>
		/// Implements a rendering of the control's ViewModel
		/// </summary>
		/// <param name="writer">The HtmlTextWriter for the Page/Response</param>
		protected abstract void RenderModel(HtmlTextWriter writer);

		/// <summary>
		/// Method called when it is time to render the control but the Model is null. By default causes the control to render nothing unless in Page Editor or Preview mode.
		/// </summary>
		protected virtual void RenderWhenModelIsNull(HtmlTextWriter writer)
		{
			if (IsEditing || IsPreviewing)
			{
				NullModelHelper.RenderNullModelMessage(writer, GetType().Name, DataSource, PresenterFactory.GetPresenter<TModel>().GetType(), typeof(TModel));
			}
		}

		/// <summary>
		/// Returns a unique cache ID string Sitecore uses to cache the control when caching is enabled. If this is not implemented, caching will not work even if it is enabled.
		/// </summary>
		protected override string GetCachingID()
		{
			return GetType().FullName;
		}

		private NameValueCollection _renderingParameters;
		/// <summary>
		/// Gets the parameters assigned to the rendering in Sitecore (i.e. rendering parameters template)
		/// </summary>
		protected NameValueCollection RenderingParameters
		{
			get
			{
				if (_renderingParameters == null)
				{
					_renderingParameters = Parameters.Length > 0 ? HttpUtility.ParseQueryString(Parameters) : new NameValueCollection();
				}

				return _renderingParameters;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hides implementation details from callers who need not be confronted with them")]
		NameValueCollection IView.ViewProperties { get { return RenderingParameters; } }
	}
}
