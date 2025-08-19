using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using DNA.CastleMinerZ.Net.Steam;
using DNA.Distribution;
using DNA.Distribution.Steam;
using DNA.Net.GamerServices;
using DNA.Reflection;
using DNA.Text;

namespace DNA.CastleMinerZ
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			BacktraceIssueReporter backtraceIssueReporter = new BacktraceIssueReporter(version, 0UL, "");
			if (!Debugger.IsAttached)
			{
				backtraceIssueReporter.RegisterGlobalHandlers();
			}
			else
			{
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
			}
			SteamOnlineServices steamOnlineServices;
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				CommandLineArgs.ProcessArguments();
				if (CommandLineArgs.Get<CastleMinerZArgs>().ShowUsage)
				{
					MessageBox.Show(CommandLineArgs.Get<CastleMinerZArgs>().GetErrorUsageAndDescription());
					return;
				}
				Guid guid = Guid.Parse("FAE62948-F4E6-4F18-9D73-ED507466057F");
				uint num = 253430U;
				CommonAssembly.Initalize();
				steamOnlineServices = new SteamOnlineServices(guid, num);
				if (!steamOnlineServices.OperationWasSuccessful)
				{
					SteamErrorCode errorCode = steamOnlineServices.ErrorCode;
					backtraceIssueReporter.ReportCrash(new Exception("Steam init failed: " + errorCode));
					steamOnlineServices.Dispose();
					string text = "Unspecified Error";
					SteamErrorCode steamErrorCode = errorCode;
					if (steamErrorCode == SteamErrorCode.CantInitAPI)
					{
						text = "Steam may not be running";
					}
					MessageBox.Show(text, "Error Running Program");
					return;
				}
				backtraceIssueReporter.UpdateIdentity(steamOnlineServices.SteamUserID, steamOnlineServices.Username);
				CastleMinerZGame.GlobalSettings.Load();
				NetworkSession.StaticProvider = new SteamNetworkSessionStaticProvider(steamOnlineServices.SteamAPI);
				NetworkSession.NetworkSessionServices = new SteamNetworkSessionServices(steamOnlineServices.SteamAPI, guid, 4);
				CastleMinerZGame.TrialMode = false;
			}
			catch (Exception ex)
			{
				backtraceIssueReporter.ReportCrash(ex);
				throw;
			}
			if (Debugger.IsAttached)
			{
				DNAGame.Run<CastleMinerZGame>(backtraceIssueReporter, steamOnlineServices);
				return;
			}
			try
			{
				DNAGame.Run<CastleMinerZGame>(backtraceIssueReporter, steamOnlineServices);
			}
			catch (Exception ex2)
			{
				backtraceIssueReporter.ReportCrash(ex2);
			}
		}
	}
}
