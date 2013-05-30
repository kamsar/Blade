using Sitecore.Data.Items;

namespace Blade.Pipelines.ResolveRenderingDataSource
{
	/// <summary>
	/// Handles rendering data sources based on a Sitecore Query, e.g.:
	/// query:/sitecore//*
	/// fast:/sitecore//*
	/// query:fast:/sitecore//*
	/// </summary>
	public class HandleSitecoreQuery : ResolveRenderingDataSourcePipelineProcessor
	{
		public override void DoProcess(ResolveRenderingDataSourceArgs args)
		{
			// we only want query type data sources, e.g. queries, fast queries, or relative sources
			if (!(args.DataSource.StartsWith("query:") || args.DataSource.StartsWith("fast:") || args.DataSource.StartsWith(".")))
			{
				return;
			}

			string query = args.DataSource;
			bool useFastQuery = false;

			// strip the starting query: if one exists, we wont send that on to Sitecore
			if (query.StartsWith("query:"))
				query = query.Substring(6);

			// strip fast: if present, but keep a note if it was a fast query for later
			if (query.StartsWith("fast:"))
			{
				query = query.Substring(5);
				useFastQuery = true;
			}

			Item rootItem = args.ContextItem;

			// move up any parent items until we get to the meat of the query
			while (query.StartsWith("../"))
			{
				rootItem = rootItem.Parent;
				query = query.Substring(3);
			}

			// strip any explicit relative paths; we'll "relative-ize" the query with the root item's full path anyway
			if (query.StartsWith("./"))
				query = query.Substring(2);

			// if the path is not an absolute one, we'll prepend the full path of the root item to turn it into an effectively relative query
			if (!query.StartsWith("/"))
				query = rootItem.Paths.FullPath + "/" + query;

			// if the original query contained the fast moniker we'll re-add it here to the parsed query
			if (useFastQuery) query = "fast:" + query;

			args.DataSourceItems.AddRange(rootItem.Database.SelectItems(query));

			args.HasItems = true;
		}
	}
}