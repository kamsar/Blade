using System.Linq;
using Sitecore.Buckets.Util;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data.Items;

namespace Blade.Pipelines.ResolveRenderingDataSource
{
	/// <summary>
	/// Handles data sources that are defined using index search syntax
	/// </summary>
	public class HandleSearch : ResolveRenderingDataSourcePipelineProcessor
	{
		public override void DoProcess(ResolveRenderingDataSourceArgs args)
		{
			// if a search context came in, we don't dispose it when done - otherwise we dispose our temp context
			bool disposeSearchContext = args.SearchContext == null;

			IProviderSearchContext searchContext = null;
			try
			{
				searchContext = args.SearchContext ?? ContentSearchManager.CreateSearchContext(new SitecoreIndexableItem(args.ContextItem));

				var query = CreateQuery(searchContext, args.DataSource);

				args.DataSourceItems.AddRange(ProcessQueryResults(query));
			}
			finally
			{
				if(disposeSearchContext && searchContext != null) searchContext.Dispose();
			}
		}

		protected virtual IQueryable<SitecoreUISearchResultItem> CreateQuery(IProviderSearchContext context, string query)
		{
			var parsedQuery = UIFilterHelpers.ParseDatasourceString(query);
			var linqQuery = LinqHelper.CreateQuery(context, parsedQuery);

			return FilterQuery(linqQuery);
		}

		protected virtual IQueryable<SitecoreUISearchResultItem> FilterQuery(IQueryable<SitecoreUISearchResultItem> query)
		{
			string language = Sitecore.Context.Language.CultureInfo.TwoLetterISOLanguageName;

			// this makes us get "normal" query behavior of returning context language and latest version only
			return query.Where(x => x.Language == language && x["_latestversion"] == "1");
		}

		protected virtual Item[] ProcessQueryResults(IQueryable<SitecoreUISearchResultItem> query)
		{
			return query.Select(x => x.GetItem()).ToArray();
		}
	}
}