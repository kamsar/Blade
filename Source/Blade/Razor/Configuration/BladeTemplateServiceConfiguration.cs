using System.Collections.Generic;
using System.Linq;
using RazorEngine.Configuration;
using System.Web.WebPages.Razor.Configuration;
using System.Web.Configuration;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Razor.Configuration
{
	public class BladeTemplateServiceConfiguration : TemplateServiceConfiguration
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification="Intentional extension point")]
		public BladeTemplateServiceConfiguration()
		{
			LoadNamespaces();

			BaseTemplateType = typeof(RazorRendering<>);
		}

		protected virtual void LoadNamespaces()
		{
			var section = ConfigurationManager.GetSection(RazorPagesSection.SectionName) as RazorPagesSection;

			if (section != null) // we found a razor config section, add any namespaces defined there
				Namespaces = new HashSet<string>(section.Namespaces.Cast<NamespaceInfo>().Select(x => x.Namespace));
			else
				Namespaces = new HashSet<string>();

			// make sure required/expected namespaces are present
			var requiredNamespaces = new[] { "Blade.Razor.Helpers", "Blade.Razor", "System.Collections.Generic", "System.Linq", "System" };
			foreach (var ns in requiredNamespaces)
			{
				if (!Namespaces.Contains(ns))
					Namespaces.Add(ns);
			}
		}
	}
}
