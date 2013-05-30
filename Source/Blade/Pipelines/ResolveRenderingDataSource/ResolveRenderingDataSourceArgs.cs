using System.Collections.Generic;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;

namespace Blade.Pipelines.ResolveRenderingDataSource
{
	public class ResolveRenderingDataSourceArgs : PipelineArgs
	{
		public ResolveRenderingDataSourceArgs(string dataSource, Item contextItem) : this(dataSource, contextItem, null) { }

		public ResolveRenderingDataSourceArgs(string dataSource, Item contextItem, IProviderSearchContext searchContext)
		{
			Assert.ArgumentNotNull(contextItem, "contextItem");

			DataSource = dataSource;
			ContextItem = contextItem;
			SearchContext = searchContext;
			DataSourceItems = new List<Item>();
		}

		/// <summary>
		/// The item(s) that are the data source for the rendering
		/// </summary>
		public List<Item> DataSourceItems { get; private set; }

		private bool _hasItems;
		/// <summary>
		/// Determines if the pipeline has resolved a data source item.
		/// NOTE: may be set to true if a handler has handled the data source, but the source did not match any valid item(s)
		/// </summary>
		public bool HasItems
		{
			get
			{
				if (DataSourceItems.Count > 0) return true;
				return _hasItems;
			}
			set { _hasItems = value; }
		}

		/// <summary>
		/// The current Sitecore Context item
		/// </summary>
		public Item ContextItem { get; private set; }

		/// <summary>
		/// The raw data source string set on the rendering
		/// </summary>
		public string DataSource { get; private set; }

		/// <summary>
		/// Optional existing index search context to use if search is needed - one will be created if this is null
		/// </summary>
		public IProviderSearchContext SearchContext { get; private set; }
	}
}
