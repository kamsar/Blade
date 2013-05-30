using Sitecore.Diagnostics;

namespace Blade.Pipelines.ResolveRenderingDataSource
{
	/// <summary>
	/// Handles the case where no data source is present, and the data source should be the context item
	/// </summary>
	public class HandleContextDataSource : ResolveRenderingDataSourcePipelineProcessor
	{
		public override void DoProcess(ResolveRenderingDataSourceArgs args)
		{
			Assert.ArgumentNotNull(args, "args");

			// had a valid data source, so skip processing an empty context data source
			if (!string.IsNullOrEmpty(args.DataSource)) return;

			args.DataSourceItems.Add(Sitecore.Context.Item);

			args.HasItems = true;
		}
	}
}