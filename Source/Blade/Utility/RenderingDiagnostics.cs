using System;
using System.Text;
using System.Web.UI;
using Sitecore.Diagnostics;
using System.Web;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Blade.Utility
{
	public class RenderingDiagnostics : IDisposable
	{
		readonly HtmlTextWriter _writer;
		readonly string _renderingName;
		readonly bool _cacheable;
		readonly bool _varyByData;
		readonly bool _varyByDevice;
		readonly bool _varyByLogin;
		readonly bool _varyByParm;
		readonly bool _varyByQueryString;
		readonly bool _varyByUser;
		readonly bool _clearOnIndexUpdate;
		readonly string _varyByCustom;
		readonly Stopwatch _timer;

		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "It's what Sitecore uses")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Parm", Justification = "It's what Sitecore uses")]
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Intentional extension point")]
		public RenderingDiagnostics(HtmlTextWriter writer, string renderingName, bool cacheable, bool varyByData, bool varyByDevice, bool varyByLogin, bool varyByParm, bool varyByQueryString, bool varyByUser, bool clearOnIndexUpdate, string varyByCustom)
		{
			Assert.IsNotNull(writer, "HtmlTextWriter cannot be null");
			Assert.IsNotNull(renderingName, "Rendering name cannot be null");

			_writer = writer;
			_renderingName = renderingName;
			_cacheable = cacheable;
			_varyByData = varyByData;
			_varyByDevice = varyByDevice;
			_varyByLogin = varyByLogin;
			_varyByParm = varyByParm;
			_varyByQueryString = varyByQueryString;
			_varyByUser = varyByUser;
			_varyByCustom = varyByCustom;
			_clearOnIndexUpdate = clearOnIndexUpdate;

			_timer = new Stopwatch();

			RenderingStartDiagnostics();
		}

		private static readonly Lazy<bool> DebugEnabled = new Lazy<bool>(() => HttpContext.Current.IsDebuggingEnabled);
		public static bool DiagnosticsEnabledForThisRequest
		{
			get
			{
				var value = HttpContext.Current.Items["RENDERING_DIAGNOSTICS_ENABLED"];
				if (value == null || value.ToString() == bool.TrueString) return true;

				return false;
			}
			set { HttpContext.Current.Items["RENDERING_DIAGNOSTICS_ENABLED"] = value; }
		}

		protected virtual bool DiagnosticsEnabled
		{
			get
			{
				if (!DiagnosticsEnabledForThisRequest) return false;
				return DebugEnabled.Value;
			}
		}

		protected virtual void RenderingStartDiagnostics()
		{
			if (!DiagnosticsEnabled) return;

			//<!-- Begin Rendering "~/bar/Foo.ascx" -->
			//<!-- Rendering was output cached at {datetime}, VaryByData, CachingID = "loremipsum" -->

			var comment = new StringBuilder();

			comment.AppendFormat("<!-- Begin Rendering {0} -->\n", _renderingName);
			if (!_cacheable)
			{
				_writer.Write(comment.ToString());
				return;
			}

			comment.AppendFormat("<!-- Rendering was output cached at {0}", DateTime.Now);
			if (_clearOnIndexUpdate)
				comment.Append(", ClearOnIndexUpdate");
			if (_varyByData)
				comment.Append(", VaryByData");
			if (_varyByDevice)
				comment.Append(", VaryByDevice");
			if (_varyByLogin)
				comment.Append(", VaryByLogin");
			if (_varyByParm)
				comment.Append(", VaryByParm");
			if (_varyByQueryString)
				comment.Append(", VaryByQueryString");
			if (_varyByUser)
				comment.Append(", VaryByUser");
			if (!string.IsNullOrEmpty(_varyByCustom))
				comment.AppendFormat(", VaryByCustom=\"{0}\"", HttpUtility.HtmlEncode(_varyByCustom));

			comment.Append(" -->");

			_writer.Write(comment.ToString());

			_timer.Start();
		}

		protected virtual void RenderingEndDiagnostics()
		{
			if (!DiagnosticsEnabled) return;

			// <!-- End Rendering "~/bar/Foo.ascx" -->
			_timer.Stop();
			_writer.Write("<!-- End Rendering {0}, render took {1}ms -->", _renderingName, _timer.ElapsedMilliseconds);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && DiagnosticsEnabled)
				RenderingEndDiagnostics();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
