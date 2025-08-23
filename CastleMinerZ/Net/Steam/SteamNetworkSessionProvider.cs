using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.Distribution.Steam;
using DNA.Net.GamerServices;
using DNA.Net.Lidgren;
using DNA.Net.MatchMaking;

namespace DNA.CastleMinerZ.Net.Steam
{
	public class SteamNetworkSessionProvider : NetworkSessionProvider
	{
		public static NetworkSession CreateNetworkSession(NetworkSessionStaticProvider staticprovider, SteamWorks steamAPI)
		{
			SteamNetworkSessionProvider provider = new SteamNetworkSessionProvider(staticprovider, steamAPI);
			NetworkSession result = new NetworkSession(provider);
			provider._networkSession = result;
			return result;
		}

		protected SteamNetworkSessionProvider(NetworkSessionStaticProvider staticProvider, SteamWorks steamAPI)
			: base(staticProvider)
		{
			this._steamAPI = steamAPI;
		}

		public override void StartHost(NetworkSessionStaticProvider.BeginCreateSessionState sqs)
		{
			if (sqs.SessionType != NetworkSessionType.Local)
			{
				CreateSessionInfo csi = new CreateSessionInfo();
				csi.SessionProperties = sqs.Properties;
				csi.MaxPlayers = sqs.MaxPlayers;
				csi.Name = sqs.ServerMessage;
				csi.PasswordProtected = !string.IsNullOrEmpty(sqs.Password);
				csi.JoinGamePolicy = JoinGamePolicy.Anyone;
				this._steamAPI.CreateLobby(csi, new LobbyCreatedDelegate(this.OnLobbyCreated), sqs);
				return;
			}
			this.OnLobbyCreated(new HostSessionInfo
			{
				JoinGamePolicy = JoinGamePolicy.InviteOnly,
				Name = sqs.ServerMessage,
				SessionProperties = sqs.Properties,
				PasswordProtected = false
			}, sqs);
		}

		protected void OnLobbyCreated(HostSessionInfo hostInfo, object context)
		{
			NetworkSessionStaticProvider.BeginCreateSessionState sqs = (NetworkSessionStaticProvider.BeginCreateSessionState)context;
			this.HostSessionInfo = hostInfo;
			this._isHost = true;
			this._sessionID = MathTools.RandomInt();
			this._sessionType = sqs.SessionType;
			this._maxPlayers = sqs.MaxPlayers;
			this._signedInGamers = new List<SignedInGamer>(sqs.LocalGamers);
			this._gameName = sqs.NetworkGameName;
			this._properties = sqs.Properties;
			this._version = sqs.Version;
			if (!string.IsNullOrWhiteSpace(this._password))
			{
				this._password = sqs.Password;
			}
			if (hostInfo == null)
			{
				sqs.ExceptionEncountered = new Exception("Could not create steam lobby");
				this._hostConnectionResult = NetworkSession.ResultCode.ExceptionThrown;
				this._hostConnectionResultString = "Could not create steam lobby";
			}
			else
			{
				this._hostConnectionResult = NetworkSession.ResultCode.Succeeded;
				base.AddLocalGamer(this._signedInGamers[0], true, 0, this._steamAPI.SteamPlayerID);
			}
			sqs.Event.Set();
			if (sqs.Callback != null)
			{
				sqs.Callback(sqs);
			}
		}

		public override void StartClientInvited(ulong lobbyId, NetworkSessionStaticProvider.BeginJoinSessionState sqs, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			this._hostConnectionResult = NetworkSession.ResultCode.Pending;
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData context = new SteamNetworkSessionProvider.KeeperOfTheInvitedGameData(getPasswordCallback, null, lobbyId, sqs);
			this._steamAPI.GetInvitedGameInfo(lobbyId, new SessionUpdatedDelegate(this.SessionUpdatedCallback), context);
		}

		private void SessionUpdatedCallback(ulong lobbyid, GameUpdateResultCode updateresult, ClientSessionInfo session, object context)
		{
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData gameData = context as SteamNetworkSessionProvider.KeeperOfTheInvitedGameData;
			gameData.SessionInfo = session;
			if (updateresult == GameUpdateResultCode.Success)
			{
				gameData.State.AvailableSession = new AvailableNetworkSession(session);
				TaskDispatcher.Instance.AddTaskForMainThread(new TaskDelegate(this.ValidateInvitedGameAndStartJoining), context);
				return;
			}
			switch (updateresult)
			{
			case GameUpdateResultCode.NoLongerValid:
				this._hostConnectionResultString = "Steam no longer running";
				this._hostConnectionResult = NetworkSession.ResultCode.Timeout;
				return;
			case GameUpdateResultCode.UnknownGame:
				this._hostConnectionResultString = "Game Not Found";
				this._hostConnectionResult = NetworkSession.ResultCode.Timeout;
				return;
			default:
				return;
			}
		}

		private void ValidateInvitedGameAndStartJoining(BaseTask task, object context)
		{
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData gameData = context as SteamNetworkSessionProvider.KeeperOfTheInvitedGameData;
			gameData.Callback(gameData.SessionInfo, gameData, new SetPasswordForInvitedGameCallback(this.GotSessionPasswordCallback));
		}

		private void GotSessionPasswordCallback(bool cancelled, string password, string errorString, object context)
		{
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData gameData = context as SteamNetworkSessionProvider.KeeperOfTheInvitedGameData;
			if (cancelled)
			{
				this._hostConnectionResultString = errorString;
				this._hostConnectionResult = NetworkSession.ResultCode.UnknownResult;
				return;
			}
			gameData.State.Password = password;
			this.StartClient(gameData.State);
		}

		public override void StartClient(NetworkSessionStaticProvider.BeginJoinSessionState sqs)
		{
			this._isHost = false;
			this._sessionType = sqs.SessionType;
			this._sessionID = sqs.AvailableSession.SessionID;
			this._properties = sqs.AvailableSession.SessionProperties;
			this._maxPlayers = sqs.AvailableSession.MaxGamerCount;
			this._signedInGamers = new List<SignedInGamer>(sqs.LocalGamers);
			this._gameName = sqs.NetworkGameName;
			this._version = sqs.Version;
			this._hostConnectionResult = NetworkSession.ResultCode.Pending;
			if (this._sessionType != NetworkSessionType.Local)
			{
				RequestConnectToHostMessage crm = new RequestConnectToHostMessage();
				crm.SessionID = this._sessionID;
				crm.SessionProperties = this._properties;
				crm.Password = sqs.Password;
				crm.Gamer = this._signedInGamers[0];
				SteamNetBuffer nom = this._steamAPI.AllocSteamNetBuffer();
				nom.Write(crm, this._gameName, this._version);
				this._steamAPI.JoinGame(sqs.AvailableSession.LobbySteamID, sqs.AvailableSession.HostSteamID, nom);
			}
		}

		private void SendRemoteData(SteamNetBuffer msg, NetDeliveryMethod flags, NetworkGamer recipient)
		{
			ulong c = recipient.AlternateAddress;
			if (c != 0UL)
			{
				this._steamAPI.SendPacket(msg, c, flags, 0);
			}
		}

		private NetDeliveryMethod GetDeliveryMethodFromOptions(SendDataOptions options)
		{
			NetDeliveryMethod flags = NetDeliveryMethod.Unknown;
			switch (options)
			{
			case SendDataOptions.None:
				flags = NetDeliveryMethod.Unreliable;
				break;
			case SendDataOptions.Reliable:
				flags = NetDeliveryMethod.ReliableUnordered;
				break;
			case SendDataOptions.InOrder:
				flags = NetDeliveryMethod.UnreliableSequenced;
				break;
			case SendDataOptions.ReliableInOrder:
				flags = NetDeliveryMethod.ReliableOrdered;
				break;
			}
			return flags;
		}

		private void PrepareMessageForSending(SendDataOptions options, NetworkGamer recipient, out SteamNetBuffer msg, out int channel, out ulong netConnection, out NetDeliveryMethod flags)
		{
			if (recipient.NetProxyObject)
			{
				msg = this._steamAPI.AllocSteamNetBuffer();
				flags = this.GetDeliveryMethodFromOptions(options);
				channel = 1;
				netConnection = this._host.AlternateAddress;
				msg.Write(3);
				msg.Write(recipient.Id);
				msg.Write((byte)flags);
				msg.Write(this._localPlayerGID);
				if (flags == NetDeliveryMethod.ReliableUnordered)
				{
					flags = NetDeliveryMethod.ReliableOrdered;
					return;
				}
			}
			else
			{
				ulong c = recipient.AlternateAddress;
				if (c != 0UL)
				{
					msg = this._steamAPI.AllocSteamNetBuffer();
					flags = this.GetDeliveryMethodFromOptions(options);
					msg.Write(recipient.Id);
					msg.Write(this._localPlayerGID);
					channel = 0;
					netConnection = c;
					return;
				}
				msg = null;
				channel = 0;
				flags = NetDeliveryMethod.Unknown;
				netConnection = 0UL;
			}
		}

		public override void SendRemoteData(byte[] data, SendDataOptions options, NetworkGamer recipient)
		{
			SteamNetBuffer msg;
			int channel;
			ulong netConnection;
			NetDeliveryMethod flags;
			this.PrepareMessageForSending(options, recipient, out msg, out channel, out netConnection, out flags);
			if (netConnection != 0UL)
			{
				msg.WriteArray(data);
				this._steamAPI.SendPacket(msg, netConnection, flags, channel);
			}
		}

		public override void SendRemoteData(byte[] data, int offset, int length, SendDataOptions options, NetworkGamer recipient)
		{
			SteamNetBuffer msg;
			int channel;
			ulong netConnection;
			NetDeliveryMethod flags;
			this.PrepareMessageForSending(options, recipient, out msg, out channel, out netConnection, out flags);
			if (netConnection != 0UL)
			{
				msg.WriteArray(data, offset, length);
				this._steamAPI.SendPacket(msg, netConnection, flags, channel);
			}
		}

		private void PrepareBroadcastMessageForSending(SendDataOptions options, out SteamNetBuffer msg, out NetDeliveryMethod flags)
		{
			msg = this._steamAPI.AllocSteamNetBuffer();
			flags = this.GetDeliveryMethodFromOptions(options);
			msg.Write(4);
			msg.Write((byte)flags);
			msg.Write(this._localPlayerGID);
			if (flags == NetDeliveryMethod.ReliableUnordered)
			{
				flags = NetDeliveryMethod.ReliableOrdered;
			}
		}

		public override void BroadcastRemoteData(byte[] data, SendDataOptions options)
		{
			ulong netConnection = this._host.AlternateAddress;
			if (netConnection != 0UL)
			{
				SteamNetBuffer msg;
				NetDeliveryMethod flags;
				this.PrepareBroadcastMessageForSending(options, out msg, out flags);
				msg.WriteArray(data);
				this._steamAPI.SendPacket(msg, netConnection, flags, 1);
			}
		}

		public override void BroadcastRemoteData(byte[] data, int offset, int length, SendDataOptions options)
		{
			ulong netConnection = this._host.AlternateAddress;
			if (netConnection != 0UL)
			{
				SteamNetBuffer msg;
				NetDeliveryMethod flags;
				this.PrepareBroadcastMessageForSending(options, out msg, out flags);
				msg.WriteArray(data, offset, length);
				this._steamAPI.SendPacket(msg, netConnection, flags, 1);
			}
		}

		private bool HandleHostStatusChangedMessage(SteamNetBuffer msg)
		{
			bool messageHandled = true;
			switch (msg.ReadByte())
			{
			case 5:
			{
				ConnectedMessage cm = new ConnectedMessage();
				byte newGID;
				do
				{
					if (this._nextPlayerGID == 0)
					{
						this._nextPlayerGID = 1;
					}
					byte nextPlayerGID;
					this._nextPlayerGID = (nextPlayerGID = this._nextPlayerGID) + 1;
					newGID = nextPlayerGID;
				}
				while (this._idToGamer.ContainsKey(newGID));
				cm.PlayerGID = newGID;
				cm.SetPeerList(this._allGamers);
				SteamNetBuffer omsg = this._steamAPI.AllocSteamNetBuffer();
				omsg.Write(1);
				omsg.Write(cm);
				this._steamAPI.SendPacket(omsg, msg.SenderId, NetDeliveryMethod.ReliableOrdered, 1);
				NetworkGamer newGamer = base.AddRemoteGamer(this._steamIDToGamer[msg.SenderId], msg.SenderId, false, cm.PlayerGID);
				this._steamIDToGamer[msg.SenderId] = newGamer;
				using (List<NetworkGamer>.Enumerator enumerator = this._remoteGamers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NetworkGamer ng = enumerator.Current;
						if (ng.AlternateAddress != msg.SenderId)
						{
							omsg = this._steamAPI.AllocSteamNetBuffer();
							omsg.Write(0);
							omsg.Write(newGamer.Id);
							omsg.Write(newGamer);
							this._steamAPI.SendPacket(omsg, ng.AlternateAddress, NetDeliveryMethod.ReliableOrdered, 1);
						}
					}
					return messageHandled;
				}
				break;
			}
			case 6:
				goto IL_023E;
			case 7:
				break;
			default:
				goto IL_023E;
			}
			if (!this._steamIDToGamer.ContainsKey(msg.SenderId))
			{
				return messageHandled;
			}
			NetworkGamer g = this._steamIDToGamer[msg.SenderId] as NetworkGamer;
			if (g != null)
			{
				DropPeerMessage dropPeer = new DropPeerMessage();
				dropPeer.PlayerGID = g.Id;
				foreach (NetworkGamer ng2 in this._remoteGamers)
				{
					if (ng2.AlternateAddress != msg.SenderId)
					{
						SteamNetBuffer om = this._steamAPI.AllocSteamNetBuffer();
						om.Write(2);
						om.Write(dropPeer);
						this._steamAPI.SendPacket(om, ng2.AlternateAddress, NetDeliveryMethod.ReliableOrdered, 1);
					}
				}
				this._steamIDToGamer.Remove(msg.SenderId);
				base.RemoveGamer(g);
				return messageHandled;
			}
			return messageHandled;
			IL_023E:
			messageHandled = false;
			return messageHandled;
		}

		private bool HandleHostSystemMessages(SteamNetBuffer msg)
		{
			bool result = true;
			switch (msg.ReadByte())
			{
			case 3:
			{
				byte recipientId = msg.ReadByte();
				NetDeliveryMethod flags = (NetDeliveryMethod)msg.ReadByte();
				NetworkGamer recipient = this.FindGamerById(recipientId);
				if (recipient != null)
				{
					byte senderId = msg.ReadByte();
					SteamNetBuffer omsg = this._steamAPI.AllocSteamNetBuffer();
					omsg.Write(recipientId);
					omsg.Write(senderId);
					omsg.CopyByteArrayFrom(msg);
					this._steamAPI.SendPacket(omsg, recipient.AlternateAddress, flags, 0);
				}
				break;
			}
			case 4:
			{
				NetDeliveryMethod flags = (NetDeliveryMethod)msg.ReadByte();
				byte senderId = msg.ReadByte();
				byte[] data = null;
				int dataSize = msg.ReadInt32();
				int offset = 0;
				bool dataIsAligned = false;
				if (dataSize > 0)
				{
					dataIsAligned = msg.GetAlignedData(out data, out offset);
					if (!dataIsAligned)
					{
						data = msg.ReadBytes(dataSize);
					}
				}
				LocalNetworkGamer host = this.FindGamerById(0) as LocalNetworkGamer;
				if (host != null)
				{
					NetworkGamer sender = this.FindGamerById(senderId);
					if (dataIsAligned)
					{
						host.AppendNewDataPacket(data, offset, dataSize, sender);
					}
					else
					{
						host.AppendNewDataPacket(data, sender);
					}
					for (int i = 0; i < this._remoteGamers.Count; i++)
					{
						if (this._remoteGamers[i].Id != senderId)
						{
							ulong c = this._remoteGamers[i].AlternateAddress;
							if (c != 0UL)
							{
								SteamNetBuffer omsg2 = this._steamAPI.AllocSteamNetBuffer();
								omsg2.Write(this._remoteGamers[i].Id);
								omsg2.Write(senderId);
								omsg2.Write(dataSize);
								if (dataSize > 0)
								{
									omsg2.Write(data, offset, dataSize);
								}
								this._steamAPI.SendPacket(omsg2, c, flags, 0);
							}
						}
					}
				}
				break;
			}
			}
			return result;
		}

		private void HandleHostConnectionApproval(SteamNetBuffer msg)
		{
			RequestConnectToHostMessage crm = msg.ReadRequestConnectToHostMessage(this._gameName, this._version);
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.GameNameInvalid)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.GameNamesDontMatch);
				return;
			}
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.VersionInvalid)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsLower)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasNewerVersion);
				return;
			}
			if (!string.IsNullOrWhiteSpace(this._password) && (string.IsNullOrWhiteSpace(crm.Password) || !crm.Password.Equals(this._password)))
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.IncorrectPassword);
				return;
			}
			if (this.AllowConnectionCallbackAlt != null && !this.AllowConnectionCallbackAlt(crm.Gamer.PlayerID, msg.SenderId))
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ConnectionDenied);
				return;
			}
			if (crm.SessionProperties.Count != this._properties.Count)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.SessionPropertiesDontMatch);
				return;
			}
			for (int i = 0; i < crm.SessionProperties.Count; i++)
			{
				if (this._properties[i] != null && crm.SessionProperties[i] != this._properties[i])
				{
					this.FailConnection(msg.SenderId, NetworkSession.ResultCode.SessionPropertiesDontMatch);
					return;
				}
			}
			GamerCollection<NetworkGamer> gamers = base.AllGamers;
			for (int j = 0; j < gamers.Count; j++)
			{
				bool failConnection = false;
				if (gamers[j] == null)
				{
					failConnection = true;
				}
				else if (gamers[j].AlternateAddress == msg.SenderId)
				{
					failConnection = true;
				}
				else if (gamers[j].Gamertag == crm.Gamer.Gamertag)
				{
					failConnection = true;
				}
				if (failConnection)
				{
					this.FailConnection(msg.SenderId, NetworkSession.ResultCode.GamerAlreadyConnected);
					return;
				}
			}
			this._steamIDToGamer[msg.SenderId] = crm.Gamer;
			this._steamAPI.AcceptConnection(msg.SenderId);
		}

		private bool HandleHostMessages(SteamNetBuffer msg)
		{
			bool messageHandled = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType != NetIncomingMessageType.StatusChanged)
			{
				if (messageType != NetIncomingMessageType.ConnectionApproval)
				{
					if (messageType != NetIncomingMessageType.Data)
					{
						messageHandled = false;
					}
					else
					{
						if (msg.Channel == 1)
						{
							return this.HandleHostSystemMessages(msg);
						}
						messageHandled = false;
					}
				}
				else
				{
					this.HandleHostConnectionApproval(msg);
				}
				return messageHandled;
			}
			return this.HandleHostStatusChangedMessage(msg);
		}

		private void AddNewPeer(SteamNetBuffer msg)
		{
			byte id = msg.ReadByte();
			Gamer newGamer = msg.ReadGamer();
			base.AddProxyGamer(newGamer, false, id);
		}

		private bool HandleClientSystemMessages(SteamNetBuffer msg)
		{
			bool result = true;
			InternalMessageTypes msgType = (InternalMessageTypes)msg.ReadByte();
			NetworkGamer g = null;
			switch (msgType)
			{
			case InternalMessageTypes.NewPeer:
				this.AddNewPeer(msg);
				return result;
			case InternalMessageTypes.ResponseToConnection:
			{
				ConnectedMessage cm = msg.ReadConnectedMessage();
				base.AddLocalGamer(this._signedInGamers[0], false, cm.PlayerGID, this._steamAPI.SteamPlayerID);
				for (int i = 0; i < cm.Peers.Length; i++)
				{
					if (cm.ids[i] == 0)
					{
						base.AddRemoteGamer(cm.Peers[i], msg.SenderId, true, 0);
					}
					else
					{
						base.AddProxyGamer(cm.Peers[i], false, cm.ids[i]);
					}
				}
				return result;
			}
			case InternalMessageTypes.DropPeer:
			{
				DropPeerMessage dropPeer = msg.ReadDropPeerMessage();
				if (this._idToGamer.TryGetValue(dropPeer.PlayerGID, out g))
				{
					base.RemoveGamer(g);
					return result;
				}
				return result;
			}
			case InternalMessageTypes.SessionPropertiesChanged:
			{
				NetworkSessionProperties newProps = msg.ReadSessionProps();
				for (int j = 0; j < newProps.Count; j++)
				{
					if (newProps[j] != null && this._properties[j] != null)
					{
						this._properties[j] = newProps[j];
					}
				}
				return result;
			}
			}
			result = false;
			return result;
		}

		private void HandleClientStatusChangedMessage(SteamNetBuffer msg)
		{
			switch (msg.ReadByte())
			{
			case 5:
				this._hostConnectionResult = NetworkSession.ResultCode.Succeeded;
				this._hostConnectionResultString = this._hostConnectionResult.ToString();
				return;
			case 6:
				break;
			case 7:
				base.HandleDisconnection(msg.ReadString());
				break;
			default:
				return;
			}
		}

		private bool HandleClientMessages(SteamNetBuffer msg)
		{
			bool messageHandled = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType != NetIncomingMessageType.StatusChanged)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					messageHandled = false;
				}
				else
				{
					if (msg.Channel == 1)
					{
						return this.HandleClientSystemMessages(msg);
					}
					messageHandled = false;
				}
			}
			else
			{
				this.HandleClientStatusChangedMessage(msg);
			}
			return messageHandled;
		}

		private void FailConnection(ulong c, NetworkSession.ResultCode reason)
		{
			this._steamAPI.Deny(c, reason.ToString());
		}

		private bool HandleCommonMessages(SteamNetBuffer msg)
		{
			bool messageHandled = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType <= NetIncomingMessageType.VerboseDebugMessage)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					if (messageType == NetIncomingMessageType.VerboseDebugMessage)
					{
						return messageHandled;
					}
				}
				else
				{
					byte pid = msg.ReadByte();
					NetworkGamer gamer = this.FindGamerById(pid);
					if (gamer == null)
					{
						return messageHandled;
					}
					LocalNetworkGamer localGamer = gamer as LocalNetworkGamer;
					if (localGamer == null)
					{
						return messageHandled;
					}
					byte senderid = msg.ReadByte();
					NetworkGamer ng = this.FindGamerById(senderid);
					if (ng != null)
					{
						byte[] data = msg.ReadByteArray();
						localGamer.AppendNewDataPacket(data, ng);
						return messageHandled;
					}
					return messageHandled;
				}
			}
			else if (messageType == NetIncomingMessageType.DebugMessage || messageType == NetIncomingMessageType.WarningMessage || messageType == NetIncomingMessageType.ErrorMessage)
			{
				return messageHandled;
			}
			messageHandled = false;
			return messageHandled;
		}

		public override void Update()
		{
			bool msgAvailable = this._steamAPI.Update();
			if (msgAvailable)
			{
				SteamNetBuffer msg;
				while ((msg = this._steamAPI.GetPacket()) != null)
				{
					if (!(this._isHost ? this.HandleHostMessages(msg) : this.HandleClientMessages(msg)))
					{
						bool messageHandled = this.HandleCommonMessages(msg);
					}
					this._steamAPI.FreeSteamNetBuffer(msg);
				}
			}
		}

		public override void Dispose(bool disposeManagedObjects)
		{
			this._staticProvider.TaskScheduler.Exit();
			if (this._steamAPI.InSession)
			{
				this._steamAPI.LeaveSession();
			}
			base.Dispose(disposeManagedObjects);
		}

		public override void ReportClientJoined(string username)
		{
		}

		public override void ReportClientLeft(string username)
		{
		}

		public override void ReportSessionAlive()
		{
		}

		public override void UpdateHostSession(string serverName, bool? passwordProtected, bool? isPublic, NetworkSessionProperties sessionProps)
		{
			if (!string.IsNullOrWhiteSpace(serverName))
			{
				this.HostSessionInfo.Name = serverName;
			}
			if (passwordProtected != null)
			{
				this.HostSessionInfo.PasswordProtected = passwordProtected.Value;
			}
			if (sessionProps != null)
			{
				this.HostSessionInfo.SessionProperties = sessionProps;
			}
			this._steamAPI.UpdateHostLobbyData(this.HostSessionInfo);
		}

		public override void UpdateHostSessionJoinPolicy(JoinGamePolicy joinGamePolicy)
		{
			this.HostSessionInfo.JoinGamePolicy = joinGamePolicy;
			this._steamAPI.UpdateHostLobbyData(this.HostSessionInfo);
		}

		public override void CloseNetworkSession()
		{
		}

		private SteamWorks _steamAPI;

		protected int _sessionID;

		protected Dictionary<ulong, Gamer> _steamIDToGamer = new Dictionary<ulong, Gamer>();

		private class KeeperOfTheInvitedGameData
		{
			public KeeperOfTheInvitedGameData(GetPasswordForInvitedGameCallback callback, ClientSessionInfo info, ulong id, NetworkSessionStaticProvider.BeginJoinSessionState state)
			{
				this.Callback = callback;
				this.SessionInfo = info;
				this.LobbyId = id;
				this.State = state;
			}

			public GetPasswordForInvitedGameCallback Callback;

			public ClientSessionInfo SessionInfo;

			public ulong LobbyId;

			public NetworkSessionStaticProvider.BeginJoinSessionState State;
		}
	}
}
