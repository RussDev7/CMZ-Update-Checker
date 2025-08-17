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
			int num = this._nextWaitingID++;
			HostDiscovery.WaitingForResponse waitingForResponse = new HostDiscovery.WaitingForResponse();
			waitingForResponse.Callback = callback;
			waitingForResponse.Context = context;
			waitingForResponse.WaitingID = num;
			waitingForResponse.SteamLobbyID = lobbyid;
			this._awaitingResponse.Add(waitingForResponse);
			waitingForResponse.Timer = Stopwatch.StartNew();
			this._steamAPI.GetUpdatedGameInfo(lobbyid);
			return num;
		}

		private void SessionUpdatedCallback(ulong lobbyid, GameUpdateResultCode updateresult, ClientSessionInfo session, object context)
		{
			HostDiscovery.WaitingForResponse waitingForResponse = this.FindWaiterBySteamID(lobbyid);
			if (waitingForResponse != null)
			{
				AvailableNetworkSession availableNetworkSession = null;
				HostDiscovery.ResultCode resultCode = HostDiscovery.ResultCode.ConnectionDenied;
				switch (updateresult)
				{
				case GameUpdateResultCode.Success:
					availableNetworkSession = new AvailableNetworkSession(session);
					resultCode = HostDiscovery.ResultCode.Success;
					break;
				case GameUpdateResultCode.NoLongerValid:
					resultCode = HostDiscovery.ResultCode.ConnectionDenied;
					break;
				case GameUpdateResultCode.UnknownGame:
					resultCode = HostDiscovery.ResultCode.ConnectionDenied;
					break;
				}
				waitingForResponse.Callback(resultCode, availableNetworkSession, waitingForResponse.Context);
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
						HostDiscovery.WaitingForResponse waitingForResponse = this._awaitingResponse[i];
						this._awaitingResponse.RemoveAt(i);
						return waitingForResponse;
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
