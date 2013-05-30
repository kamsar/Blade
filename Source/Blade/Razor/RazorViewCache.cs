using System;
using System.Collections.Generic;
using System.Linq;
using RazorEngine.Templating;
using System.IO;
using System.Web;
using Sitecore.Diagnostics;
using Blade.Razor.Templating;

namespace Blade.Razor
{
	/// <summary>
	/// Handles caching of compiled Razor templates and their declared model types
	/// </summary>
	internal static class RazorViewCache
	{
		static readonly Dictionary<string, ViewCacheEntry> ViewCache = new Dictionary<string, ViewCacheEntry>();
		static readonly StringLock ViewCompileLocks = new StringLock();
		static readonly BladeTemplateService TemplateService = new BladeTemplateService();

		public static ITemplate GetCompiledTemplate(string filePath, string baseFilePath)
		{
			string resolvedFilePath = MapTemplateRelativePath(filePath, baseFilePath);

			return GetCompiledTemplate(resolvedFilePath);
		}

		public static ITemplate GetCompiledTemplate(string filePath)
		{
			string translatedPath = TranslateTemplatePath(filePath);

			return GetTemplateInfo(translatedPath).GetTemplateInstance();
		}

		private static string TranslateTemplatePath(string templatePath)
		{
			if (!templatePath.EndsWith(".cshtml"))
				return templatePath + ".cshtml";

			return templatePath;
		}

		private static string MapTemplateRelativePath(string filePath, string baseFilePath)
		{
			// check for an absolute virtual path if so just use it
			if (filePath.StartsWith("/") || filePath.StartsWith("~/")) return filePath;

			string virtualBaseFilePath = @"~\" + baseFilePath.Replace(HttpContext.Current.Request.PhysicalApplicationPath, String.Empty);
			string virtualBaseDirectory = Path.GetDirectoryName(virtualBaseFilePath).Replace("\\", "/");

			return string.Concat(virtualBaseDirectory, "/", filePath);
		}

		/// <summary>
		/// Gets the type of renderer that should be used for a given template. Uses cache if available.
		/// </summary>
		public static Type GetViewModelType(string filePath)
		{
			return GetTemplateInfo(filePath).ViewModelType;
		}

		private static ViewCacheEntry GetTemplateInfo(string filePath)
		{
			var absoluteFilePath = HttpContext.Current.Server.MapPath(filePath);

			var cached = GetCachedTemplate(absoluteFilePath);

			if (cached == null)
			{
				using (ViewCompileLocks.AcquireLock(absoluteFilePath))
				{
					if ((cached = GetCachedTemplate(absoluteFilePath)) == null)
					{
						return GetFreshTemplate(absoluteFilePath);
					}
				}
			}

			return cached;
		}

		private static Type CompileTemplate(string absolutePath, string template)
		{
			try
			{
				Log.Info("Compiling razor: " + absolutePath, typeof(RazorViewCache));
				return TemplateService.CompileFileTemplateType(template, absolutePath);
			}
			catch (TemplateCompilationException tce)
			{
				throw new RazorCompileException(absolutePath, tce);
			}
		}

		private static string GetTemplate(string absolutePath)
		{
			return File.ReadAllText(absolutePath);
		}

		private static DateTime GetLastModified(string absolutePath)
		{
			return File.GetLastWriteTimeUtc(absolutePath);
		}

		private static ViewCacheEntry GetCachedTemplate(string absoluteFilePath)
		{
			if (ViewCache.ContainsKey(absoluteFilePath))
			{
				var cachedValue = ViewCache[absoluteFilePath];
				if (cachedValue.IsValid)
					return cachedValue;

				using (ViewCompileLocks.AcquireLock(absoluteFilePath))
				{
					if (ViewCache.ContainsKey(absoluteFilePath))
					{
						ViewCache.Remove(absoluteFilePath); // it was stale. Kill it.
					}
				}
			}

			return null;
		}

		private static ViewCacheEntry GetFreshTemplate(string absoluteFilePath)
		{
			var entry = new ViewCacheEntry(absoluteFilePath);

			ViewCache[absoluteFilePath] = entry;

			return entry;
		}

		public static Type GetViewModelType(ITemplate template)
		{
			var baseType = typeof(ITemplate<>);
			var typeToCheck = template.GetType();

			while (typeToCheck != typeof(object) && typeToCheck != null)
			{
				Type genericInterfaceType = typeToCheck.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == baseType);

				if (genericInterfaceType != null)
					return genericInterfaceType.GetGenericArguments()[0];

				typeToCheck = typeToCheck.BaseType;
			}

			return typeof(object);
		}

		private class ViewCacheEntry
		{
			public ViewCacheEntry(string absolutePath)
			{
				TemplatePath = absolutePath;
				Template = GetTemplate(TemplatePath);
				CachedFileLastModified = GetLastModified(TemplatePath);
				CompiledTemplate = CompileTemplate(TemplatePath, Template);

			}

			private string TemplatePath { get; set; }

			private string Template { get; set; }
			private Type CompiledTemplate { get; set; }
			private DateTime CachedFileLastModified { get; set; }
			public ITemplate GetTemplateInstance()
			{
				return TemplateService.GetTemplateInstance(CompiledTemplate, TemplatePath);
			}

			public Type ViewModelType
			{
				get { return GetViewModelType(GetTemplateInstance()); }
			}

			public bool IsValid
			{
				get 
				{
					return GetLastModified(TemplatePath) == CachedFileLastModified;
				}
			}
		}
	}
}
