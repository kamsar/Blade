using System;
using System.Linq;
using Blade.Pipelines.ResolveRenderingDataSource;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;

namespace Blade.Utility
{
	/// <summary>
	/// Handles resolution of data source item(s) values for renderings
	/// </summary>
	/// <remarks>
	/// Based on a heavily modified/simplified version of John West's SC7 sublayout parameters prototype helper code:
	/// http://www.sitecore.net/Community/Technical-Blogs/John-West-Sitecore-Blog/Posts/2013/05/Sitecore-7-Data-Sources-Part-6-Access-Multiple-Data-Source-Items-from-MVC-Views.aspx
	/// 
	/// Thanks for all the work you've done for the community John :)
	/// </remarks>
	public static class DataSourceHelper
	{
		/// <summary>
		/// Given a DataSource string (from the layout editor), resolves the item that refers to.
		/// </summary>
		/// <param name="dataSource">The string data source passed to the presentation component.</param>
		/// <param name="contextItem">The context item for relative paths and specifying the database from which to retrieve other items.</param>
		public static Item ResolveDataSource(string dataSource, Item contextItem)
		{
			return ResolveDataSource(dataSource, contextItem, null, true);
		}

		/// <summary>
		/// Given a DataSource string (from the layout editor), resolves the item that refers to.
		/// </summary>
		/// <param name="dataSource">The string data source passed to the presentation component.</param>
		/// <param name="contextItem">The context item for relative paths and specifying the database from which to retrieve other items.</param>
		/// <param name="searchContext">The search context.</param>
		public static Item ResolveDataSource(string dataSource, Item contextItem, IProviderSearchContext searchContext)
		{
			return ResolveDataSource(dataSource, contextItem, searchContext, true);
		}

		/// <summary>
		/// Given a DataSource string (from the layout editor), resolves the item that refers to.
		/// </summary>
		/// <param name="dataSource">The string data source passed to the presentation component.</param>
		/// <param name="contextItem">The context item for relative paths and specifying the database from which to retrieve other items.</param>
		/// <param name="searchContext">The search context.</param>
		/// <param name="assertSingleItem">If true, an exception will be thrown if more than one item matches the data source spec</param>
		public static Item ResolveDataSource(string dataSource, Item contextItem, IProviderSearchContext searchContext, bool assertSingleItem)
		{
			var items = ResolveMultipleDataSource(dataSource, contextItem, searchContext);

			if (assertSingleItem && items.Count() > 1)
					throw new Exception(string.Format("The data source {0} matches more than one item, and only a single item is expected.", dataSource));

			return items.FirstOrDefault();
		}

		/// <summary>
		/// Given a DataSource string (from the layout editor), resolves the item(s) that refers to.
		/// </summary>
		/// <param name="dataSource">The string data source passed to the presentation component.</param>
		/// <param name="contextItem">The context item for relative paths and specifying the database from which to retrieve other items.</param>
		public static Item[] ResolveMultipleDataSource(string dataSource, Item contextItem)
		{
			return ResolveMultipleDataSource(dataSource, contextItem, null, true);
		}

		/// <summary>
		/// Given a DataSource string (from the layout editor), resolves the item(s) that refers to.
		/// </summary>
		/// <param name="dataSource">The string data source passed to the presentation component.</param>
		/// <param name="contextItem">The context item for relative paths and specifying the database from which to retrieve other items.</param>
		/// <param name="searchContext">The search context.</param>
		public static Item[] ResolveMultipleDataSource(string dataSource, Item contextItem, IProviderSearchContext searchContext)
		{
			return ResolveMultipleDataSource(dataSource, contextItem, searchContext, true);
		}

		/// <summary>
		/// Given a DataSource string (from the layout editor), resolves the item(s) that refers to.
		/// </summary>
		/// <param name="dataSource">The string data source passed to the presentation component.</param>
		/// <param name="contextItem">The context item for relative paths and specifying the database from which to retrieve other items.</param>
		/// <param name="searchContext">The search context.</param>
		/// <param name="assertSingleItem">If true, an exception will be thrown if more than one item matches the data source spec</param>
		public static Item[] ResolveMultipleDataSource(string dataSource, Item contextItem, IProviderSearchContext searchContext, bool assertSingleItem)
		{
			Assert.IsNotNull(contextItem, "contextItem");

			var items = RunPipeline(dataSource, contextItem, searchContext);

			return items;
		}

		/// <summary>
		/// Invoke the ResolveRenderingDataSource pipeline to resolve the data source item
		/// </summary>
		private static Item[] RunPipeline(string dataSource, Item contextItem, IProviderSearchContext searchContext)
		{
			var args = new ResolveRenderingDataSourceArgs(dataSource, contextItem, searchContext);

			CorePipeline.Run("resolveRenderingDataSource", args);

			return args.DataSourceItems.ToArray();
		}
	}
}