using Blade.Views;

namespace Blade.Razor
{
	public interface IBladeTemplateMetadata
	{
		string TemplateFilePath { get; set; }
		IRazorViewShim RenderingShim { get; set; }
	}
}
