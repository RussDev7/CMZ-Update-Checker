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
					BacktraceCredentials backtraceCredentials = new BacktraceCredentials("https://ddna.sp.backtrace.io:6098", "c1063bda6b0c2a2e5b0497799d94d290f3678bfd1a6b90f615869c63b61349a8");
					BacktraceClientConfiguration backtraceClientConfiguration = new BacktraceClientConfiguration(backtraceCredentials);
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary["version"] = this._version.ToString();
					dictionary["assembly"] = Assembly.GetExecutingAssembly().GetName().Name;
					if (this._userID != 0UL)
					{
						dictionary["userid_hash"] = BacktraceIssueReporter.HashSteamId(this._userID);
					}
					backtraceClientConfiguration.ClientAttributes = dictionary;
					this._client = new BacktraceClient(backtraceClientConfiguration, null);
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
				StackTrace stackTrace = new StackTrace(ex, true);
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary["managed_stack"] = stackTrace.ToString();
				dictionary["exception_source"] = sourceTag;
				this._client.Send(new BacktraceReport(ex, dictionary, null, true));
			}
			catch
			{
			}
		}

		private static string HashSteamId(ulong steamId)
		{
			string text = steamId.ToString() + "CastleMinerZSecretSalt2025";
			string text2;
			using (SHA256 sha = SHA256.Create())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				byte[] array = sha.ComputeHash(bytes);
				StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
				foreach (byte b in array)
				{
					stringBuilder.Append(b.ToString("x2"));
				}
				text2 = stringBuilder.ToString();
			}
			return text2;
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
