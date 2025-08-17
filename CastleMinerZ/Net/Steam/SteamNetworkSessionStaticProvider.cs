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
				NetworkSessionStaticProvider.BeginCreateSessionState beginCreateSessionState = (NetworkSessionStaticProvider.BeginCreateSessionState)state;
				while ((beginCreateSessionState.Session.HostConnectionResult != NetworkSession.ResultCode.Succeeded || beginCreateSessionState.Session.LocalGamers.Count <= 0) && beginCreateSessionState.Session.HostConnectionResult <= NetworkSession.ResultCode.Succeeded)
				{
					Thread.Sleep(100);
					beginCreateSessionState.Session.Update();
				}
				if (beginCreateSessionState.ExceptionEncountered == null)
				{
					beginCreateSessionState.ExceptionEncountered = new Exception("Unable to start steam lobby");
				}
				TaskDispatcher.Instance.AddTaskForMainThread(delegate(object obj)
				{
					NetworkSessionStaticProvider.BeginCreateSessionState beginCreateSessionState2 = obj as NetworkSessionStaticProvider.BeginCreateSessionState;
					beginCreateSessionState2.Event.Set();
					if (beginCreateSessionState2.Callback != null)
					{
						beginCreateSessionState2.Callback(beginCreateSessionState2);
					}
				}, beginCreateSessionState);
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
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				NetworkSessionStaticProvider.BeginJoinSessionState beginJoinSessionState = (NetworkSessionStaticProvider.BeginJoinSessionState)state;
				int num = 4;
				NetworkSession.ResultCode resultCode;
				string text;
				while (beginJoinSessionState.Session.HostConnectionResult != NetworkSession.ResultCode.Succeeded || beginJoinSessionState.Session.LocalGamers.Count <= 0)
				{
					if (stopwatch.Elapsed.TotalSeconds > 15.0)
					{
						if (num == 0)
						{
							resultCode = NetworkSession.ResultCode.Timeout;
							text = "Server or Steam is not responding";
							goto IL_00DF;
						}
						num--;
						beginJoinSessionState.Session.ResetHostConnectionResult();
						beginJoinSessionState.Session.StartClient(beginJoinSessionState);
						stopwatch.Restart();
					}
					if (beginJoinSessionState.Session.HostConnectionResult <= NetworkSession.ResultCode.Succeeded)
					{
						Thread.Sleep(100);
						beginJoinSessionState.Session.Update();
						continue;
					}
					resultCode = beginJoinSessionState.Session.HostConnectionResult;
					text = beginJoinSessionState.Session.HostConnectionResultString;
					IL_00DF:
					beginJoinSessionState.HostConnectionResult = resultCode;
					beginJoinSessionState.HostConnectionResultString = text;
					TaskDispatcher.Instance.AddTaskForMainThread(delegate(object obj)
					{
						NetworkSessionStaticProvider.BeginJoinSessionState beginJoinSessionState2 = obj as NetworkSessionStaticProvider.BeginJoinSessionState;
						beginJoinSessionState2.Event.Set();
						if (beginJoinSessionState2.Callback != null)
						{
							beginJoinSessionState2.Callback(beginJoinSessionState2);
						}
					}, beginJoinSessionState);
					return;
				}
				resultCode = beginJoinSessionState.Session.HostConnectionResult;
				text = beginJoinSessionState.Session.HostConnectionResultString;
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
			NetworkSessionStaticProvider.SessionQueryState sessionQueryState = (NetworkSessionStaticProvider.SessionQueryState)context;
			sessionQueryState.ClientSessionsFound = new List<ClientSessionInfo>(clientSessions);
		}

		private void WaitForFindToComplete(object state)
		{
			try
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				NetworkSessionStaticProvider.SessionQueryState sessionQueryState = (NetworkSessionStaticProvider.SessionQueryState)state;
				while (sessionQueryState.ClientSessionsFound == null)
				{
					Thread.Sleep(100);
					this._steamAPI.Update();
					if (stopwatch.Elapsed.TotalSeconds > 5.0)
					{
						this._steamAPI.StopFindingGames();
					}
				}
				List<AvailableNetworkSession> list = new List<AvailableNetworkSession>();
				foreach (ClientSessionInfo clientSessionInfo in sessionQueryState.ClientSessionsFound)
				{
					list.Add(new AvailableNetworkSession(clientSessionInfo));
				}
				sessionQueryState.Sessions = new AvailableNetworkSessionCollection(list);
				sessionQueryState.ClientSessionsFound.Clear();
				sessionQueryState.ClientSessionsFound = null;
				sessionQueryState.Event.Set();
				if (sessionQueryState.Callback != null)
				{
					sessionQueryState.Callback(sessionQueryState);
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
