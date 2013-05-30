using RazorEngine.Templating;
using RazorEngine.Text;

namespace Blade.Razor.Helpers
{
	public class BladeHtmlHelper<T>
	{
		public BladeHtmlHelper(RazorRendering<T> view)
		{
			View = view;
		}

		public RazorRendering<T> View { get; private set; }

		public TemplateWriter Partial(string viewPath)
		{
			return View.Include(viewPath);
		}

		public TemplateWriter Partial<TModel>(string viewPath, TModel model)
		{
			return View.Include(viewPath, model);
		}

		public IEncodedString Raw(string value)
		{
			return View.Raw(value);
		}
	}
}
