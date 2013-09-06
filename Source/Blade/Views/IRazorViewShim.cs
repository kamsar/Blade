namespace Blade.Views
{
	public interface IRazorViewShim : IView
	{
		string ViewPath { get; set; }
	}
}
