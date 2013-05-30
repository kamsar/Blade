namespace Blade
{
    public interface IPresenterFactory
    {
        IPresenter<TModel> GetPresenter<TModel>()
            where TModel : class;
    }
}
