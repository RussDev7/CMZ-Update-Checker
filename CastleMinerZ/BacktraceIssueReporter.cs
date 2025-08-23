using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Backtrace;
using Backtrace.Model;
using DNA.Diagnostics.IssueReporting;

namespace DNA.CastleMinerZ
{
	internal class BacktraceIssueReporter : IssueReporter
	{
		public BacktraceIssueReporter(Version version, ulong userID, string userName)
		{
			this._version = version ?? new Version(0, 0);
			this._userID = userID;
			this._userName = userName ?? "";
		}

		public void UpdateIdentity(ulong userId, string userName)
		{
			this._userID = userId;
			this._userName = userName ?? "";
		}

		public void RegisterGlobalHandlers()
		{
			if (Debugger.IsAttached)
			{
				return;
			}
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
			{
				this.SafeReport(e.Exception, "Application.ThreadException");
			};
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
			{
				Exception ex = (e.ExceptionObject as Exception) ?? new Exception("Non-exception unhandled error");
				this.SafeReport(ex, "AppDomain.UnhandledException");
			};
			TaskScheduler.UnobservedTaskException += delegate(object sender, UnobservedTaskExceptionEventArgs e)
			{
				try
				{
					this.SafeReport(e.Exception, "TaskScheduler.UnobservedTaskException");
				}
				finally
				{
					e.SetObserved();
				}
			};
		}

		public override void ReportCrash(Exception e)
		{
			this.SafeReport(e, "IssueReporter.ReportCrash");
			base.ReportCrash(e);
		}

		public override void ReportStat(string stat, string value)
		{
			base.ReportStat(stat, value);
		}

		private void EnsureClientCreated()
		{
			if (this._clientCreated)
			{
				return;
			}
			lock (this._clientLock)
			{
				if (!this._clientCreated)
				{
					BacktraceCredentials credentials = new BacktraceCredentials("https://ddna.sp.backtrace.io:6098", "c1063bda6b0c2a2e5b0497799d94d290f3678bfd1a6b90f615869c63b61349a8");
					BacktraceClientConfiguration cfg = new BacktraceClientConfiguration(credentials);
					Dictionary<string, object> baseAttr = new Dictionary<string, object>();
					baseAttr["version"] = this._version.ToString();
					baseAttr["assembly"] = Assembly.GetExecutingAssembly().GetName().Name;
					if (this._userID != 0UL)
					{
						baseAttr["userid_hash"] = BacktraceIssueReporter.HashSteamId(this._userID);
					}
					cfg.ClientAttributes = baseAttr;
					this._client = new BacktraceClient(cfg, null);
					this._clientCreated = true;
				}
			}
		}

		private void SafeReport(Exception ex, string sourceTag)
		{
			try
			{
				if (ex == null)
				{
					ex = new Exception("Unknown exception (null)");
				}
				this.EnsureClientCreated();
				StackTrace st = new StackTrace(ex, true);
				Dictionary<string, object> attrs = new Dictionary<string, object>();
				attrs["managed_stack"] = st.ToString();
				attrs["exception_source"] = sourceTag;
				this._client.Send(new BacktraceReport(ex, attrs, null, true));
			}
			catch
			{
			}
		}

		private static string HashSteamId(ulong steamId)
		{
			string input = steamId.ToString() + "CastleMinerZSecretSalt2025";
			string text;
			using (SHA256 sha = SHA256.Create())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(input);
				byte[] hash = sha.ComputeHash(bytes);
				StringBuilder sb = new StringBuilder(hash.Length * 2);
				foreach (byte b in hash)
				{
					sb.Append(b.ToString("x2"));
				}
				text = sb.ToString();
			}
			return text;
		}

		private const string BacktraceUrl = "https://ddna.sp.backtrace.io:6098";

		private const string BacktraceToken = "c1063bda6b0c2a2e5b0497799d94d290f3678bfd1a6b90f615869c63b61349a8";

		private const string SecretSalt = "CastleMinerZSecretSalt2025";

		private readonly Version _version;

		private ulong _userID;

		private string _userName = "";

		private BacktraceClient _client;

		private readonly object _clientLock = new object();

		private bool _clientCreated;
	}
}
