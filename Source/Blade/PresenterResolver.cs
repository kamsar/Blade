using Sitecore.Configuration;

namespace Blade
{
	/// <summary>
	/// This class controls how Blade views resolve their Presenter. You can plug different implementations of IPresenterFactory in
	/// during application start to modify how presenters resolve, change the default presenter type, or implement dependency injection on presenters
	/// 
	/// This works similarly to DependencyResolver.SetResolver() in ASP.NET MVC
	/// </summary>
	public static class PresenterResolver
	{
		private static readonly object SyncRoot = new object();
		private static volatile IPresenterFactory _currentFactory;

		/// <summary>
		/// Gets the current Instance Manager instance
		/// </summary>
		public static IPresenterFactory Current
		{
			get
			{
				if (_currentFactory == null)
				{
					lock (SyncRoot)
					{
						if (_currentFactory == null)
							_currentFactory = LoadPresenterFactoryFromConfig();
					}
				}

				return _currentFactory;
			}
			set
			{
				lock (SyncRoot)
				{
					_currentFactory = value;
				}
			}
		}

		private static IPresenterFactory LoadPresenterFactoryFromConfig()
		{
			return (IPresenterFactory)Factory.CreateObject("/sitecore/blade/presenterFactory", true);
		}
	}
}
