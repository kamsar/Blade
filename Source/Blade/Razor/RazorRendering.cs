using System;
using RazorEngine.Templating;
using System.Collections.Specialized;
using Blade.Utility;
using System.IO;
using Blade.Razor.Helpers;
using Blade.Views;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Razor
{
	/// <summary>
	/// This is the standard base class from which Razor templates inherit. Any properties defined here will be available to Razor.
	/// </summary>
	/// <typeparam name="TModel">The type of model this template renders (may be "dynamic" if necessary)</typeparam>
	public class RazorRendering<TModel> : HtmlTemplateBase<TModel>, IBladeTemplateMetadata
	{
		/// <summary>
		/// Any parameters set on the rendering in Sitecore. Not applicable to partial views.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="Desirable, as it passes through the parent's implementation")]
		public NameValueCollection RenderingParameters { get; set; }

		/// <summary>
		/// Checks if the page is in inline edit mode (by an author)
		/// </summary>
		public bool IsEditing { get { return Sitecore.Context.PageMode.IsPageEditor; } }

		/// <summary>
		/// Checks if the page is being viewed in preview mode
		/// </summary>
		protected bool IsPreviewing { get { return Sitecore.Context.PageMode.IsPreview; } } 

		protected override ITemplate ResolveLayout(string name)
		{
			ITemplate layout = RazorViewCache.GetCompiledTemplate(name, ((IBladeTemplateMetadata)this).TemplateFilePath);
			if (!RazorUtility.TrySetModel(layout, Model))
				throw new ArgumentException("Model of type " + typeof(TModel) + " could not be set on template " + name);

			return layout;
		}

		public TemplateWriter Include(string name)
		{
			// passes the context model to includes if one is not specified (matches MVC behavior)
			return Include(name, Model);
		}

		public override TemplateWriter Include(string name, object model)
		{
			ITemplate instance = RazorViewCache.GetCompiledTemplate(name, ((IBladeTemplateMetadata)this).TemplateFilePath);
			if (!RazorUtility.TrySetModel(instance, model))
			{
				if(model != null)
					throw new ArgumentException("Model of type " + model.GetType() + " could not be set on template " + name);

				throw new ArgumentException("Null model could not be set on template " + name);
			}

			return ExecuteInclude(name, instance);
		}

		private BladeHtmlHelper<TModel> _htmlHelper;
		public BladeHtmlHelper<TModel> Html
		{
			get
			{
				if (_htmlHelper == null) _htmlHelper = new BladeHtmlHelper<TModel>(this);
				return _htmlHelper;
			}
		}

		private TemplateWriter ExecuteInclude(string name, ITemplate instance)
		{
			if (instance == null)
				throw new ArgumentException("No template could be resolved with name '" + name + "'");

			var templateMetadata = (IBladeTemplateMetadata) instance;
			templateMetadata.RenderingShim = ((IBladeTemplateMetadata)this).RenderingShim;
			return new TemplateWriter(delegate(TextWriter tw)
			{
				tw.Write(instance.Run(new ExecuteContext()));
			});
		}

		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification="Hides implementation details callers need not be bothered with")]
		string IBladeTemplateMetadata.TemplateFilePath { get; set; }
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Hides implementation details callers need not be bothered with")]
		IRazorViewShim IBladeTemplateMetadata.RenderingShim { get; set; }
	}
}
