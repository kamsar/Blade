namespace Blade.Pipelines.ResolveRenderingDataSource
{
	public abstract class ResolveRenderingDataSourcePipelineProcessor
	{
		/// <summary>
		/// Method is invoked only if no previous pipeline processor has provided a data source item
		/// </summary>
		public abstract void DoProcess(ResolveRenderingDataSourceArgs args);

		/// <summary>
		/// Method executes always on pipeline execution
		/// </summary>
		public virtual void Process(ResolveRenderingDataSourceArgs args)
		{
			if (args.HasItems) return;

			DoProcess(args);
		}
	}
}
