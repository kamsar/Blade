using System;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Blade.Razor.Configuration;

namespace Blade.Razor.Templating
{
	public class BladeTemplateService : TemplateService
	{
		readonly ITemplateServiceConfiguration _config;

		public BladeTemplateService() : this(new BladeTemplateServiceConfiguration())
		{

		}

		public BladeTemplateService(ITemplateServiceConfiguration configuration)
			: base(configuration)
		{
			_config = configuration;
		}

		public ITemplateServiceConfiguration Configuration { get { return _config; } }

		public Type CompileFileTemplateType(string template, string templateAbsolutePath)
		{
			Type templateType;

			try
			{
				templateType = CreateTemplateType(template, null);
			}
			catch (TemplateParsingException tpe)
			{
				throw new RazorParseException(templateAbsolutePath, tpe);
			}

			return templateType;
		}

		public ITemplate GetTemplateInstance(Type templateType, string templateAbsolutePath)
		{
			var context = CreateInstanceContext(templateType);
			ITemplate instance = _config.Activator.CreateInstance(context);
			instance.TemplateService = this;

			var fileTemplate = instance as IBladeTemplateMetadata;

			if (fileTemplate != null)
				fileTemplate.TemplateFilePath = templateAbsolutePath;
			else
				throw new ArgumentException("Template type must be IRazorFileTemplate", "templateType");

			return instance;
		}
	}
}
