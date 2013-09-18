
namespace Blade
{
	/// <summary>
	/// Represents a presenter that abstracts the logic of how to get a model type away from the rendering
	/// </summary>
	public interface IPresenter<out T>
	{
		T GetModel(IView view);
	}
}
