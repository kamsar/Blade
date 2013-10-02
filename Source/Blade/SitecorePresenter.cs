using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Blade.Razor;
using Blade.Utility;
using Sitecore.Data.Items;

namespace Blade
{
	/// <summary>
	/// A generic presenter that resolves the Sitecore rendering data source for you. 
	/// This is a good starting point to derive from if you need to modify presentation behavior.
	/// </summary>
	/// <typeparam name="TModel">Type of the ViewModel that will be returned to the view</typeparam>
	public abstract class SitecorePresenter<TModel> : IPresenter<TModel>, IPostBackPresenter<TModel>, IXmlHttpRequestPresenter<TModel>
	{
		private Item _dataSource;

		public virtual TModel GetModel(IView view)
		{
			Item dataSource = _dataSource ?? DataSourceHelper.ResolveDataSource(view.DataSource, Sitecore.Context.Item);

			return GetModel(view, dataSource);
		}

		protected abstract TModel GetModel(IView view, Item dataSource);
		
		/// <summary>
		/// Overrides normal data source resolution and provides a static data source item.
		/// Useful for testing, if you must use Item for testing for some reason.
		/// It'd be better to use SynthesisPresenter for testing since it does not need the Sitecore API however.
		/// </summary>
		/// <param name="item">The item to set the data source of the presenter to</param>
		public void SetDataSource(Item item)
		{
			_dataSource = item;
		}

		/// <summary>
		/// Handles post back to the view. This is called after GetModel(), but may affect the values of the model. No model binding is performed on the model.
		/// </summary>
		/// <param name="view">The view handling the UI</param>
		/// <param name="model">The model the view received (probably from GetModel())</param>
		/// <param name="controllerContext">(optional) a MVC ControllerContext to pass around the ModelState validation data in</param>
		/// <remarks>
		/// If you need to add custom validation messages and such add them to the ((Controller)controllerContext.Controller).ModelState - if the controller context is not null!
		/// </remarks>
		public virtual void HandlePostBack(IView view, TModel model, ControllerContext controllerContext)
		{
			if(IsOverride(GetType().GetMethod("HandlePostBackWithModelBinding", BindingFlags.NonPublic | BindingFlags.Instance)) && TryUpdateModel(model, controllerContext))
				HandlePostBackWithModelBinding(view, model);
		}

		/// <summary>
		/// Handles post back to the view. This is called after GetModel(), but may affect the values of the model. Model binding is automatically applied to the model.
		/// </summary>
		/// <param name="view">The view handling the UI</param>
		/// <param name="model">The model the view received (probably from GetModel())</param>
		protected virtual void HandlePostBackWithModelBinding(IView view, TModel model)
		{
			// do nothing - you can choose to handle postback or not
		}

		/// <summary>
		/// Handles AJAX requests to the view (of any HTTP method). This is called after GetModel(), but may affect the values of the model.
		/// Most times this method will probably clear the output stream and say, return JSON or a HTML snippet. No model binding is performed by this method.
		/// </summary>
		/// <param name="view">The view handling the UI</param>
		/// <param name="model">The model the view received (probably from GetModel())</param>
		/// <param name="controllerContext">(optional) a MVC ControllerContext to pass around the ModelState validation data in</param>
		public virtual void HandleXmlHttpRequest(IView view, TModel model, ControllerContext controllerContext)
		{
			if (IsOverride(GetType().GetMethod("HandleXmlHttpRequestWithModelBinding", BindingFlags.NonPublic | BindingFlags.Instance)) && TryUpdateModel(model, controllerContext))
				HandleXmlHttpRequestWithModelBinding(view, model);
		}

		/// <summary>
		/// Handles AJAX requests to the view (of any HTTP method). This is called after GetModel(), but may affect the values of the model.
		/// Most times this method will probably clear the output stream and say, return JSON or a HTML snippet. Model binding is automatically applied to the model.
		/// </summary>
		/// <param name="view">The view handling the UI</param>
		/// <param name="model">The model the view received (probably from GetModel())</param>
		protected virtual void HandleXmlHttpRequestWithModelBinding(IView view, TModel model)
		{
			// do nothing - you choose to handle this explicitly
		}

		/// <summary>
		/// Gets the MVC value provider that is used to resolve source values for model binding
		/// </summary>
		/// <param name="context">The current controller context</param>
		/// <returns>The value provider to use</returns>
		protected virtual IValueProvider GetValueProvider(ControllerContext context)
		{
			return ValueProviderFactories.Factories.GetValueProvider(context);
		}

		ModelBinderDictionary _binders;
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Property is settable so that the dictionary can be provided for unit testing purposes.")]
		protected internal ModelBinderDictionary Binders
		{
			get
			{
				if (_binders == null)
				{
					_binders = ModelBinders.Binders;
				}
				return _binders;
			}
			set { _binders = value; }
		}

		/// <summary>
		/// Attempts MVC model binding on the request variables. Respects custom binders.
		/// </summary>
		/// <param name="model">Model to bind to</param>
		/// <param name="controllerContext">The current controller context, if any (optional) - used to persist ModelState validation</param>
		/// <returns>True if the model passes all validations</returns>
		protected bool TryUpdateModel(TModel model, ControllerContext controllerContext)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}
			
			if(controllerContext == null)
				controllerContext = ViewRenderer.CreateController<EmptyController>().ControllerContext;

			Predicate<string> propertyFilter = propertyName => true;
			var bindAttribute = typeof (TModel).GetCustomAttributes(typeof (BindAttribute), true).FirstOrDefault() as BindAttribute;
			if (bindAttribute != null)
				propertyFilter = bindAttribute.IsPropertyAllowed;

			IModelBinder binder = Binders.GetBinder(typeof(TModel));

			var bindingContext = new ModelBindingContext
				{
				ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(TModel)),
				ModelName = null,
				ModelState = ((Controller)controllerContext.Controller).ModelState,
				PropertyFilter = propertyFilter,
				ValueProvider = GetValueProvider(controllerContext)
			};

			binder.BindModel(controllerContext, bindingContext);

			return bindingContext.ModelState.IsValid;
		}

		/// <summary>
		/// Prevents us from model binding automatically when you have not specifically overridden the model bound postback handler(s)
		/// </summary>
		private static bool IsOverride(MethodInfo m)
		{
			return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
		}
	}
}
