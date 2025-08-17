using System;
using DNA.Distribution.Steam;

namespace DNA.Distribution
{
	public class SteamOnlineServices : OnlineServices, IDisposable
	{
		public SteamOnlineServices(Guid productID, uint steamID)
			: base(productID)
		{
			this.SteamAPI = new SteamWorks(steamID);
			if (this.SteamAPI.OperationWasSuccessful)
			{
				this._username = this.SteamAPI.SteamName;
				this._steamUserID = this.SteamAPI.SteamPlayerID;
			}
		}

		public override DateTime GetServerTime()
		{
			return DateTime.UtcNow;
		}

		public override void Update(TimeSpan elapsedTime, TimeSpan totalTime)
		{
			if (!this._disposed && this.SteamAPI != null)
			{
				this.SteamAPI.MinimalUpdate();
			}
		}

		public override bool ValidateLicense(string userName, string password, out string reason)
		{
			this._username = userName;
			reason = "success";
			return true;
		}

		public override bool ValidateLicenseFacebook(string facebookID, string accessToken, out string username, out string reason)
		{
			reason = "facebookUser";
			username = "facebookUser";
			this._username = reason;
			return true;
		}

		public override void AcceptTerms(string userName, string password)
		{
		}

		public override void AcceptTermsFacebook(string facebookID)
		{
		}

		public override bool RegisterFacebook(string facebookID, string accessToken, string email, string userName, string password, out string reason)
		{
			reason = "success";
			return true;
		}

		public override string GetLauncherPage()
		{
			return "http://www.castleminer.com";
		}

		public override string GetProductTitle()
		{
			return "Null Title";
		}

		public override int? GetAddOn(Guid guid)
		{
			return null;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing && this.SteamAPI != null)
				{
					this.SteamAPI.Unintialize();
				}
				this.SteamAPI = null;
				this._disposed = true;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		public bool OperationWasSuccessful
		{
			get
			{
				return this.SteamAPI == null || this.SteamAPI.OperationWasSuccessful;
			}
		}

		public SteamErrorCode ErrorCode
		{
			get
			{
				if (this.SteamAPI != null)
				{
					return this.SteamAPI.ErrorCode;
				}
				return SteamErrorCode.Disposed;
			}
		}

		public SteamWorks SteamAPI;

		private bool _disposed;
	}
}
