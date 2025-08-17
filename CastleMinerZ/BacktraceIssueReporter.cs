using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Backtrace;
using Backtrace.Model;
using DNA.Diagnostics.IssueReporting;

namespace DNA.CastleMinerZ
{
	internal class BacktraceIssueReporter : IssueReporter
	{
		public BacktraceIssueReporter(Version version, ulong userID, string userName)
		{
			this._userID = userID;
			this._userName = userName;
			this._version = version;
			BacktraceCredentials backtraceCredentials = new BacktraceCredentials("https://ddna.sp.backtrace.io:6098", "c1063bda6b0c2a2e5b0497799d94d290f3678bfd1a6b90f615869c63b61349a8");
			new Dictionary<string, object>();
			BacktraceClientConfiguration backtraceClientConfiguration = new BacktraceClientConfiguration(backtraceCredentials);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["version"] = this._version.ToString();
			dictionary["userid"] = BacktraceIssueReporter.HashSteamId(this._userID);
			backtraceClientConfiguration.ClientAttributes = dictionary;
			this.backtraceClient = new BacktraceClient(backtraceClientConfiguration, null);
		}

		public static string HashSteamId(ulong steamId)
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

		public override void ReportCrash(Exception e)
		{
			this.backtraceClient.Send(new BacktraceReport(e, null, null, true));
			base.ReportCrash(e);
		}

		public override void ReportStat(string stat, string value)
		{
			base.ReportStat(stat, value);
		}

		private const string SecretSalt = "CastleMinerZSecretSalt2025";

		private BacktraceClient backtraceClient;

		private Version _version;

		private ulong _userID;

		private string _userName;
	}
}
