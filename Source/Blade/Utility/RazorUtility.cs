using RazorEngine.Templating;
using Blade.Razor;

namespace Blade.Utility
{
	public static class RazorUtility
	{
		/// <summary>
		/// Attempts to set the Model on a Razor ITemplate instance.
		/// </summary>
		/// <returns>True if the model was successfully set, or false if the model type was incompatible with the template.</returns>
		public static bool TrySetModel(ITemplate razorTemplate, object model)
		{
			var modelType = RazorViewCache.GetViewModelType(razorTemplate);

			if (!modelType.IsInstanceOfType(model)) return false;

			var property = razorTemplate.GetType().GetProperty("Model");
			property.SetValue(razorTemplate, model, null);

			return true;
		}
	}
}
