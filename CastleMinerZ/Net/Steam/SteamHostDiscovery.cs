using System;
using System.Diagnostics;
using DNA.Distribution.Steam;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;

namespace DNA.CastleMinerZ.Net.Steam
{
	internal class SteamHostDiscovery : HostDiscovery
	{
		public SteamHostDiscovery(SteamWorks steam, string gamename, int version, PlayerID playerID)
			: base(gamename, version, playerID)
		{
			this._steamAPI = steam;
			this._steamAPI.SetOnGameUpdatedCallback(new SessionUpdatedDelegate(this.SessionUpdatedCallback), null);
			this.Timeout = 10f;
		}

		public override int GetHostInfo(ulong lobbyid, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			int result = this._nextWaitingID++;
			HostDiscovery.WaitingForResponse wc = new HostDiscovery.WaitingForResponse();
			wc.Callback = callback;
			wc.Context = context;
			wc.WaitingID = result;
			wc.SteamLobbyID = lobbyid;
			this._awaitingResponse.Add(wc);
			wc.Timer = Stopwatch.StartNew();
			this._steamAPI.GetUpdatedGameInfo(lobbyid);
			return result;
		}

		private void SessionUpdatedCallback(ulong lobbyid, GameUpdateResultCode updateresult, ClientSessionInfo session, object context)
		{
			HostDiscovery.WaitingForResponse waiter = this.FindWaiterBySteamID(lobbyid);
			if (waiter != null)
			{
				AvailableNetworkSession netsession = null;
				HostDiscovery.ResultCode result = HostDiscovery.ResultCode.ConnectionDenied;
				switch (updateresult)
				{
				case GameUpdateResultCode.Success:
					netsession = new AvailableNetworkSession(session);
					result = HostDiscovery.ResultCode.Success;
					break;
				case GameUpdateResultCode.NoLongerValid:
					result = HostDiscovery.ResultCode.ConnectionDenied;
					break;
				case GameUpdateResultCode.UnknownGame:
					result = HostDiscovery.ResultCode.ConnectionDenied;
					break;
				}
				waiter.Callback(result, netsession, waiter.Context);
			}
		}

		private HostDiscovery.WaitingForResponse FindWaiterBySteamID(ulong lobbyId)
		{
			lock (this._awaitingResponse)
			{
				for (int i = 0; i < this._awaitingResponse.Count; i++)
				{
					if (this._awaitingResponse[i].SteamLobbyID == lobbyId)
					{
						HostDiscovery.WaitingForResponse result = this._awaitingResponse[i];
						this._awaitingResponse.RemoveAt(i);
						return result;
					}
				}
			}
			return null;
		}

		public override void Update()
		{
			base.Update();
		}

		public override void Shutdown()
		{
			this._steamAPI.ClearOnGameUpdatedCallback();
		}

		private SteamWorks _steamAPI;
	}
}
