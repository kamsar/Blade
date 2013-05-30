using Blade.Utility;
using Sitecore.Data.Items;

namespace Blade
{
    /// <summary>
    /// A generic presenter that resolves the Sitecore rendering data source for you. 
    /// This is a good starting point to derive from if you need to modify presentation behavior.
    /// </summary>
    /// <typeparam name="TModel">Type of the ViewModel that will be returned to the view</typeparam>
    public abstract class SitecorePresenter<TModel> : IPresenter<TModel>
    {
	    public TModel GetModel(IView view)
        {
            Item dataSource = DataSourceHelper.ResolveDataSource(view.DataSource, Sitecore.Context.Item);

            return GetModel(view, dataSource);
        }

        protected abstract TModel GetModel(IView view, Item dataSource);
    }
}
