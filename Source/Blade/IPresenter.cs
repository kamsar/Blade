
namespace Blade
{
	/// <summary>
	/// Represents a presenter that abstracts logic away from the rendering
	/// </summary>
	public interface IPresenter<out T>
	{
		T GetModel(IView view);
	}
}
