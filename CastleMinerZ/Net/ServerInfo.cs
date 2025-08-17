using System;
using System.Diagnostics;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.UI;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class ServerInfo
	{
		public bool IsValidIP
		{
			get
			{
				return this._isValidIP;
			}
		}

		public bool WaitingForResponseFromHost
		{
			get
			{
				return this._nextResult == HostDiscovery.ResultCode.Pending;
			}
		}

		public bool PasswordProtected
		{
			get
			{
				return this._passwordProtected;
			}
		}

		public bool ReadyToJoin
		{
			get
			{
				return this._currentResult == HostDiscovery.ResultCode.Success && this.Session != null;
			}
		}

		public GameDifficultyTypes GameDifficulty
		{
			get
			{
				return this._gameDifficulty;
			}
		}

		public string DateCreatedStr
		{
			get
			{
				return this._dateCreated;
			}
		}

		public DateTime DateCreated
		{
			get
			{
				if (this.Session != null)
				{
					return this.Session.DateCreated;
				}
				return DateTime.MinValue;
			}
		}

		public string GameDifficultyString
		{
			get
			{
				switch (this._gameDifficulty)
				{
				case GameDifficultyTypes.EASY:
					return Strings.Easy;
				case GameDifficultyTypes.HARD:
					return Strings.Hard;
				case GameDifficultyTypes.NOENEMIES:
					return Strings.No_Enemies;
				case GameDifficultyTypes.HARDCORE:
					return Strings.Hardcore;
				default:
					return "";
				}
			}
		}

		public GameModeTypes GameMode
		{
			get
			{
				return this._gameMode;
			}
		}

		public string GameModeString
		{
			get
			{
				switch (this._gameMode)
				{
				case GameModeTypes.Endurance:
					return Strings.Endurance;
				case GameModeTypes.Survival:
					return Strings.Survival;
				case GameModeTypes.DragonEndurance:
					return Strings.Dragon_Endurance;
				case GameModeTypes.Creative:
					return Strings.Creative;
				case GameModeTypes.Exploration:
					return Strings.Exploration;
				case GameModeTypes.Scavenger:
					return Strings.Scavenger;
				default:
					if (this._infiniteResourceMode)
					{
						return Strings.Creative;
					}
					return "";
				}
			}
		}

		public string HostUsername
		{
			get
			{
				return this._hostUsername;
			}
			set
			{
				this._hostUsername = value;
			}
		}

		public string ServerName
		{
			get
			{
				switch (this._currentResult)
				{
				case HostDiscovery.ResultCode.Pending:
				case HostDiscovery.ResultCode.Success:
					return this._serverName;
				default:
					return this._errorMessage;
				}
			}
		}

		public string IPAddressString
		{
			get
			{
				return this._ipAddressString;
			}
		}

		public int NumberPlayers
		{
			get
			{
				return this._numPlayers;
			}
		}

		public string NumberPlayerString
		{
			get
			{
				return this._numPlayersString;
			}
		}

		public int MaxPlayers
		{
			get
			{
				return this._maxPlayers;
			}
		}

		public string MaxPlayersString
		{
			get
			{
				return this._maxPlayersString;
			}
		}

		public bool IsOnline
		{
			get
			{
				return this._isOnline;
			}
		}

		public string PVPstr
		{
			get
			{
				switch (this._pvp)
				{
				case CastleMinerZGame.PVPEnum.Off:
					return Strings.Off;
				case CastleMinerZGame.PVPEnum.Everyone:
					return Strings.Everyone;
				case CastleMinerZGame.PVPEnum.NotFriends:
					return Strings.Non_Friends_Only;
				default:
					return "";
				}
			}
		}

		public int NumFriends
		{
			get
			{
				return this._numFriends;
			}
		}

		public string NumFriendsStr
		{
			get
			{
				return this._numFriendsStr;
			}
		}

		public int Proximity
		{
			get
			{
				return this._proximity;
			}
		}

		public ServerInfo(AvailableNetworkSession session)
		{
			if (session.IPEndPoint == null)
			{
				this._ipAddressString = "No ip address";
			}
			else
			{
				this._ipAddressString = session.IPEndPoint.Address.ToString();
			}
			this._isValidIP = true;
			this._isOnline = true;
			this.Session = session;
			this._serverName = session.ServerMessage;
			this._hostUsername = session.HostGamertag;
			this._numPlayers = session.CurrentGamerCount;
			this._numPlayersString = this._numPlayers.ToString();
			this._maxPlayers = session.MaxGamerCount;
			this._maxPlayersString = this._maxPlayers.ToString();
			this._passwordProtected = session.PasswordProtected;
			this._numFriends = session.FriendCount;
			this._numFriendsStr = this._numFriends.ToString();
			this._proximity = session.Proximity;
			this._gameMode = (GameModeTypes)session.SessionProperties[2].Value;
			this._gameDifficulty = (GameDifficultyTypes)session.SessionProperties[3].Value;
			this._infiniteResourceMode = session.SessionProperties[4] == 1;
			if (session.SessionProperties[5] != null)
			{
				this._pvp = (CastleMinerZGame.PVPEnum)session.SessionProperties[5].Value;
				return;
			}
			this._pvp = CastleMinerZGame.PVPEnum.Off;
		}

		public void DiscoveryCallback(HostDiscovery.ResultCode result, AvailableNetworkSession session, object context)
		{
			this._nextResult = result;
			this._currentResult = result;
			this._requestID = -1;
			this._refreshing = false;
			switch (result)
			{
			case HostDiscovery.ResultCode.Success:
				this._isValidIP = true;
				this._isOnline = true;
				this.Session = session;
				this._serverName = session.ServerMessage;
				this._hostUsername = session.HostGamertag;
				this._numPlayers = session.CurrentGamerCount;
				this._numPlayersString = this._numPlayers.ToString();
				this._maxPlayers = session.MaxGamerCount;
				this._maxPlayersString = this._maxPlayers.ToString();
				this._passwordProtected = session.PasswordProtected;
				this._gameMode = (GameModeTypes)session.SessionProperties[2].Value;
				this._gameDifficulty = (GameDifficultyTypes)session.SessionProperties[3].Value;
				this._infiniteResourceMode = session.SessionProperties[4] == 1;
				if (session.SessionProperties[5] != null)
				{
					this._pvp = (CastleMinerZGame.PVPEnum)session.SessionProperties[5].Value;
					return;
				}
				this._pvp = CastleMinerZGame.PVPEnum.Off;
				return;
			case HostDiscovery.ResultCode.TimedOut:
				this._isValidIP = true;
				this._isOnline = false;
				this._errorMessage = Strings.Waiting_for_server_to_respond____;
				return;
			case HostDiscovery.ResultCode.FailedToResolveHostName:
				this._isValidIP = false;
				this._isOnline = false;
				this._errorMessage = Strings.Could_not_resolve_host_name;
				return;
			case HostDiscovery.ResultCode.HostNameInvalid:
				this._isValidIP = false;
				this._isOnline = false;
				this._errorMessage = Strings.Host_name_is_not_valid;
				return;
			case HostDiscovery.ResultCode.WrongGameName:
				this._isValidIP = true;
				this._isOnline = false;
				this._errorMessage = Strings.This_is_not_a_CastleMiner_Z_server;
				break;
			case HostDiscovery.ResultCode.ServerHasNewerVersion:
				this._isValidIP = true;
				this._isOnline = false;
				this._errorMessage = Strings.Version_mismatch__Server_is_newer;
				return;
			case HostDiscovery.ResultCode.ServerHasOlderVersion:
				this._isValidIP = true;
				this._isOnline = false;
				this._errorMessage = Strings.Version_mismatch__Server_is_older;
				return;
			case HostDiscovery.ResultCode.VersionIsInvalid:
				this._isValidIP = true;
				this._isOnline = false;
				this._errorMessage = Strings.Version_number_was_invalid;
				return;
			case HostDiscovery.ResultCode.GamerAlreadyConnected:
				break;
			case HostDiscovery.ResultCode.ConnectionDenied:
				this._isValidIP = true;
				this._isOnline = false;
				this._errorMessage = Strings.Connection_was_denied_by_server;
				return;
			default:
				return;
			}
		}

		public void RefreshServerStatus(HostDiscovery discovery)
		{
			if ((this._updateTimer == null || this._updateTimer.Elapsed.TotalSeconds > 2.0) && !this.WaitingForResponseFromHost && (this._currentResult == HostDiscovery.ResultCode.Success || this._currentResult == HostDiscovery.ResultCode.TimedOut))
			{
				this.UpdateServerStatus(discovery);
			}
		}

		public void UpdateServerStatus(HostDiscovery discovery)
		{
			this._updateTimer = Stopwatch.StartNew();
			if (this.WaitingForResponseFromHost)
			{
				discovery.RemovePendingRequest(this._requestID);
			}
			this._nextResult = HostDiscovery.ResultCode.Pending;
			this._requestID = discovery.GetHostInfo(this.Session.LobbySteamID, new HostDiscovery.HostDiscoveryCallback(this.DiscoveryCallback), null);
		}

		private const float _sRetryInterval = 2f;

		public AvailableNetworkSession Session;

		private string _ipAddressString;

		private string _serverName = "";

		private string _errorMessage = Strings.Waiting_for_server_to_respond____;

		private string _previousMessage = "";

		private int _numPlayers;

		private string _numPlayersString = "0";

		private int _maxPlayers;

		private string _maxPlayersString = "0";

		private bool _isOnline = true;

		private bool _passwordProtected;

		private string _hostUsername = "";

		private string _dateCreated;

		private CastleMinerZGame.PVPEnum _pvp;

		private bool _isValidIP = true;

		private bool _refreshing;

		private HostDiscovery.ResultCode _nextResult;

		private HostDiscovery.ResultCode _currentResult = HostDiscovery.ResultCode.TimedOut;

		private GameModeTypes _gameMode;

		private GameDifficultyTypes _gameDifficulty;

		private bool _infiniteResourceMode;

		private int _requestID = -1;

		private Stopwatch _updateTimer;

		private int _numFriends;

		private string _numFriendsStr;

		private int _proximity;
	}
}
