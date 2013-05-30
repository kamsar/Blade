using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blade.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Configuration
{
	/// <summary>
	/// Provides a list of all types present in specified assembly names. This provider is appropriate when Synthesis classes and presenters are in a known set of places.
	/// It is also much faster than AppDomainTypeListProvider.
	/// </summary>
	public class ConfigurationPresenterFactory  : BasePresenterFactory
	{
		private readonly List<Assembly> _assemblies = new List<Assembly>();
        private bool _typesAreLoaded;

		public void AddAssembly(string name)
		{
            if (_typesAreLoaded) throw new InvalidOperationException("Types have already been loaded, and adding a new assembly will not have any effect.");

			Assembly a = Assembly.Load(name);
			if (a == null) throw new ArgumentException("The assembly name was not valid");

			_assemblies.Add(a);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="In this case, we really don't care about any assembly loading errors")]
        protected override ICollection<Type> LoadTypes()
        {
            _typesAreLoaded = true;

            IEnumerable<Assembly> assemblies;

            if(_assemblies.Count > 0)
                assemblies = _assemblies;
            else assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            return assemblies.SelectMany(delegate(Assembly x)
                            {
								try { return x.GetTypes(); }
								catch (ReflectionTypeLoadException rex) { return rex.Types.Where(y => y != null).ToArray(); } // http://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx
								catch { return new Type[] { }; }
                            })
                            .Where(x => !x.IsGenericType && x.ImplementsOpenGenericInterface(typeof(IPresenter<>)))
                            .ToList();
        }
    }
}
