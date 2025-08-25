using System;
using DNA.Distribution.Steam;

namespace DNA.Distribution
{
	public class SteamOnlineServices : OnlineServices, IDisposable
	{
		public SteamOnlineServices(uint steamID)
		{
			this.SteamAPI = new SteamWorks(steamID);
			if (this.SteamAPI.OperationWasSuccessful)
			{
				this._username = this.SteamAPI.SteamName;
				this._steamUserID = this.SteamAPI.SteamPlayerID;
			}
		}

		public override void Update(TimeSpan elapsedTime, TimeSpan totalTime)
		{
			if (!this._disposed && this.SteamAPI != null)
			{
				this.SteamAPI.MinimalUpdate();
			}
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
