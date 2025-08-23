using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.Distribution.Steam;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net.Steam
{
	public class SteamNetworkSessionStaticProvider : NetworkSessionStaticProvider
	{
		protected override NetworkSession CreateSession()
		{
			return SteamNetworkSessionProvider.CreateNetworkSession(this, this._steamAPI);
		}

		public SteamNetworkSessionStaticProvider(SteamWorks steamAPI)
		{
			this._steamAPI = steamAPI;
			this._steamAPI.SetOnGameLobbyJoinRequestedCallback(new GameLobbyJoinRequestedDelegate(this.OnJoinLobbyRequested), null);
		}

		private void OnJoinLobbyRequested(ulong lobbyid, ulong inviter, object context)
		{
			base.CallInviteAccepted(new InviteAcceptedEventArgs(Gamer.SignedInGamers[PlayerIndex.One], false)
			{
				LobbyId = lobbyid,
				InviterId = inviter
			});
		}

		protected override void FinishBeginCreate(NetworkSessionStaticProvider.BeginCreateSessionState state)
		{
			state.Session.StartHost(state);
			if (state.SessionType != NetworkSessionType.Local)
			{
				this._steamAPI.AllowMinimalUpdates = false;
				base.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.WaitForHostToStart), state);
			}
		}

		private void WaitForHostToStart(object state)
		{
			try
			{
				NetworkSessionStaticProvider.BeginCreateSessionState sqs = (NetworkSessionStaticProvider.BeginCreateSessionState)state;
				while ((sqs.Session.HostConnectionResult != NetworkSession.ResultCode.Succeeded || sqs.Session.LocalGamers.Count <= 0) && sqs.Session.HostConnectionResult <= NetworkSession.ResultCode.Succeeded)
				{
					Thread.Sleep(100);
					sqs.Session.Update();
				}
				if (sqs.ExceptionEncountered == null)
				{
					sqs.ExceptionEncountered = new Exception("Unable to start steam lobby");
				}
				TaskDispatcher.Instance.AddTaskForMainThread(delegate(object obj)
				{
					NetworkSessionStaticProvider.BeginCreateSessionState bjss = obj as NetworkSessionStaticProvider.BeginCreateSessionState;
					bjss.Event.Set();
					if (bjss.Callback != null)
					{
						bjss.Callback(bjss);
					}
				}, sqs);
			}
			finally
			{
				this._steamAPI.AllowMinimalUpdates = true;
			}
		}

		protected override void FinishBeginJoinInvited(ulong lobbyId, NetworkSessionStaticProvider.BeginJoinSessionState state, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			state.Session.StartClientInvited(lobbyId, state, getPasswordCallback);
			this._steamAPI.AllowMinimalUpdates = false;
			base.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.WaitForClientToStart), state);
		}

		protected override void FinishBeginJoin(NetworkSessionStaticProvider.BeginJoinSessionState state)
		{
			state.Session.StartClient(state);
			this._steamAPI.AllowMinimalUpdates = false;
			base.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.WaitForClientToStart), state);
		}

		private void WaitForClientToStart(object state)
		{
			Stopwatch timeWaiting = Stopwatch.StartNew();
			try
			{
				NetworkSessionStaticProvider.BeginJoinSessionState sqs = (NetworkSessionStaticProvider.BeginJoinSessionState)state;
				int retries = 4;
				NetworkSession.ResultCode resultCode;
				string resultString;
				while (sqs.Session.HostConnectionResult != NetworkSession.ResultCode.Succeeded || sqs.Session.LocalGamers.Count <= 0)
				{
					if (timeWaiting.Elapsed.TotalSeconds > 15.0)
					{
						if (retries == 0)
						{
							resultCode = NetworkSession.ResultCode.Timeout;
							resultString = "Server or Steam is not responding";
							goto IL_00DF;
						}
						retries--;
						sqs.Session.ResetHostConnectionResult();
						sqs.Session.StartClient(sqs);
						timeWaiting.Restart();
					}
					if (sqs.Session.HostConnectionResult <= NetworkSession.ResultCode.Succeeded)
					{
						Thread.Sleep(100);
						sqs.Session.Update();
						continue;
					}
					resultCode = sqs.Session.HostConnectionResult;
					resultString = sqs.Session.HostConnectionResultString;
					IL_00DF:
					sqs.HostConnectionResult = resultCode;
					sqs.HostConnectionResultString = resultString;
					TaskDispatcher.Instance.AddTaskForMainThread(delegate(object obj)
					{
						NetworkSessionStaticProvider.BeginJoinSessionState bjss = obj as NetworkSessionStaticProvider.BeginJoinSessionState;
						bjss.Event.Set();
						if (bjss.Callback != null)
						{
							bjss.Callback(bjss);
						}
					}, sqs);
					return;
				}
				resultCode = sqs.Session.HostConnectionResult;
				resultString = sqs.Session.HostConnectionResultString;
				goto IL_00DF;
			}
			finally
			{
				this._steamAPI.AllowMinimalUpdates = true;
			}
		}

		protected override void FinishBeginFind(NetworkSessionStaticProvider.SessionQueryState state)
		{
			state.Sessions = null;
			state.ClientSessionsFound = null;
			this._steamAPI.FindGames(state.SearchProperties, new SessionsFoundDelegate(this.OnSessionsFound), state);
			this._steamAPI.AllowMinimalUpdates = false;
			base.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.WaitForFindToComplete), state);
		}

		protected void OnSessionsFound(List<ClientSessionInfo> clientSessions, object context)
		{
			NetworkSessionStaticProvider.SessionQueryState sqs = (NetworkSessionStaticProvider.SessionQueryState)context;
			sqs.ClientSessionsFound = new List<ClientSessionInfo>(clientSessions);
		}

		private void WaitForFindToComplete(object state)
		{
			try
			{
				Stopwatch timeWaiting = Stopwatch.StartNew();
				NetworkSessionStaticProvider.SessionQueryState sqs = (NetworkSessionStaticProvider.SessionQueryState)state;
				while (sqs.ClientSessionsFound == null)
				{
					Thread.Sleep(100);
					this._steamAPI.Update();
					if (timeWaiting.Elapsed.TotalSeconds > 5.0)
					{
						this._steamAPI.StopFindingGames();
					}
				}
				List<AvailableNetworkSession> sessions = new List<AvailableNetworkSession>();
				foreach (ClientSessionInfo info in sqs.ClientSessionsFound)
				{
					sessions.Add(new AvailableNetworkSession(info));
				}
				sqs.Sessions = new AvailableNetworkSessionCollection(sessions);
				sqs.ClientSessionsFound.Clear();
				sqs.ClientSessionsFound = null;
				sqs.Event.Set();
				if (sqs.Callback != null)
				{
					sqs.Callback(sqs);
				}
			}
			finally
			{
				this._steamAPI.AllowMinimalUpdates = true;
			}
		}

		public override HostDiscovery GetHostDiscoveryObject(string gamename, int version, PlayerID playerID)
		{
			return new SteamHostDiscovery(this._steamAPI, gamename, version, playerID);
		}

		private SteamWorks _steamAPI;
	}
}
