using RazorEngine.Templating;

namespace Blade.Views
{
	public interface IRazorViewShim : IView
	{
		ITemplate Template { get; set; }
		string ViewPath { get; set; }
	}
}
