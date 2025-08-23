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
			BacktraceIssueReporter reporter = new BacktraceIssueReporter(version, 0UL, "");
			if (!Debugger.IsAttached)
			{
				reporter.RegisterGlobalHandlers();
			}
			else
			{
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
			}
			SteamOnlineServices licenseServices;
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
				Guid productID = Guid.Parse("FAE62948-F4E6-4F18-9D73-ED507466057F");
				uint SteamAppID = 253430U;
				CommonAssembly.Initalize();
				licenseServices = new SteamOnlineServices(productID, SteamAppID);
				if (!licenseServices.OperationWasSuccessful)
				{
					SteamErrorCode steamError = licenseServices.ErrorCode;
					reporter.ReportCrash(new Exception("Steam init failed: " + steamError));
					licenseServices.Dispose();
					string errorMsg = "Unspecified Error";
					SteamErrorCode steamErrorCode = steamError;
					if (steamErrorCode == SteamErrorCode.CantInitAPI)
					{
						errorMsg = "Steam may not be running";
					}
					MessageBox.Show(errorMsg, "Error Running Program");
					return;
				}
				reporter.UpdateIdentity(licenseServices.SteamUserID, licenseServices.Username);
				CastleMinerZGame.GlobalSettings.Load();
				NetworkSession.StaticProvider = new SteamNetworkSessionStaticProvider(licenseServices.SteamAPI);
				NetworkSession.NetworkSessionServices = new SteamNetworkSessionServices(licenseServices.SteamAPI, productID, 4);
				CastleMinerZGame.TrialMode = false;
			}
			catch (Exception fatal)
			{
				reporter.ReportCrash(fatal);
				throw;
			}
			if (Debugger.IsAttached)
			{
				DNAGame.Run<CastleMinerZGame>(reporter, licenseServices);
				return;
			}
			try
			{
				DNAGame.Run<CastleMinerZGame>(reporter, licenseServices);
			}
			catch (Exception exRun)
			{
				reporter.ReportCrash(exRun);
			}
		}
	}
}
