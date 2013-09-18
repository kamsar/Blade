namespace Blade.Views
{
	/// <summary>
	/// This interface allows for passing the view path from the SitecoreRazorRenderingType to the actual RazorViewShim&lt;T&gt; that handles the rendering
	/// without the fuss of generics
	/// </summary>
	internal interface IRazorViewShim : IView
	{
		string ViewPath { get; set; }
	}
}
