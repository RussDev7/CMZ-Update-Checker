using System;
using System.Reflection;
using System.Windows.Forms;
using DNA.CastleMinerZ.Net.Steam;
using DNA.Diagnostics.IssueReporting;
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
			SteamOnlineServices steamOnlineServices = new SteamOnlineServices(guid, num);
			if (!steamOnlineServices.OperationWasSuccessful)
			{
				SteamErrorCode errorCode = steamOnlineServices.ErrorCode;
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
			CastleMinerZGame.GlobalSettings.Load();
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			NetworkSession.StaticProvider = new SteamNetworkSessionStaticProvider(steamOnlineServices.SteamAPI);
			NetworkSession.NetworkSessionServices = new SteamNetworkSessionServices(steamOnlineServices.SteamAPI, guid, 4);
			IssueReporter issueReporter = new BacktraceIssueReporter(version, steamOnlineServices.SteamUserID, steamOnlineServices.Username);
			CastleMinerZGame.TrialMode = false;
			DNAGame.Run<CastleMinerZGame>(issueReporter, steamOnlineServices);
			steamOnlineServices.Dispose();
		}
	}
}
