using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Blade.Pipelines.ResolveRenderingDataSource
{
	/// <summary>
	/// Handles when a static item or list of items is specified for the data source, e.g.
	/// {F455CB4A-3E30-4C01-87BF-22300FD855F2}
	/// {F455CB4A-3E30-4C01-87BF-22300FD855F2}|{A7DA1824-02FE-4BF7-9817-297A7F0C29C4}
	/// /sitecore/content/Home
	/// /sitecore/content/Home:nameOfMultilistField [pulls the list of IDs on that multilist field on the target item as data sources]
	/// /sitecore/content/Home|/sitecore/templates
	/// </summary>
	public class HandleStaticItems : ResolveRenderingDataSourcePipelineProcessor
	{
		public override void DoProcess(ResolveRenderingDataSourceArgs args)
		{
			// ignore data sources that are not absolute paths or IDs
			if (!(args.DataSource.StartsWith("/") || args.DataSource.StartsWith("{"))) return;

			// the split on | supports multilist style ID lists (but also path lists technically...)
			foreach (string identifier in args.DataSource.Split('|'))
			{
				string[] parts = identifier.Split(':');

				// verify bounds (for an item multilist identifier e.g. "/sitecore/content/foo:fieldName")
				// note: relative paths are not supported with multilist identifiers' source item
				Assert.IsTrue(parts.Length > 0, "parts > 0");
				Assert.IsTrue(parts.Length < 3, "parts < 3");

				Item item = args.ContextItem.Database.GetItem(parts[0]);

				// couldn't resolve data source item, skip entry
				if (item == null)
				{
					Log.Warn(string.Format("HandleStaticItems: A rendering on {0} contains an invalid item reference: {1}", args.ContextItem.Paths.FullPath, parts[0]), this);
					continue;
				}

				// parse a item multilist field reference
				if (parts.Length > 1)
				{
					MultilistField field = item.Fields[parts[1]];

					// invalid field reference
					if (field == null || string.IsNullOrEmpty(field.Value))
					{
						Log.Warn(string.Format("HandleStaticItems: The multilist field {0} on {1} referred to in a rendering data source on {2} did not exist.", item.Paths.FullPath, parts[1], args.ContextItem.Paths.FullPath), this);
						continue;
					}

					args.DataSourceItems.AddRange(field.GetItems());

					return;
				}

				// add a single identifier from the data source string
				args.DataSourceItems.Add(item);
			}

			args.HasItems = true;
		}
	}
}