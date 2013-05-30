using Blade;
using Sitecore.Data.Items;

namespace $rootnamespace$.Layouts
{
	public class SamplePresenter : SitecorePresenter<SampleModel>
	{
		protected override SampleModel GetModel(IView view, Item dataSource)
		{
			return new SampleModel {SampleProperty = dataSource.Name};
		}
	}
}