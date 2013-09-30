using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Blade
{
	/// <summary>
	/// Handles the resolution of presenters for a given model type
	/// </summary>
	public abstract class BasePresenterFactory : IPresenterFactory
	{
		volatile ICollection<Type> _typeLookup;
		readonly ConcurrentDictionary<Type, Type> _typeLookupResultCache;
		readonly object _syncLock = new object();

		protected ICollection<Type> TypeLookup
		{
			get
			{
				if (_typeLookup == null)
				{
					lock (_syncLock)
					{
						if (_typeLookup == null)
						{
							_typeLookup = LoadTypes();
						}
					}
				}

				return _typeLookup;
			}
		}

		protected ConcurrentDictionary<Type, Type> TypeLookupResultCache { get { return _typeLookupResultCache; } }

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification="Intentional extension point")]
		protected BasePresenterFactory()
		{
			_typeLookupResultCache = new ConcurrentDictionary<Type, Type>();
		}

		protected abstract ICollection<Type> LoadTypes();

		/// <summary>
		/// Finds a rendering handler for a given data type
		/// </summary>
		/// <returns>The render handler for the type, or null if none exists</returns>
		public IPresenter<TModel> GetPresenter<TModel>()
			where TModel : class
		{
			return ResolvePresenter<TModel>();
		}

		protected virtual IPresenter<TModel> ResolvePresenter<TModel>()
			where TModel : class
		{
			if(typeof(TModel) == typeof(object))
				throw new ArgumentException("Model type cannot be object (or dynamic) because that would match any presenter (any type can be assigned to object).");

			Type presenterInterfaceType = typeof(IPresenter<TModel>);
			Type presenterType;

			// check if we've resolved a presenter for this type before and use cache if so
			if (!TypeLookupResultCache.TryGetValue(presenterInterfaceType, out presenterType))
			{
				// iterate over all loaded types and find valid presenters for this type
				foreach (var presenter in TypeLookup)
				{
					if (presenterInterfaceType.IsAssignableFrom(presenter))
					{
						presenterType = presenter;
						break;
					}
				}
				
				// no presenter found, try a default presenter if implemented
				if(presenterType == null)
					presenterType = GetDefaultPresenter<TModel>();

				// if we found a presenter, add it to the cache for next time we resolve it
				if (presenterType != null)
					TypeLookupResultCache.AddOrUpdate(presenterInterfaceType, presenterType, (x, y) => presenterType);
			}

			if(presenterType == null)
				throw new PresenterResolutionException(GetType().FullName + " could not resolve any presenter for model of type " + typeof(TModel));

			// instantiate the presenter object and return it
			return ActivatePresenter<TModel>(presenterType);
		}

		protected virtual IPresenter<TModel> ActivatePresenter<TModel>(Type presenterType)
		{
			return Activator.CreateInstance(presenterType) as IPresenter<TModel>;
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification="Type inferences are not desirable here")]
		protected virtual Type GetDefaultPresenter<TModel>()
			where TModel : class
		{
			throw new PresenterResolutionException(GetType().FullName + " could not resolve any presenter for model of type " + typeof(TModel) + " and it does not support a default presenter.");
		}
	}
}
