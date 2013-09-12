using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Web.UI.WebControls;
using Blade.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Views
{
	public abstract class UserControlView<TModel> : UserControl, IView
		where TModel : class
	{
		protected readonly IPresenterFactory PresenterFactory;

		protected UserControlView()
			: this(PresenterResolver.Current)
		{

		}

		protected UserControlView(IPresenterFactory presenterFactory)
		{
			PresenterFactory = presenterFactory;
		}

		IPresenter<TModel> _presenter;
		/// <summary>
		/// Gets the presenter used for the control. Returns null if no presenter
		/// </summary>
		private IPresenter<TModel> Presenter
		{
			get
			{
				if (_presenter == null)
				{
					_presenter = PresenterFactory.GetPresenter<TModel>();
				}

				return _presenter;
			}
		}

		TModel _model;
		string _stringDataSource;

		/// <summary>
		/// Gets the current model of the control. This is returned by its presenter
		/// 
		/// Returns null if the entity is null or of an incompatible type.
		/// </summary>
		/// <returns>an instance of the generic type.</returns>
		protected virtual TModel Model
		{
			get
			{
				if (_model == null && Presenter != null)
				{
					_model = Presenter.GetModel(this);
				}

				return _model;
			}

			// used to inject arbitrary models for testing the view via inherited stub classes
			set
			{
				_model = value;
			}
		}

		/// <summary>
		/// This is the value of the "Data Source" parameter from the sitecore layout editor.
		/// </summary>
		public virtual string DataSource
		{
			get
			{
				if (_stringDataSource == null)
				{
					_stringDataSource = SublayoutContainer != null ? SublayoutContainer.DataSource : string.Empty;
				}

				return _stringDataSource;
			}

			set
			{
				_stringDataSource = value;
				_model = null; // decache the model since the datasource got updated
				_presenter = null;
			}
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
		/// Gets the Sublayout web control that Sitecore wraps user controls in. Useful for manipulating caching settings.
		/// </summary>
		protected Sublayout SublayoutContainer
		{
			get
			{
				if (Parent is Sublayout)
				{
					return Parent as Sublayout;
				}

				return null;
			}
		}

		private NameValueCollection _sublayoutParameters;
		/// <summary>
		/// Gets the parameters assigned to the sublayout in Sitecore (i.e. rendering parameters template)
		/// </summary>
		protected NameValueCollection RenderingParameters
		{
			get
			{
				if (_sublayoutParameters == null)
				{
					var parameters = Attributes["sc_parameters"];

					if (!string.IsNullOrEmpty(parameters))
						_sublayoutParameters = HttpUtility.ParseQueryString(parameters);
					else
						_sublayoutParameters = new NameValueCollection();
				}

				return _sublayoutParameters;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hides implementation details from callers who need not be confronted with them")]
		NameValueCollection IView.ViewProperties { get { return RenderingParameters; } }

		#region Caching Parameters
		protected bool Cacheable
		{
			get { return (SublayoutContainer ?? new Sublayout()).Cacheable; }
			set { if (SublayoutContainer != null) SublayoutContainer.Cacheable = value; }
		}

		protected bool VaryByData
		{
			get { return (SublayoutContainer ?? new Sublayout()).VaryByData; }
			set { if (SublayoutContainer != null) SublayoutContainer.VaryByData = value; }
		}

		protected bool VaryByDevice
		{
			get { return (SublayoutContainer ?? new Sublayout()).VaryByDevice; }
			set { if (SublayoutContainer != null) SublayoutContainer.VaryByDevice = value; }
		}

		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Sitecore convention")]
		protected bool VaryByLogin
		{
			get { return (SublayoutContainer ?? new Sublayout()).VaryByLogin; }
			set { if (SublayoutContainer != null) SublayoutContainer.VaryByLogin = value; }
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Parm", Justification = "Sitecore convention")]
		protected bool VaryByParm
		{
			get { return (SublayoutContainer ?? new Sublayout()).VaryByParm; }
			set { if (SublayoutContainer != null) SublayoutContainer.VaryByParm = value; }
		}

		protected bool VaryByQueryString
		{
			get { return (SublayoutContainer ?? new Sublayout()).VaryByQueryString; }
			set { if (SublayoutContainer != null) SublayoutContainer.VaryByQueryString = value; }
		}

		protected bool VaryByUser
		{
			get { return (SublayoutContainer ?? new Sublayout()).VaryByUser; }
			set { if (SublayoutContainer != null) SublayoutContainer.VaryByUser = value; }
		}

		protected bool ClearOnIndexUpdate
		{
			get { return (SublayoutContainer ?? new Sublayout()).ClearOnIndexUpdate; }
			set { if (SublayoutContainer != null) SublayoutContainer.ClearOnIndexUpdate = value; }
		}
		#endregion

		protected virtual void OnInitWhenModelIsNull(EventArgs e)
		{
		}

		protected override void OnInit(EventArgs e)
		{
			if (Model != null)
				base.OnInit(e);
			else
				OnInitWhenModelIsNull(e);
		}

		protected virtual void OnLoadWhenModelIsNull(EventArgs e)
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			if (Model != null)
				base.OnLoad(e);
			else
				OnLoadWhenModelIsNull(e);

			// check if we have a postback OR XHR and process that with the presenter if it supports it
			if (new HttpRequestWrapper(Request).IsAjaxRequest())
			{
				var xhrPresenter = Presenter as IXmlHttpRequestPresenter<TModel>;
				if(xhrPresenter != null)
					xhrPresenter.HandleXmlHttpRequest(this, Model, null);
			}
			else if (Request.HttpMethod == "POST")
			{
				var postPresenter = Presenter as IPostBackPresenter<TModel>;
				if(postPresenter != null)
					postPresenter.HandlePostBack(this, Model, null);
			}
		}

		protected virtual void OnPreRenderWhenModelIsNull(EventArgs e)
		{
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (Model != null)
				base.OnPreRender(e);
			else
				OnPreRenderWhenModelIsNull(e);
		}

		/// <summary>
		/// Method called when it is time to render the control but the Model is null. By default causes the control to render nothing unless in Page Editor or Preview mode.
		/// </summary>
		protected virtual void RenderWhenModelIsNull(HtmlTextWriter writer)
		{
			if (IsEditing || IsPreviewing)
			{
				NullModelHelper.RenderNullModelMessage(writer, AppRelativeVirtualPath, DataSource, PresenterFactory.GetPresenter<TModel>().GetType(), typeof(TModel));
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			using (new RenderingDiagnostics(writer, AppRelativeVirtualPath, Cacheable, VaryByData, VaryByDevice, VaryByLogin, VaryByParm, VaryByQueryString, VaryByUser, ClearOnIndexUpdate, string.Empty))
			{
				if (Model != null)
					base.Render(writer);
				else
					RenderWhenModelIsNull(writer);
			}
		}
	}
}
