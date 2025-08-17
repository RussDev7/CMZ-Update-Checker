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
			SteamNetworkSessionProvider steamNetworkSessionProvider = new SteamNetworkSessionProvider(staticprovider, steamAPI);
			NetworkSession networkSession = new NetworkSession(steamNetworkSessionProvider);
			steamNetworkSessionProvider._networkSession = networkSession;
			return networkSession;
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
				CreateSessionInfo createSessionInfo = new CreateSessionInfo();
				createSessionInfo.SessionProperties = sqs.Properties;
				createSessionInfo.MaxPlayers = sqs.MaxPlayers;
				createSessionInfo.Name = sqs.ServerMessage;
				createSessionInfo.PasswordProtected = !string.IsNullOrEmpty(sqs.Password);
				createSessionInfo.JoinGamePolicy = JoinGamePolicy.Anyone;
				this._steamAPI.CreateLobby(createSessionInfo, new LobbyCreatedDelegate(this.OnLobbyCreated), sqs);
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
			NetworkSessionStaticProvider.BeginCreateSessionState beginCreateSessionState = (NetworkSessionStaticProvider.BeginCreateSessionState)context;
			this.HostSessionInfo = hostInfo;
			this._isHost = true;
			this._sessionID = MathTools.RandomInt();
			this._sessionType = beginCreateSessionState.SessionType;
			this._maxPlayers = beginCreateSessionState.MaxPlayers;
			this._signedInGamers = new List<SignedInGamer>(beginCreateSessionState.LocalGamers);
			this._gameName = beginCreateSessionState.NetworkGameName;
			this._properties = beginCreateSessionState.Properties;
			this._version = beginCreateSessionState.Version;
			if (!string.IsNullOrWhiteSpace(this._password))
			{
				this._password = beginCreateSessionState.Password;
			}
			if (hostInfo == null)
			{
				beginCreateSessionState.ExceptionEncountered = new Exception("Could not create steam lobby");
				this._hostConnectionResult = NetworkSession.ResultCode.ExceptionThrown;
				this._hostConnectionResultString = "Could not create steam lobby";
			}
			else
			{
				this._hostConnectionResult = NetworkSession.ResultCode.Succeeded;
				base.AddLocalGamer(this._signedInGamers[0], true, 0, this._steamAPI.SteamPlayerID);
			}
			beginCreateSessionState.Event.Set();
			if (beginCreateSessionState.Callback != null)
			{
				beginCreateSessionState.Callback(beginCreateSessionState);
			}
		}

		public override void StartClientInvited(ulong lobbyId, NetworkSessionStaticProvider.BeginJoinSessionState sqs, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			this._hostConnectionResult = NetworkSession.ResultCode.Pending;
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData keeperOfTheInvitedGameData = new SteamNetworkSessionProvider.KeeperOfTheInvitedGameData(getPasswordCallback, null, lobbyId, sqs);
			this._steamAPI.GetInvitedGameInfo(lobbyId, new SessionUpdatedDelegate(this.SessionUpdatedCallback), keeperOfTheInvitedGameData);
		}

		private void SessionUpdatedCallback(ulong lobbyid, GameUpdateResultCode updateresult, ClientSessionInfo session, object context)
		{
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData keeperOfTheInvitedGameData = context as SteamNetworkSessionProvider.KeeperOfTheInvitedGameData;
			keeperOfTheInvitedGameData.SessionInfo = session;
			if (updateresult == GameUpdateResultCode.Success)
			{
				keeperOfTheInvitedGameData.State.AvailableSession = new AvailableNetworkSession(session);
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
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData keeperOfTheInvitedGameData = context as SteamNetworkSessionProvider.KeeperOfTheInvitedGameData;
			keeperOfTheInvitedGameData.Callback(keeperOfTheInvitedGameData.SessionInfo, keeperOfTheInvitedGameData, new SetPasswordForInvitedGameCallback(this.GotSessionPasswordCallback));
		}

		private void GotSessionPasswordCallback(bool cancelled, string password, string errorString, object context)
		{
			SteamNetworkSessionProvider.KeeperOfTheInvitedGameData keeperOfTheInvitedGameData = context as SteamNetworkSessionProvider.KeeperOfTheInvitedGameData;
			if (cancelled)
			{
				this._hostConnectionResultString = errorString;
				this._hostConnectionResult = NetworkSession.ResultCode.UnknownResult;
				return;
			}
			keeperOfTheInvitedGameData.State.Password = password;
			this.StartClient(keeperOfTheInvitedGameData.State);
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
				RequestConnectToHostMessage requestConnectToHostMessage = new RequestConnectToHostMessage();
				requestConnectToHostMessage.SessionID = this._sessionID;
				requestConnectToHostMessage.SessionProperties = this._properties;
				requestConnectToHostMessage.Password = sqs.Password;
				requestConnectToHostMessage.Gamer = this._signedInGamers[0];
				SteamNetBuffer steamNetBuffer = this._steamAPI.AllocSteamNetBuffer();
				steamNetBuffer.Write(requestConnectToHostMessage, this._gameName, this._version);
				this._steamAPI.JoinGame(sqs.AvailableSession.LobbySteamID, sqs.AvailableSession.HostSteamID, steamNetBuffer);
			}
		}

		private void SendRemoteData(SteamNetBuffer msg, NetDeliveryMethod flags, NetworkGamer recipient)
		{
			ulong alternateAddress = recipient.AlternateAddress;
			if (alternateAddress != 0UL)
			{
				this._steamAPI.SendPacket(msg, alternateAddress, flags, 0);
			}
		}

		private NetDeliveryMethod GetDeliveryMethodFromOptions(SendDataOptions options)
		{
			NetDeliveryMethod netDeliveryMethod = NetDeliveryMethod.Unknown;
			switch (options)
			{
			case SendDataOptions.None:
				netDeliveryMethod = NetDeliveryMethod.Unreliable;
				break;
			case SendDataOptions.Reliable:
				netDeliveryMethod = NetDeliveryMethod.ReliableUnordered;
				break;
			case SendDataOptions.InOrder:
				netDeliveryMethod = NetDeliveryMethod.UnreliableSequenced;
				break;
			case SendDataOptions.ReliableInOrder:
				netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
				break;
			}
			return netDeliveryMethod;
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
				ulong alternateAddress = recipient.AlternateAddress;
				if (alternateAddress != 0UL)
				{
					msg = this._steamAPI.AllocSteamNetBuffer();
					flags = this.GetDeliveryMethodFromOptions(options);
					msg.Write(recipient.Id);
					msg.Write(this._localPlayerGID);
					channel = 0;
					netConnection = alternateAddress;
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
			SteamNetBuffer steamNetBuffer;
			int num;
			ulong num2;
			NetDeliveryMethod netDeliveryMethod;
			this.PrepareMessageForSending(options, recipient, out steamNetBuffer, out num, out num2, out netDeliveryMethod);
			if (num2 != 0UL)
			{
				steamNetBuffer.WriteArray(data);
				this._steamAPI.SendPacket(steamNetBuffer, num2, netDeliveryMethod, num);
			}
		}

		public override void SendRemoteData(byte[] data, int offset, int length, SendDataOptions options, NetworkGamer recipient)
		{
			SteamNetBuffer steamNetBuffer;
			int num;
			ulong num2;
			NetDeliveryMethod netDeliveryMethod;
			this.PrepareMessageForSending(options, recipient, out steamNetBuffer, out num, out num2, out netDeliveryMethod);
			if (num2 != 0UL)
			{
				steamNetBuffer.WriteArray(data, offset, length);
				this._steamAPI.SendPacket(steamNetBuffer, num2, netDeliveryMethod, num);
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
			ulong alternateAddress = this._host.AlternateAddress;
			if (alternateAddress != 0UL)
			{
				SteamNetBuffer steamNetBuffer;
				NetDeliveryMethod netDeliveryMethod;
				this.PrepareBroadcastMessageForSending(options, out steamNetBuffer, out netDeliveryMethod);
				steamNetBuffer.WriteArray(data);
				this._steamAPI.SendPacket(steamNetBuffer, alternateAddress, netDeliveryMethod, 1);
			}
		}

		public override void BroadcastRemoteData(byte[] data, int offset, int length, SendDataOptions options)
		{
			ulong alternateAddress = this._host.AlternateAddress;
			if (alternateAddress != 0UL)
			{
				SteamNetBuffer steamNetBuffer;
				NetDeliveryMethod netDeliveryMethod;
				this.PrepareBroadcastMessageForSending(options, out steamNetBuffer, out netDeliveryMethod);
				steamNetBuffer.WriteArray(data, offset, length);
				this._steamAPI.SendPacket(steamNetBuffer, alternateAddress, netDeliveryMethod, 1);
			}
		}

		private bool HandleHostStatusChangedMessage(SteamNetBuffer msg)
		{
			bool flag = true;
			switch (msg.ReadByte())
			{
			case 5:
			{
				ConnectedMessage connectedMessage = new ConnectedMessage();
				byte b;
				do
				{
					if (this._nextPlayerGID == 0)
					{
						this._nextPlayerGID = 1;
					}
					byte nextPlayerGID;
					this._nextPlayerGID = (nextPlayerGID = this._nextPlayerGID) + 1;
					b = nextPlayerGID;
				}
				while (this._idToGamer.ContainsKey(b));
				connectedMessage.PlayerGID = b;
				connectedMessage.SetPeerList(this._allGamers);
				SteamNetBuffer steamNetBuffer = this._steamAPI.AllocSteamNetBuffer();
				steamNetBuffer.Write(1);
				steamNetBuffer.Write(connectedMessage);
				this._steamAPI.SendPacket(steamNetBuffer, msg.SenderId, NetDeliveryMethod.ReliableOrdered, 1);
				NetworkGamer networkGamer = base.AddRemoteGamer(this._steamIDToGamer[msg.SenderId], msg.SenderId, false, connectedMessage.PlayerGID);
				this._steamIDToGamer[msg.SenderId] = networkGamer;
				using (List<NetworkGamer>.Enumerator enumerator = this._remoteGamers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NetworkGamer networkGamer2 = enumerator.Current;
						if (networkGamer2.AlternateAddress != msg.SenderId)
						{
							steamNetBuffer = this._steamAPI.AllocSteamNetBuffer();
							steamNetBuffer.Write(0);
							steamNetBuffer.Write(networkGamer.Id);
							steamNetBuffer.Write(networkGamer);
							this._steamAPI.SendPacket(steamNetBuffer, networkGamer2.AlternateAddress, NetDeliveryMethod.ReliableOrdered, 1);
						}
					}
					return flag;
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
				return flag;
			}
			NetworkGamer networkGamer3 = this._steamIDToGamer[msg.SenderId] as NetworkGamer;
			if (networkGamer3 != null)
			{
				DropPeerMessage dropPeerMessage = new DropPeerMessage();
				dropPeerMessage.PlayerGID = networkGamer3.Id;
				foreach (NetworkGamer networkGamer4 in this._remoteGamers)
				{
					if (networkGamer4.AlternateAddress != msg.SenderId)
					{
						SteamNetBuffer steamNetBuffer2 = this._steamAPI.AllocSteamNetBuffer();
						steamNetBuffer2.Write(2);
						steamNetBuffer2.Write(dropPeerMessage);
						this._steamAPI.SendPacket(steamNetBuffer2, networkGamer4.AlternateAddress, NetDeliveryMethod.ReliableOrdered, 1);
					}
				}
				this._steamIDToGamer.Remove(msg.SenderId);
				base.RemoveGamer(networkGamer3);
				return flag;
			}
			return flag;
			IL_023E:
			flag = false;
			return flag;
		}

		private bool HandleHostSystemMessages(SteamNetBuffer msg)
		{
			bool flag = true;
			switch (msg.ReadByte())
			{
			case 3:
			{
				byte b = msg.ReadByte();
				NetDeliveryMethod netDeliveryMethod = (NetDeliveryMethod)msg.ReadByte();
				NetworkGamer networkGamer = this.FindGamerById(b);
				if (networkGamer != null)
				{
					byte b2 = msg.ReadByte();
					SteamNetBuffer steamNetBuffer = this._steamAPI.AllocSteamNetBuffer();
					steamNetBuffer.Write(b);
					steamNetBuffer.Write(b2);
					steamNetBuffer.CopyByteArrayFrom(msg);
					this._steamAPI.SendPacket(steamNetBuffer, networkGamer.AlternateAddress, netDeliveryMethod, 0);
				}
				break;
			}
			case 4:
			{
				NetDeliveryMethod netDeliveryMethod = (NetDeliveryMethod)msg.ReadByte();
				byte b2 = msg.ReadByte();
				byte[] array = null;
				int num = msg.ReadInt32();
				int num2 = 0;
				bool flag2 = false;
				if (num > 0)
				{
					flag2 = msg.GetAlignedData(out array, out num2);
					if (!flag2)
					{
						array = msg.ReadBytes(num);
					}
				}
				LocalNetworkGamer localNetworkGamer = this.FindGamerById(0) as LocalNetworkGamer;
				if (localNetworkGamer != null)
				{
					NetworkGamer networkGamer2 = this.FindGamerById(b2);
					if (flag2)
					{
						localNetworkGamer.AppendNewDataPacket(array, num2, num, networkGamer2);
					}
					else
					{
						localNetworkGamer.AppendNewDataPacket(array, networkGamer2);
					}
					for (int i = 0; i < this._remoteGamers.Count; i++)
					{
						if (this._remoteGamers[i].Id != b2)
						{
							ulong alternateAddress = this._remoteGamers[i].AlternateAddress;
							if (alternateAddress != 0UL)
							{
								SteamNetBuffer steamNetBuffer2 = this._steamAPI.AllocSteamNetBuffer();
								steamNetBuffer2.Write(this._remoteGamers[i].Id);
								steamNetBuffer2.Write(b2);
								steamNetBuffer2.Write(num);
								if (num > 0)
								{
									steamNetBuffer2.Write(array, num2, num);
								}
								this._steamAPI.SendPacket(steamNetBuffer2, alternateAddress, netDeliveryMethod, 0);
							}
						}
					}
				}
				break;
			}
			}
			return flag;
		}

		private void HandleHostConnectionApproval(SteamNetBuffer msg)
		{
			RequestConnectToHostMessage requestConnectToHostMessage = msg.ReadRequestConnectToHostMessage(this._gameName, this._version);
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.GameNameInvalid)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.GameNamesDontMatch);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.VersionInvalid)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsLower)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ServerHasNewerVersion);
				return;
			}
			if (!string.IsNullOrWhiteSpace(this._password) && (string.IsNullOrWhiteSpace(requestConnectToHostMessage.Password) || !requestConnectToHostMessage.Password.Equals(this._password)))
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.IncorrectPassword);
				return;
			}
			if (this.AllowConnectionCallbackAlt != null && !this.AllowConnectionCallbackAlt(requestConnectToHostMessage.Gamer.PlayerID, msg.SenderId))
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.ConnectionDenied);
				return;
			}
			if (requestConnectToHostMessage.SessionProperties.Count != this._properties.Count)
			{
				this.FailConnection(msg.SenderId, NetworkSession.ResultCode.SessionPropertiesDontMatch);
				return;
			}
			for (int i = 0; i < requestConnectToHostMessage.SessionProperties.Count; i++)
			{
				if (this._properties[i] != null && requestConnectToHostMessage.SessionProperties[i] != this._properties[i])
				{
					this.FailConnection(msg.SenderId, NetworkSession.ResultCode.SessionPropertiesDontMatch);
					return;
				}
			}
			GamerCollection<NetworkGamer> allGamers = base.AllGamers;
			for (int j = 0; j < allGamers.Count; j++)
			{
				bool flag = false;
				if (allGamers[j] == null)
				{
					flag = true;
				}
				else if (allGamers[j].AlternateAddress == msg.SenderId)
				{
					flag = true;
				}
				else if (allGamers[j].Gamertag == requestConnectToHostMessage.Gamer.Gamertag)
				{
					flag = true;
				}
				if (flag)
				{
					this.FailConnection(msg.SenderId, NetworkSession.ResultCode.GamerAlreadyConnected);
					return;
				}
			}
			this._steamIDToGamer[msg.SenderId] = requestConnectToHostMessage.Gamer;
			this._steamAPI.AcceptConnection(msg.SenderId);
		}

		private bool HandleHostMessages(SteamNetBuffer msg)
		{
			bool flag = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType != NetIncomingMessageType.StatusChanged)
			{
				if (messageType != NetIncomingMessageType.ConnectionApproval)
				{
					if (messageType != NetIncomingMessageType.Data)
					{
						flag = false;
					}
					else
					{
						if (msg.Channel == 1)
						{
							return this.HandleHostSystemMessages(msg);
						}
						flag = false;
					}
				}
				else
				{
					this.HandleHostConnectionApproval(msg);
				}
				return flag;
			}
			return this.HandleHostStatusChangedMessage(msg);
		}

		private void AddNewPeer(SteamNetBuffer msg)
		{
			byte b = msg.ReadByte();
			Gamer gamer = msg.ReadGamer();
			base.AddProxyGamer(gamer, false, b);
		}

		private bool HandleClientSystemMessages(SteamNetBuffer msg)
		{
			bool flag = true;
			InternalMessageTypes internalMessageTypes = (InternalMessageTypes)msg.ReadByte();
			NetworkGamer networkGamer = null;
			switch (internalMessageTypes)
			{
			case InternalMessageTypes.NewPeer:
				this.AddNewPeer(msg);
				return flag;
			case InternalMessageTypes.ResponseToConnection:
			{
				ConnectedMessage connectedMessage = msg.ReadConnectedMessage();
				base.AddLocalGamer(this._signedInGamers[0], false, connectedMessage.PlayerGID, this._steamAPI.SteamPlayerID);
				for (int i = 0; i < connectedMessage.Peers.Length; i++)
				{
					if (connectedMessage.ids[i] == 0)
					{
						base.AddRemoteGamer(connectedMessage.Peers[i], msg.SenderId, true, 0);
					}
					else
					{
						base.AddProxyGamer(connectedMessage.Peers[i], false, connectedMessage.ids[i]);
					}
				}
				return flag;
			}
			case InternalMessageTypes.DropPeer:
			{
				DropPeerMessage dropPeerMessage = msg.ReadDropPeerMessage();
				if (this._idToGamer.TryGetValue(dropPeerMessage.PlayerGID, out networkGamer))
				{
					base.RemoveGamer(networkGamer);
					return flag;
				}
				return flag;
			}
			case InternalMessageTypes.SessionPropertiesChanged:
			{
				NetworkSessionProperties networkSessionProperties = msg.ReadSessionProps();
				for (int j = 0; j < networkSessionProperties.Count; j++)
				{
					if (networkSessionProperties[j] != null && this._properties[j] != null)
					{
						this._properties[j] = networkSessionProperties[j];
					}
				}
				return flag;
			}
			}
			flag = false;
			return flag;
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
			bool flag = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType != NetIncomingMessageType.StatusChanged)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					flag = false;
				}
				else
				{
					if (msg.Channel == 1)
					{
						return this.HandleClientSystemMessages(msg);
					}
					flag = false;
				}
			}
			else
			{
				this.HandleClientStatusChangedMessage(msg);
			}
			return flag;
		}

		private void FailConnection(ulong c, NetworkSession.ResultCode reason)
		{
			this._steamAPI.Deny(c, reason.ToString());
		}

		private bool HandleCommonMessages(SteamNetBuffer msg)
		{
			bool flag = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType <= NetIncomingMessageType.VerboseDebugMessage)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					if (messageType == NetIncomingMessageType.VerboseDebugMessage)
					{
						return flag;
					}
				}
				else
				{
					byte b = msg.ReadByte();
					NetworkGamer networkGamer = this.FindGamerById(b);
					if (networkGamer == null)
					{
						return flag;
					}
					LocalNetworkGamer localNetworkGamer = networkGamer as LocalNetworkGamer;
					if (localNetworkGamer == null)
					{
						return flag;
					}
					byte b2 = msg.ReadByte();
					NetworkGamer networkGamer2 = this.FindGamerById(b2);
					if (networkGamer2 != null)
					{
						byte[] array = msg.ReadByteArray();
						localNetworkGamer.AppendNewDataPacket(array, networkGamer2);
						return flag;
					}
					return flag;
				}
			}
			else if (messageType == NetIncomingMessageType.DebugMessage || messageType == NetIncomingMessageType.WarningMessage || messageType == NetIncomingMessageType.ErrorMessage)
			{
				return flag;
			}
			flag = false;
			return flag;
		}

		public override void Update()
		{
			bool flag = this._steamAPI.Update();
			if (flag)
			{
				SteamNetBuffer packet;
				while ((packet = this._steamAPI.GetPacket()) != null)
				{
					if (!(this._isHost ? this.HandleHostMessages(packet) : this.HandleClientMessages(packet)))
					{
						bool flag2 = this.HandleCommonMessages(packet);
					}
					this._steamAPI.FreeSteamNetBuffer(packet);
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
