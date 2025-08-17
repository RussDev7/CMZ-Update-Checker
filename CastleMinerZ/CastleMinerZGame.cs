using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DNA.Audio;
using DNA.Avatars;
using DNA.CastleMinerZ.Achievements;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.Distribution;
using DNA.Drawing;
using DNA.Drawing.Animation;
using DNA.Drawing.UI;
using DNA.IO.Storage;
using DNA.Net;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;
using DNA.Profiling;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class CastleMinerZGame : DNAGame
	{
		public CastleMinerZGame.PVPEnum PVPState
		{
			get
			{
				if (this.IsOnlineGame && base.CurrentNetworkSession.SessionProperties[5] != null)
				{
					return (CastleMinerZGame.PVPEnum)base.CurrentNetworkSession.SessionProperties[5].Value;
				}
				return CastleMinerZGame.PVPEnum.Off;
			}
			set
			{
				if (this.IsOnlineGame && base.CurrentNetworkSession.SessionProperties[5] != (int)value)
				{
					base.CurrentNetworkSession.SessionProperties[5] = new int?((int)value);
					if (this.IsOnlineGame)
					{
						base.CurrentNetworkSession.UpdateHostSession(null, null, null, base.CurrentNetworkSession.SessionProperties);
					}
				}
			}
		}

		public bool IsEnduranceMode
		{
			get
			{
				return CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance || CastleMinerZGame.Instance.GameMode == GameModeTypes.DragonEndurance;
			}
		}

		public bool IsPublicGame
		{
			get
			{
				return this._joinGamePolicy == JoinGamePolicy.Anyone;
			}
			set
			{
				if (value != this.IsPublicGame)
				{
					this.JoinGamePolicy = (value ? JoinGamePolicy.Anyone : JoinGamePolicy.FriendsOnly);
				}
			}
		}

		public JoinGamePolicy JoinGamePolicy
		{
			get
			{
				return this._joinGamePolicy;
			}
			set
			{
				if (value != this._joinGamePolicy)
				{
					this._joinGamePolicy = value;
					if (this.IsOnlineGame)
					{
						base.CurrentNetworkSession.UpdateHostSessionJoinPolicy(this._joinGamePolicy);
					}
				}
			}
		}

		internal CastleMinerZArgs CommandLine
		{
			get
			{
				return CommandLineArgs.Get<CastleMinerZArgs>();
			}
		}

		public bool IsAvatarLoaded
		{
			get
			{
				return this._myAvatarDescription != null;
			}
		}

		public float LoadProgress
		{
			get
			{
				return (float)this._terrain.LoadingProgress / 100f;
			}
		}

		public bool IsOnlineGame
		{
			get
			{
				return base.CurrentNetworkSession != null && base.CurrentNetworkSession.SessionType == NetworkSessionType.PlayerMatch;
			}
		}

		public bool IsGameHost
		{
			get
			{
				return this.MyNetworkGamer != null && this.MyNetworkGamer.Id == this.TerrainServerID;
			}
		}

		public override string ServerMessage
		{
			get
			{
				string text = base.ServerMessage;
				if (text == null && this.CurrentWorld != null)
				{
					text = this.CurrentWorld.ServerMessage;
				}
				return text;
			}
			set
			{
				if (value != this.CurrentWorld.ServerMessage && this.IsOnlineGame)
				{
					base.CurrentNetworkSession.UpdateHostSession(value, null, null, null);
				}
				if (this.CurrentWorld != null)
				{
					this.CurrentWorld.ServerMessage = value;
				}
				base.ServerMessage = value;
			}
		}

		public Player LocalPlayer
		{
			get
			{
				return this._localPlayer;
			}
		}

		public override OnlineServices LicenseServices
		{
			set
			{
				base.LicenseServices = value;
			}
		}

		public CastleMinerZGame()
			: base(CastleMinerZGame.GlobalSettings.ScreenSize, false, CastleMinerZGame.GameVersion)
		{
			CastleMinerZGame.Instance = this;
			this._controllerMapping.SetToDefault();
			if (Debugger.IsAttached)
			{
				base.WantProfiling(false, true);
			}
			base.Content = new ProfiledContentManager(base.Services, "ReachContent", "HiDefContent", CastleMinerZGame.GlobalSettings.TextureQualityLevel);
			base.Content.RootDirectory = "Content";
			this.Graphics.PreparingDeviceSettings += GraphicsProfileManager.Instance.ExamineGraphicsDevices;
			Profiler.Profiling = false;
			Profiler.SetColor("Zombie Update", Color.Blue);
			Profiler.SetColor("Zombie Collision", Color.Red);
			Profiler.SetColor("Drawing Terrain", Color.Green);
			this.Graphics.SynchronizeWithVerticalRetrace = true;
			base.IsFixedTimeStep = false;
			this.PauseDuringGuide = false;
			this.StartGamerServices();
			TaskDispatcher.Create();
			base.Window.AllowUserResizing = false;
			this.IsFullScreen = CastleMinerZGame.GlobalSettings.FullScreen;
		}

		public bool IsFullScreen
		{
			get
			{
				return this.Graphics.IsFullScreen;
			}
			set
			{
				if (this.Graphics.IsFullScreen != value)
				{
					this.Graphics.IsFullScreen = value;
					this.Graphics.ApplyChanges();
				}
			}
		}

		public bool IsLocalPlayerId(byte id)
		{
			return this.LocalPlayer != null && this.LocalPlayer.Gamer != null && this.LocalPlayer.Gamer.Id == id;
		}

		protected override void SecondaryLoad()
		{
			SoundManager.ActiveListener = this.Listener;
			Texture2D texture2D = base.Content.Load<Texture2D>("UI\\Screens\\LoadScreen");
			LoadScreen loadScreen = new LoadScreen(texture2D, TimeSpan.FromSeconds(10.300000190734863));
			MainThreadMessageSender.Init();
			this.mainScreenGroup.PushScreen(loadScreen);
			base.ScreenManager.PushScreen(this.mainScreenGroup);
			base.ScreenManager.PushScreen(this.overlayScreenGroup);
			SoundManager.Instance.Load("Sounds");
			this.DaySounds = SoundManager.Instance.GetCatagory("AmbientDay");
			this.NightSounds = SoundManager.Instance.GetCatagory("AmbientNight");
			this.CaveSounds = SoundManager.Instance.GetCatagory("AmbientCave");
			this.MusicSounds = SoundManager.Instance.GetCatagory("Music");
			this.HellSounds = SoundManager.Instance.GetCatagory("AmbientHell");
			this.PlayMusic("Theme");
			this.SetAudio(1f, 0f, 0f, 0f);
			ControllerImages.Load(base.Content);
			this.MenuBackdrop = base.Content.Load<Texture2D>("UI\\Screens\\MenuBack");
			this._terrain = new BlockTerrain(base.GraphicsDevice, base.Content);
			InventoryItem.Initalize(base.Content);
			BlockEntity.Initialize();
			TracerManager.Initialize();
			string text = "Fonts\\";
			this._consoleFont = base.Content.LoadLocalized(text + "ConsoleFont");
			this._largeFont = base.Content.LoadLocalized(text + "LargeFont");
			this._medFont = base.Content.LoadLocalized(text + "MedFont");
			this._medLargeFont = base.Content.LoadLocalized(text + "MedLargeFont");
			this._smallFont = base.Content.LoadLocalized(text + "SmallFont");
			this._systemFont = base.Content.LoadLocalized(text + "System");
			this._nameTagFont = base.Content.LoadLocalized(text + "NameTagFont");
			this._myriadLarge = base.Content.LoadLocalized(text + "MyriadLarge");
			this._myriadMed = base.Content.LoadLocalized(text + "MyriadMedium");
			this._myriadSmall = base.Content.LoadLocalized(text + "MyriadSmall");
			this._uiSprites = base.Content.Load<SpriteManager>("UI\\SpriteSheet");
			this.DialogScreenImage = base.Content.Load<Texture2D>("UI\\Screens\\DialogBack");
			this.Logo = this._uiSprites["Logo"];
			this.ButtonFrame = new ScalableFrame(this._uiSprites, "CtrlFrame");
			PCDialogScreen.DefaultTitlePadding = new Vector2(55f, 15f);
			PCDialogScreen.DefaultDescriptionPadding = new Vector2(25f, 35f);
			PCDialogScreen.DefaultButtonsPadding = new Vector2(15f, 23f);
			PCDialogScreen.DefaultClickSound = "Click";
			PCDialogScreen.DefaultOpenSound = "Popup";
			ProfilerUtils.SystemFont = this._systemFont;
			EnemyType.Init();
			DragonType.Init();
			FireballEntity.Init();
			DragonClientEntity.Init();
			RocketEntity.Init();
			BlasterShot.Init();
			GrenadeProjectile.Init();
			AvatarAnimationManager.Instance.RegisterAnimation("Swim", base.Content.Load<AnimationClip>("Character\\Animation\\Swim Underwater"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Wave", base.Content.Load<AnimationClip>("Character\\Animation\\Wave"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Run", base.Content.Load<AnimationClip>("Character\\Animation\\Run"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Walk", base.Content.Load<AnimationClip>("Character\\Animation\\Walk"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("Die", base.Content.Load<AnimationClip>("Character\\Animation\\Faint"), false);
			AvatarAnimationManager.Instance.RegisterAnimation("RPGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\RPG\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RPGWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\RPG\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RPGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\RPG\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GunRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\AR\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserGunRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\AR\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserSMGRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\SMG\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserPistolRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Pistol\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserRifleRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Rifle\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserShotgunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Shotgun\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserShotgunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Shotgun\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LaserShotgunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Space\\Shotgun\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("LMGShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\LMG\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PistolShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Pistol\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunRun", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PumpShotgunShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Shotgun\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("RifleShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Rifle\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulder", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Shoulder"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulderWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\ShoulderWalk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGReload", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Reload"), false, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\Shoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulderIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\ShoulderIdle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("SMGShoulderShoot", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\SMG\\Animation\\ShoulderShoot"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GenericIdle", base.Content.Load<AnimationClip>("Character\\Animation\\GenericIdle"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("GenericUse", base.Content.Load<AnimationClip>("Character\\Animation\\GenericUse"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GenericWalk", base.Content.Load<AnimationClip>("Character\\Animation\\GenericWalk"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("FistIdle", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("FistUse", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Use"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("FistWalk", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("PickIdle", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("PickUse", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Use"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("PickWalk", base.Content.Load<AnimationClip>("Props\\Tools\\PickAxe\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("BlockIdle", base.Content.Load<AnimationClip>("Props\\Items\\Block\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("BlockUse", base.Content.Load<AnimationClip>("Props\\Items\\Block\\Animation\\Use"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("BlockWalk", base.Content.Load<AnimationClip>("Props\\Items\\Block\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("Grenade_Reset", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Release"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("Grenade_Throw", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Throw"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("Grenade_Cook", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Cook"), true, new AvatarBone[] { AvatarBone.BackUpper });
			AvatarAnimationManager.Instance.RegisterAnimation("GrenadeIdle", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Idle"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("GrenadeWalk", base.Content.Load<AnimationClip>("Props\\Weapons\\Conventional\\Grenade\\Animation\\Walk"), true, new AvatarBone[] { AvatarBone.CollarRight });
			AvatarAnimationManager.Instance.RegisterAnimation("Stand", base.Content.Load<AnimationClip>("Character\\Animation\\Stand0"), true);
			AvatarAnimationManager.Instance.RegisterAnimation("IdleHead", base.Content.Load<AnimationClip>("Character\\Animation\\MaleIdleLookAround"), true, new AvatarBone[] { AvatarBone.Neck });
			AvatarAnimationManager.Instance.RegisterAnimation("Tilt", base.Content.Load<AnimationClip>("Character\\Animation\\Tilt"), true, new AvatarBone[] { AvatarBone.BackLower }, new AvatarBone[]
			{
				AvatarBone.CollarRight,
				AvatarBone.CollarLeft
			});
			this.FrontEnd = new FrontEndScreen(this);
			this.BeginLoadTerrain(null, true);
			while (!loadScreen.Finished && !this._exitRequested)
			{
				Thread.Sleep(50);
			}
			this.mainScreenGroup.PopScreen();
			this.mainScreenGroup.PushScreen(this.FrontEnd);
			texture2D.Dispose();
			NetworkSession.InviteAccepted += this.NetworkSession_InviteAccepted;
			base.SecondaryLoad();
			this._waitToExit = false;
		}

		private void NetworkSession_InviteAccepted(object sender, InviteAcceptedEventArgs e)
		{
			DNA.Drawing.UI.Screen.SelectedPlayerIndex = new PlayerIndex?(e.Gamer.PlayerIndex);
			if (Guide.IsTrialMode)
			{
				base.ShowMarketPlace();
				return;
			}
			if (base.CurrentNetworkSession != null)
			{
				this.EndGame(true);
			}
			this.FrontEnd.PopToMainMenu(e.Gamer, delegate(bool success)
			{
				if (success)
				{
					WaitScreen.DoWait(this.FrontEnd._uiGroup, Strings.Loading_Player_Info___, delegate
					{
						this.FrontEnd.SetupNewGamer(e.Gamer, this.SaveDevice);
					}, delegate
					{
						TaskDispatcher.Instance.AddTaskForMainThread(delegate
						{
							this.FrontEnd.JoinInvitedGame(e.LobbyId);
						});
					});
				}
			});
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			this._exitRequested = true;
			while (this._waitToExit || this._saving)
			{
				Thread.Sleep(50);
			}
			try
			{
				if (ChunkCache.Instance != null)
				{
					ChunkCache.Instance.Stop(false);
				}
				if (TaskDispatcher.Instance != null)
				{
					TaskDispatcher.Instance.Stop();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			base.OnExiting(sender, args);
		}

		public void BeginLoadTerrain(WorldInfo info, bool host)
		{
			if (info == null)
			{
				this.CurrentWorld = WorldInfo.CreateNewWorld(null);
			}
			else
			{
				this.CurrentWorld = info;
			}
			this._terrain.AsyncInit(this.CurrentWorld, host, delegate(IAsyncResult result)
			{
				this._savingTerrain = false;
			});
		}

		public void WaitForTerrainLoad(ThreadStart callback)
		{
			this._waitForTerrainCallback = callback;
		}

		public void GetWorldInfo(CastleMinerZGame.WorldInfoCallback callback)
		{
			this._waitForWorldInfo = callback;
			RequestWorldInfoMessage.Send(this.MyNetworkGamer);
		}

		public override void StartGamerServices()
		{
			base.Components.Add(new GamerServicesComponent(this, "CastleMinerZ"));
			this.HasGamerServices = true;
		}

		public void HostGame(bool local, SuccessCallback callback)
		{
			NetworkSessionProperties networkSessionProperties = new NetworkSessionProperties();
			networkSessionProperties[0] = new int?(3);
			networkSessionProperties[2] = new int?((int)this.GameMode);
			networkSessionProperties[1] = new int?(0);
			networkSessionProperties[3] = new int?((int)this.Difficulty);
			networkSessionProperties[5] = new int?((int)this.PVPState);
			if (this.InfiniteResourceMode)
			{
				networkSessionProperties[4] = new int?(1);
			}
			else
			{
				networkSessionProperties[4] = new int?(0);
			}
			if (local)
			{
				base.HostGame(NetworkSessionType.Local, networkSessionProperties, new SignedInGamer[] { DNA.Drawing.UI.Screen.CurrentGamer }, 2, false, true, callback, "CastleMinerZSteam", 3, this.CurrentWorld.ServerMessage, null);
				return;
			}
			base.HostGame(NetworkSessionType.PlayerMatch, networkSessionProperties, new SignedInGamer[] { DNA.Drawing.UI.Screen.CurrentGamer }, 16, false, true, callback, "CastleMinerZSteam", 3, this.CurrentWorld.ServerMessage, string.IsNullOrWhiteSpace(this.CurrentWorld.ServerPassword) ? null : this.CurrentWorld.ServerPassword);
		}

		public void GetNetworkSessions(CastleMinerZGame.GotSessionsCallback callback)
		{
			NetworkSessionProperties networkSessionProperties = new NetworkSessionProperties();
			for (int i = 0; i < networkSessionProperties.Count; i++)
			{
				networkSessionProperties[i] = null;
			}
			networkSessionProperties[0] = new int?(3);
			networkSessionProperties[1] = new int?(0);
			QuerySessionInfo querySessionInfo = new QuerySessionInfo();
			querySessionInfo._props = networkSessionProperties;
			NetworkSession.BeginFind(NetworkSessionType.PlayerMatch, new SignedInGamer[] { DNA.Drawing.UI.Screen.CurrentGamer }, querySessionInfo, delegate(IAsyncResult result)
			{
				AvailableNetworkSessionCollection availableNetworkSessionCollection = null;
				try
				{
					availableNetworkSessionCollection = NetworkSession.EndFind(result);
				}
				catch
				{
				}
				try
				{
					CastleMinerZGame.GotSessionsCallback gotSessionsCallback = (CastleMinerZGame.GotSessionsCallback)result.AsyncState;
					if (gotSessionsCallback != null)
					{
						gotSessionsCallback(availableNetworkSessionCollection);
					}
				}
				catch (Exception ex)
				{
					base.CrashGame(ex);
				}
			}, callback);
		}

		public void StartGame()
		{
			if (this.CurrentWorld != null)
			{
				this.ServerMessage = this.CurrentWorld.ServerMessage;
			}
			this.PlayerStats.GamesPlayed++;
			PlayerExistsMessage.Send(this.MyNetworkGamer, this._myAvatarDescription, true);
			this.Difficulty = (GameDifficultyTypes)base.CurrentNetworkSession.SessionProperties[3].Value;
		}

		public void SaveData()
		{
			if (this._saving)
			{
				return;
			}
			CastleMinerZGame.SaveDataInfo saveDataInfo = new CastleMinerZGame.SaveDataInfo();
			if (this.GameScreen == null || this.GameScreen.HUD == null)
			{
				return;
			}
			saveDataInfo.Inventory = this.GameScreen.HUD.PlayerInventory;
			saveDataInfo.Worldinfo = this.CurrentWorld;
			saveDataInfo.PlayerStats = this.PlayerStats;
			this.TaskScheduler.QueueUserWorkItem(new ParameterizedThreadStart(this.SaveDataInternal), saveDataInfo);
		}

		public void SavePlayerStats(CastleMinerZPlayerStats playerStats)
		{
			lock (this.saveLock)
			{
				if (DNA.Drawing.UI.Screen.CurrentGamer != null && !DNA.Drawing.UI.Screen.CurrentGamer.IsGuest)
				{
					this.SaveDevice.Save("stats.sav", true, true, delegate(Stream stream)
					{
						BinaryWriter binaryWriter = new BinaryWriter(stream);
						playerStats.Save(binaryWriter);
						binaryWriter.Flush();
					});
				}
			}
		}

		public void SaveDataInternal(object state)
		{
			CastleMinerZGame.SaveDataInfo saveDataInfo = (CastleMinerZGame.SaveDataInfo)state;
			lock (this.saveLock)
			{
				try
				{
					this._saving = true;
					this.SavePlayerStats(saveDataInfo.PlayerStats);
					if (saveDataInfo.Worldinfo.OwnerGamerTag != null)
					{
						saveDataInfo.Worldinfo.LastPlayedDate = DateTime.Now;
						saveDataInfo.Worldinfo.LastPosition = this.LocalPlayer.LocalPosition;
						saveDataInfo.Worldinfo.SaveToStorage(DNA.Drawing.UI.Screen.CurrentGamer, this.SaveDevice);
					}
					if (!this.LocalPlayer.FinalSaveRegistered)
					{
						if (this.LocalPlayer.Gamer.IsHost)
						{
							this.LocalPlayer.SaveInventory(this.SaveDevice, saveDataInfo.Worldinfo.SavePath);
						}
						else if (base.CurrentNetworkSession == null)
						{
							this.LocalPlayer.SaveInventory(this.SaveDevice, saveDataInfo.Worldinfo.SavePath);
						}
						else
						{
							InventoryStoreOnServerMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, saveDataInfo.Inventory, false);
						}
					}
					if (this.GameMode != GameModeTypes.Endurance)
					{
						ChunkCache.Instance.Flush(true);
					}
					this.SaveDevice.Flush();
				}
				catch
				{
				}
				finally
				{
					this._saving = false;
				}
			}
		}

		public void EndGame(bool saveData)
		{
			if (this.MyNetworkGamer.IsHost && this.IsOnlineGame)
			{
				base.CurrentNetworkSession.CloseNetworkSession();
			}
			if (this.LocalPlayer != null && this.LocalPlayer.UnderwaterCue != null && !this.LocalPlayer.UnderwaterCue.IsPaused)
			{
				this.LocalPlayer.UnderwaterCue.Pause();
			}
			if (this.GameScreen != null && this.GameScreen.HUD != null && this.GameScreen.HUD.ActiveInventoryItem != null)
			{
				this.GameScreen.HUD.ActiveInventoryItem.ItemClass.OnItemUnequipped();
			}
			base.LeaveGame();
			if (saveData && this.LocalPlayer != null)
			{
				this.SaveData();
			}
			if (this.mainScreenGroup.CurrentScreen == this.GameScreen)
			{
				this.mainScreenGroup.PopScreen();
			}
			this.GameScreen = null;
			if (this._terrain.Parent != null)
			{
				this._terrain.RemoveFromParent();
			}
			if (WaterPlane.Instance != null && WaterPlane.Instance.Parent != null)
			{
				WaterPlane.Instance.RemoveFromParent();
			}
			if (DNA.Drawing.UI.Screen.CurrentGamer == null)
			{
				this.FrontEnd.PopToStartScreen();
			}
			else
			{
				this.FrontEnd.PopToMainMenu(DNA.Drawing.UI.Screen.CurrentGamer, null);
			}
			if (this.GameMode == GameModeTypes.Endurance && this.FrontEnd.WorldManager != null)
			{
				this.FrontEnd.WorldManager.Delete(this.CurrentWorld);
				this.SaveDevice.Flush();
			}
			this._waitForTerrainCallback = null;
			this._savingTerrain = true;
			this.BeginLoadTerrain(null, true);
			WaitScreen.DoWait(this.FrontEnd._uiGroup, Strings.Please_Wait___, new ProgressCallback(this.IsSavingProgress));
		}

		public override void OnSessionEnded(NetworkSessionEndReason reason)
		{
			this.EndGame(true);
			this.FrontEnd.ShowUIDialog(Strings.Session_Ended, Strings.You_have_been_disconnected_from_the_network_session_, false);
			base.OnSessionEnded(reason);
		}

		private bool IsSavingProgress()
		{
			return !this._savingTerrain;
		}

		protected override void Update(GameTime gameTime)
		{
			if (CastleMinerZGame.TrialMode)
			{
				this.TrialModeTimer.Update(gameTime.ElapsedGameTime);
				if (this.TrialModeTimer.Expired)
				{
					Process.Start("http://www.digitaldnagames.com/upsell/castleminerz.aspx");
					base.Exit();
				}
			}
			if (this.CurrentWorld != null)
			{
				this.CurrentWorld.Update(gameTime);
			}
			this.UpdateMusic(gameTime);
			if (this._terrain != null)
			{
				this._terrain.GlobalUpdate(gameTime);
				if (this._terrain.MinimallyLoaded && this._waitForTerrainCallback != null)
				{
					this._waitForTerrainCallback();
					this._waitForTerrainCallback = null;
				}
			}
			if (this.PlayerStats != null)
			{
				this.PlayerStats.TimeInFull += gameTime.ElapsedGameTime;
				this.PlayerStats.TimeOfPurchase = DateTime.UtcNow;
				if (this.FrontEnd != null && this.mainScreenGroup.CurrentScreen == this.FrontEnd)
				{
					this.PlayerStats.TimeInMenu += gameTime.ElapsedGameTime;
				}
				if (base.CurrentNetworkSession != null && base.CurrentNetworkSession.SessionType == NetworkSessionType.PlayerMatch && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
				{
					this.PlayerStats.TimeOnline += gameTime.ElapsedGameTime;
				}
			}
			if (this.RequestEndGame)
			{
				this.RequestEndGame = false;
				this.EndGame(true);
			}
			TaskDispatcher.Instance.RunMainThreadTasks();
			base.Update(gameTime);
		}

		public void CheaterFound()
		{
			this.SaveDevice.DeleteStorage();
			base.Exit();
		}

		protected override void SendNetworkUpdates(NetworkSession session, GameTime gameTime)
		{
			if (session == null || session.LocalGamers.Count == 0)
			{
				return;
			}
			if (session.LocalGamers[0].IsHost && this.GameScreen != null)
			{
				this._worldUpdateTimer.Update(gameTime.ElapsedGameTime);
				if (this._worldUpdateTimer.Expired)
				{
					TimeOfDayMessage.Send(session.LocalGamers[0], this.GameScreen.Day);
					this._worldUpdateTimer.Reset();
				}
			}
			this._currentFrameNumber++;
			if (this._currentFrameNumber > session.AllGamers.Count)
			{
				this._currentFrameNumber = 0;
			}
			if (this._localPlayer != null)
			{
				for (int i = 0; i < session.AllGamers.Count; i++)
				{
					if (session.AllGamers[i].IsLocal && i != this._currentFrameNumber && !this._localPlayer.UsingTool && !this._localPlayer.Reloading)
					{
						return;
					}
				}
				if (session.LocalGamers.Count > 0)
				{
					PlayerUpdateMessage.Send(session.LocalGamers[0], this._localPlayer, this._controllerMapping);
				}
			}
			MainThreadMessageSender.Instance.DrainQueue();
		}

		public void SetupNewGamer(SignedInGamer gamer)
		{
			this.PlayerStats = new CastleMinerZPlayerStats();
			this.PlayerStats.GamerTag = gamer.Gamertag;
			this.PlayerStats.InvertYAxis = gamer.GameDefaults.InvertYAxis;
			this.PlayerStats.SecondTrayFaded = false;
			this.LoadPlayerData();
			this.Brightness = this.PlayerStats.brightness;
			if (this.PlayerStats.musicMute)
			{
				this.MusicSounds.SetVolume(0f);
			}
			else
			{
				this.MusicSounds.SetVolume(this.PlayerStats.musicVolume);
			}
			this.AcheivmentManager = new CastleMinerZAchievementManager(this);
			this._myAvatarDescription = new AvatarDescription(new byte[10]);
		}

		public void MakeAboveGround(bool spawnontop)
		{
			if (spawnontop)
			{
				this._localPlayer.LocalPosition = this._terrain.FindTopmostGroundLocation(this._localPlayer.LocalPosition);
				return;
			}
			this._localPlayer.LocalPosition = this._terrain.FindSafeStartLocation(this._localPlayer.LocalPosition);
		}

		public void PlayMusic(string cueName)
		{
			this._fadeMusic = false;
			if (this.MusicCue != null && this.MusicCue.IsPlaying && this.MusicCue.Name != cueName)
			{
				this.MusicCue.Stop(AudioStopOptions.Immediate);
				this.MusicCue = null;
			}
			if (this.MusicCue == null || !this.MusicCue.IsPlaying)
			{
				this.MusicCue = SoundManager.Instance.PlayInstance(cueName);
			}
			if (this.PlayerStats.musicMute)
			{
				this.MusicSounds.SetVolume(0f);
				return;
			}
			this.MusicSounds.SetVolume(this.PlayerStats.musicVolume);
		}

		public void FadeMusic()
		{
			this._fadeMusic = true;
			this.musicFadeTimer.Reset();
		}

		public void SetAudio(float day, float night, float cave, float hell)
		{
			if (this.DayCue == null)
			{
				this.DayCue = SoundManager.Instance.PlayInstance("Birds");
			}
			if (this.NightCue == null)
			{
				this.NightCue = SoundManager.Instance.PlayInstance("Crickets");
			}
			if (this.CaveCue == null)
			{
				this.CaveCue = SoundManager.Instance.PlayInstance("Drips");
			}
			if (this.HellCue == null)
			{
				this.HellCue = SoundManager.Instance.PlayInstance("lostSouls");
			}
			if (this.LocalPlayer != null && this.LocalPlayer.Underwater)
			{
				day = 0f;
				night = 0f;
				cave = 0f;
				hell = 0f;
			}
			this.DaySounds.SetVolume(day);
			this.NightSounds.SetVolume(night);
			this.CaveSounds.SetVolume(cave);
			this.HellSounds.SetVolume(hell);
			SoundManager.Instance.SetGlobalVarible("Outdoors", 1f - Math.Max(cave, hell));
		}

		protected override void AfterLoad()
		{
			InventoryItem.FinishInitialization(base.GraphicsDevice);
			base.AfterLoad();
		}

		public void UpdateMusic(GameTime time)
		{
			if (this._fadeMusic && this.MusicCue.IsPlaying)
			{
				this.musicFadeTimer.Update(time.ElapsedGameTime);
				if (this.musicFadeTimer.Expired)
				{
					if (this.MusicCue.IsPlaying)
					{
						this.MusicCue.Stop(AudioStopOptions.Immediate);
						return;
					}
				}
				else
				{
					float num = this.PlayerStats.musicVolume - this.musicFadeTimer.PercentComplete;
					if (num < 0f)
					{
						num = 0f;
					}
					if (this.PlayerStats.musicMute)
					{
						this.MusicSounds.SetVolume(0f);
						return;
					}
					this.MusicSounds.SetVolume(num);
				}
			}
		}

		public NetworkGamer GetGamerFromID(byte id)
		{
			return base.CurrentNetworkSession.FindGamerById(id);
		}

		private void ProcessUpdateSpawnerMessage(DNA.Net.Message message)
		{
			UpdateSpawnerMessage updateSpawnerMessage = (UpdateSpawnerMessage)message;
			if (updateSpawnerMessage.IsStarted)
			{
				CastleMinerZGame.Instance.CurrentWorld.GetSpawner(IntVector3.FromVector3(updateSpawnerMessage.SpawnerPosition), true, BlockTypeEnum.Empty);
				return;
			}
			Spawner spawner = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(IntVector3.FromVector3(updateSpawnerMessage.SpawnerPosition), false, BlockTypeEnum.Empty);
			spawner.HandleStopSpawningMessage();
		}

		private void ProcessDestroyCustomBlockMessage(DNA.Net.Message message)
		{
			DestroyCustomBlockMessage destroyCustomBlockMessage = (DestroyCustomBlockMessage)message;
			Door door;
			if (this.CurrentWorld.Doors.TryGetValue(destroyCustomBlockMessage.Location, out door))
			{
				door.Destroyed = true;
				this.CurrentWorld.Doors.Remove(destroyCustomBlockMessage.Location);
			}
		}

		private void ProcessMeleePlayerMessage(DNA.Net.Message message)
		{
			if (this.PVPState == CastleMinerZGame.PVPEnum.Everyone || (!this.MyNetworkGamer.IsHost && !this.MyNetworkGamer.SignedInGamer.IsFriend(base.CurrentNetworkSession.Host)))
			{
				MeleePlayerMessage meleePlayerMessage = (MeleePlayerMessage)message;
				float num = 0.21f;
				if (meleePlayerMessage.ItemID == InventoryItemIDs.IronLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.CopperLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.GoldLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.DiamondLaserSword || meleePlayerMessage.ItemID == InventoryItemIDs.BloodStoneLaserSword)
				{
					num = 1.1f;
				}
				this.GameScreen.HUD.ApplyDamage(num, meleePlayerMessage.DamageSource);
			}
		}

		private void _processAddExplosiveFlashMessage(DNA.Net.Message message)
		{
			AddExplosiveFlashMessage addExplosiveFlashMessage = (AddExplosiveFlashMessage)message;
			if (this.GameScreen != null)
			{
				this.GameScreen.AddExplosiveFlashModel(addExplosiveFlashMessage.Position);
			}
		}

		private void _processAddExplosionEffectsMessage(DNA.Net.Message message)
		{
			AddExplosionEffectsMessage addExplosionEffectsMessage = (AddExplosionEffectsMessage)message;
			Explosive.AddEffects(addExplosionEffectsMessage.Position, true);
		}

		private void _processKickMessage(DNA.Net.Message message, LocalNetworkGamer localGamer)
		{
		}

		private void _processRequestWorldInfoMessage(DNA.Net.Message message, LocalNetworkGamer localGamer, bool isEcho)
		{
			if (localGamer.IsHost && !isEcho)
			{
				if (this.PlayerStats.BanList.ContainsKey(message.Sender.AlternateAddress))
				{
					KickMessage.Send(localGamer, message.Sender, true);
					return;
				}
				WorldInfoMessage.Send(localGamer, this.CurrentWorld);
			}
		}

		private void _processClientReadyForChunkMessage(DNA.Net.Message message, bool isEcho)
		{
			byte id = this.MyNetworkGamer.Id;
			if (id == this.TerrainServerID && !isEcho)
			{
				ChunkCache.Instance.SendRemoteChunkList(message.Sender.Id, false);
			}
		}

		private void _processProvideDeltaListMessage(DNA.Net.Message message)
		{
			ChunkCache.Instance.RemoteChunkListArrived(((ProvideDeltaListMessage)message).Delta);
		}

		private void _processAlterBlocksMessage(DNA.Net.Message message)
		{
			AlterBlockMessage alterBlockMessage = (AlterBlockMessage)message;
			this._terrain.SetBlock(alterBlockMessage.BlockLocation, alterBlockMessage.BlockType);
		}

		private void _processRequestChunkMessage(DNA.Net.Message message)
		{
			RequestChunkMessage requestChunkMessage = (RequestChunkMessage)message;
			ChunkCache.Instance.RetrieveChunkForNetwork(requestChunkMessage.Sender.Id, requestChunkMessage.BlockLocation, requestChunkMessage.Priority, null);
		}

		private void _processProvideChunkMessage(DNA.Net.Message message)
		{
			ProvideChunkMessage provideChunkMessage = (ProvideChunkMessage)message;
			ChunkCache.Instance.ChunkDeltaArrived(provideChunkMessage.BlockLocation, provideChunkMessage.Delta, provideChunkMessage.Priority);
		}

		private void _processWorldInfoMessage(DNA.Net.Message message)
		{
			WorldInfoMessage worldInfoMessage = (WorldInfoMessage)message;
			WorldInfo worldInfo = worldInfoMessage.WorldInfo;
			if (this._waitForWorldInfo != null)
			{
				this._waitForWorldInfo(worldInfo);
				this._waitForWorldInfo = null;
			}
		}

		private void _processTimeOfDayMessage(DNA.Net.Message message, bool isEcho)
		{
			if (!isEcho)
			{
				TimeOfDayMessage timeOfDayMessage = (TimeOfDayMessage)message;
				if (this.GameScreen != null)
				{
					this.GameScreen.Day = timeOfDayMessage.TimeOfDay;
				}
			}
		}

		private void _processBroadcastTextMessage(DNA.Net.Message message)
		{
			BroadcastTextMessage broadcastTextMessage = (BroadcastTextMessage)message;
			Console.WriteLine(broadcastTextMessage.Message);
		}

		private void _processItemCrateMessage(DNA.Net.Message message)
		{
			ItemCrateMessage itemCrateMessage = (ItemCrateMessage)message;
			itemCrateMessage.Apply(this.CurrentWorld);
		}

		private void _processDestroyCrateMessage(DNA.Net.Message message)
		{
			DestroyCrateMessage destroyCrateMessage = (DestroyCrateMessage)message;
			Crate crate;
			if (this.CurrentWorld.Crates.TryGetValue(destroyCrateMessage.Location, out crate))
			{
				crate.Destroyed = true;
				this.CurrentWorld.Crates.Remove(destroyCrateMessage.Location);
			}
		}

		private void _processDoorOpenCloseMessage(DNA.Net.Message message)
		{
			DoorOpenCloseMessage doorOpenCloseMessage = (DoorOpenCloseMessage)message;
			AudioEmitter audioEmitter = new AudioEmitter();
			audioEmitter.Position = doorOpenCloseMessage.Location;
			if (doorOpenCloseMessage.Opened)
			{
				SoundManager.Instance.PlayInstance("DoorOpen", audioEmitter);
			}
			else
			{
				SoundManager.Instance.PlayInstance("DoorClose", audioEmitter);
			}
			Door door;
			if (this.CurrentWorld.Doors.TryGetValue(doorOpenCloseMessage.Location, out door))
			{
				door.Open = doorOpenCloseMessage.Opened;
			}
		}

		private void _processAppointServerMessage(DNA.Net.Message message)
		{
			byte id = this.MyNetworkGamer.Id;
			AppointServerMessage appointServerMessage = (AppointServerMessage)message;
			NetworkGamer gamerFromID = this.GetGamerFromID(appointServerMessage.PlayerID);
			if (appointServerMessage.PlayerID == id)
			{
				ChunkCache.Instance.MakeHost(null, true);
			}
			else if (this.TerrainServerID == id)
			{
				ChunkCache.Instance.MakeHost(null, false);
			}
			else if (appointServerMessage.PlayerID != this.TerrainServerID)
			{
				ChunkCache.Instance.HostChanged();
			}
			this.TerrainServerID = appointServerMessage.PlayerID;
		}

		private void _processRestartLevelMessage(DNA.Net.Message message)
		{
			if (this.GameScreen != null)
			{
				this.LocalPlayer.Dead = false;
				this.LocalPlayer.FPSMode = true;
				this.GameScreen.HUD.RefreshPlayer();
				this.GameScreen.TeleportToLocation(WorldInfo.DefaultStartLocation, true);
				if (this.MusicCue != null && this.MusicCue.IsPlaying)
				{
					this.MusicCue.Stop(AudioStopOptions.Immediate);
				}
				InGameHUD.Instance.Reset();
				CastleMinerZGame.Instance.GameScreen.Day = 0.4f;
				InGameHUD.Instance.maxDistanceTraveled = 0;
			}
		}

		private void _processInventoryStoreOnServerMessage(DNA.Net.Message message, bool isHost)
		{
			if (isHost)
			{
				InventoryStoreOnServerMessage inventoryStoreOnServerMessage = (InventoryStoreOnServerMessage)message;
				Player player = (Player)inventoryStoreOnServerMessage.Sender.Tag;
				if (player != this._localPlayer)
				{
					player.PlayerInventory = inventoryStoreOnServerMessage.Inventory;
				}
				if (inventoryStoreOnServerMessage.FinalSave)
				{
					player.FinalSaveRegistered = true;
				}
				this.TaskScheduler.QueueUserWorkItem(delegate(object state)
				{
					Player player2 = (Player)state;
					player2.SaveInventory(this.SaveDevice, this.CurrentWorld.SavePath);
				}, player);
			}
		}

		private void _processInventoryRetrieveFromServerMessage(DNA.Net.Message message, bool isHost)
		{
			InventoryRetrieveFromServerMessage inventoryRetrieveFromServerMessage = (InventoryRetrieveFromServerMessage)message;
			NetworkGamer gamerFromID = this.GetGamerFromID(inventoryRetrieveFromServerMessage.playerID);
			if (gamerFromID != null && gamerFromID.Tag != null)
			{
				Player player = (Player)gamerFromID.Tag;
				player.PlayerInventory = inventoryRetrieveFromServerMessage.Inventory;
				player.PlayerInventory.Player = player;
			}
		}

		private void _processRequestInventoryMessage(DNA.Net.Message message, bool isHost)
		{
			if (isHost && message.Sender.Tag != null)
			{
				this.TaskScheduler.QueueUserWorkItem(delegate(object state)
				{
					Player player = (Player)state;
					bool flag = player.LoadInventory(this.SaveDevice, this.CurrentWorld.SavePath);
					InventoryRetrieveFromServerMessage.Send((LocalNetworkGamer)this._localPlayer.Gamer, player, flag);
				}, message.Sender.Tag);
			}
		}

		private void _processPlayerExistsMessage(DNA.Net.Message message, bool isEcho, bool isHost)
		{
			PlayerExistsMessage playerExistsMessage = (PlayerExistsMessage)message;
			if (message.Sender.Tag == null)
			{
				Player player = new Player(message.Sender, new AvatarDescription(playerExistsMessage.AvatarDescriptionData));
				if (isEcho)
				{
					this._localPlayer = player;
					this.GameScreen = new GameScreen(this, player);
					this.GameScreen.Inialize();
					this.mainScreenGroup.PushScreen(this.GameScreen);
					this._localPlayer.LocalPosition = this.CurrentWorld.LastPosition;
					this.CurrentWorld.InfiniteResourceMode = this.InfiniteResourceMode;
					RequestInventoryMessage.Send((LocalNetworkGamer)this._localPlayer.Gamer);
					if (this._localPlayer.LocalPosition == Vector3.Zero)
					{
						this._localPlayer.LocalPosition = new Vector3(3f, 3f, 3f);
						this.MakeAboveGround(true);
					}
					else
					{
						this.MakeAboveGround(false);
					}
					this.FadeMusic();
					lock (this.holdingGround)
					{
						while (this.holdingGround.Children.Count > 0)
						{
							Entity entity2 = this.holdingGround.Children[0];
							entity2.RemoveFromParent();
							this.GameScreen.AddPlayer((Player)entity2);
						}
						this.holdingGround.Children.Clear();
						goto IL_024B;
					}
				}
				if (this.GameScreen == null)
				{
					lock (this.holdingGround)
					{
						this.holdingGround.Children.Add(player);
						goto IL_024B;
					}
				}
				this.GameScreen.AddPlayer(player);
				if (playerExistsMessage.RequestResponse)
				{
					PlayerExistsMessage.Send(this.MyNetworkGamer, this._myAvatarDescription, false);
					if (isHost)
					{
						TimeOfDayMessage.Send(this.MyNetworkGamer, this.GameScreen.Day);
					}
					TimeConnectedMessage.Send(this.MyNetworkGamer, this.LocalPlayer);
				}
				ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this.GameScreen.HUD.ActiveInventoryItem.ItemClass.ID);
				CrateFocusMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this._localPlayer.FocusCrate, this._localPlayer.FocusCrateItem);
				IL_024B:
				if (EnemyManager.Instance != null)
				{
					EnemyManager.Instance.BroadcastExistingDragonMessage(message.Sender.Id);
				}
			}
		}

		protected override void OnMessage(DNA.Net.Message message)
		{
			LocalNetworkGamer myNetworkGamer = this.MyNetworkGamer;
			bool isHost = myNetworkGamer.IsHost;
			bool flag = message.Sender == myNetworkGamer;
			if (3 != base.CurrentNetworkSession.SessionProperties[0].Value)
			{
				this.EndGame(false);
				this.FrontEnd.ShowUIDialog(Strings.Session_Ended, Strings.You_have_a_different_version_of_the_game_than_the_host_, false);
				return;
			}
			if (message is PlayerExistsMessage)
			{
				this._processPlayerExistsMessage(message, flag, isHost);
			}
			else if (message is AddExplosiveFlashMessage)
			{
				this._processAddExplosiveFlashMessage(message);
			}
			else if (message is AddExplosionEffectsMessage)
			{
				this._processAddExplosionEffectsMessage(message);
			}
			else if (message is KickMessage)
			{
				this._processKickMessage(message, myNetworkGamer);
			}
			else if (message is RequestWorldInfoMessage)
			{
				this._processRequestWorldInfoMessage(message, myNetworkGamer, flag);
			}
			else if (message is ClientReadyForChunksMessage)
			{
				this._processClientReadyForChunkMessage(message, flag);
			}
			else if (message is ProvideDeltaListMessage && !isHost)
			{
				this._processProvideDeltaListMessage(message);
			}
			else if (message is AlterBlockMessage)
			{
				this._processAlterBlocksMessage(message);
			}
			else if (message is RequestChunkMessage && isHost)
			{
				this._processRequestChunkMessage(message);
			}
			else if (message is ProvideChunkMessage && !isHost)
			{
				this._processProvideChunkMessage(message);
			}
			else if (message is WorldInfoMessage)
			{
				this._processWorldInfoMessage(message);
			}
			else if (message is TimeOfDayMessage)
			{
				this._processTimeOfDayMessage(message, flag);
			}
			else if (message is BroadcastTextMessage)
			{
				this._processBroadcastTextMessage(message);
			}
			else if (message is ItemCrateMessage)
			{
				this._processItemCrateMessage(message);
			}
			else if (message is DestroyCrateMessage)
			{
				this._processDestroyCrateMessage(message);
			}
			else if (message is DoorOpenCloseMessage)
			{
				this._processDoorOpenCloseMessage(message);
			}
			else if (message is AppointServerMessage)
			{
				this._processAppointServerMessage(message);
			}
			else if (message is RestartLevelMessage)
			{
				this._processRestartLevelMessage(message);
			}
			else if (message is InventoryStoreOnServerMessage)
			{
				this._processInventoryStoreOnServerMessage(message, isHost);
			}
			else if (message is InventoryRetrieveFromServerMessage)
			{
				this._processInventoryRetrieveFromServerMessage(message, isHost);
			}
			else if (message is RequestInventoryMessage)
			{
				this._processRequestInventoryMessage(message, isHost);
			}
			else if (message is DetonateRocketMessage)
			{
				Explosive.HandleDetonateRocketMessage(message as DetonateRocketMessage);
			}
			else if (message is DetonateGrenadeMessage)
			{
				GrenadeProjectile.HandleDetonateGrenadeMessage(message as DetonateGrenadeMessage);
			}
			else if (message is DetonateExplosiveMessage)
			{
				Explosive.HandleDetonateExplosiveMessage((DetonateExplosiveMessage)message);
			}
			else if (message is RemoveBlocksMessage)
			{
				Explosive.HandleRemoveBlocksMessage((RemoveBlocksMessage)message);
			}
			else if (message is MeleePlayerMessage)
			{
				this.ProcessMeleePlayerMessage(message);
			}
			else if (message is DestroyCustomBlockMessage)
			{
				this.ProcessDestroyCustomBlockMessage(message);
			}
			if (message is CastleMinerZMessage)
			{
				CastleMinerZMessage castleMinerZMessage = (CastleMinerZMessage)message;
				switch (castleMinerZMessage.MessageType)
				{
				case CastleMinerZMessage.MessageTypes.Broadcast:
				{
					for (int i = 0; i < base.CurrentNetworkSession.AllGamers.Count; i++)
					{
						NetworkGamer networkGamer = base.CurrentNetworkSession.AllGamers[i];
						if (networkGamer.Tag != null)
						{
							Player player = (Player)networkGamer.Tag;
							player.ProcessMessage(message);
						}
					}
					break;
				}
				case CastleMinerZMessage.MessageTypes.PlayerUpdate:
					if (message.Sender.Tag != null)
					{
						Player player2 = (Player)message.Sender.Tag;
						player2.ProcessMessage(message);
					}
					break;
				case CastleMinerZMessage.MessageTypes.EnemyMessage:
					if (EnemyManager.Instance != null)
					{
						EnemyManager.Instance.HandleMessage(castleMinerZMessage);
					}
					break;
				case CastleMinerZMessage.MessageTypes.PickupMessage:
					if (PickupManager.Instance != null)
					{
						PickupManager.Instance.HandleMessage(castleMinerZMessage);
					}
					break;
				}
			}
			base.OnMessage(message);
		}

		protected override void OnGamerJoined(NetworkGamer gamer)
		{
			LocalNetworkGamer localNetworkGamer = base.CurrentNetworkSession.LocalGamers[0];
			Console.WriteLine(Strings.Player_Joined + ": " + gamer.Gamertag);
			if (gamer == localNetworkGamer)
			{
				this.MyNetworkGamer = localNetworkGamer;
				if (!localNetworkGamer.IsHost)
				{
					this.GameMode = (GameModeTypes)base.CurrentNetworkSession.SessionProperties[2].Value;
					int? num = base.CurrentNetworkSession.SessionProperties[4];
					int num2 = 1;
					if ((num.GetValueOrDefault() == num2) & (num != null))
					{
						this.InfiniteResourceMode = true;
					}
					else
					{
						this.InfiniteResourceMode = true;
					}
				}
				else
				{
					base.CurrentNetworkSession.Password = this.CurrentWorld.ServerPassword;
				}
			}
			else if (localNetworkGamer.IsHost)
			{
				AppointServerMessage.Send(this.MyNetworkGamer, this.TerrainServerID);
				if (this.IsOnlineGame)
				{
					base.CurrentNetworkSession.ReportClientJoined(gamer.Gamertag);
				}
			}
			base.OnGamerJoined(gamer);
		}

		public override void OnHostChanged(NetworkGamer oldHost, NetworkGamer newHost)
		{
			if (newHost != null)
			{
				this.MyNetworkGamer = base.CurrentNetworkSession.LocalGamers[0];
				if (newHost == this.MyNetworkGamer)
				{
					this.AppointNewServer();
				}
			}
			base.OnHostChanged(oldHost, newHost);
		}

		private void AppointNewServer()
		{
			TimeSpan timeSpan = TimeSpan.Zero;
			byte b = 0;
			bool flag = false;
			foreach (NetworkGamer networkGamer in base.CurrentNetworkSession.AllGamers)
			{
				if (networkGamer.Tag != null)
				{
					Player player = (Player)networkGamer.Tag;
					if (player.TimeConnected >= timeSpan)
					{
						timeSpan = player.TimeConnected;
						b = networkGamer.Id;
						flag = true;
					}
				}
			}
			if (flag)
			{
				if (b != this.TerrainServerID)
				{
					AppointServerMessage.Send(this.MyNetworkGamer, b);
					return;
				}
			}
			else
			{
				base.CurrentNetworkSession.AllowHostMigration = false;
				base.CurrentNetworkSession.AllowJoinInProgress = false;
				this.EndGame(false);
				this.FrontEnd.ShowUIDialog(Strings.Session_Ended, Strings.You_have_been_disconnected_from_the_network_session_, false);
			}
		}

		protected override void OnGamerLeft(NetworkGamer gamer)
		{
			if (base.CurrentNetworkSession == null || base.CurrentNetworkSession.LocalGamers.Count == 0)
			{
				return;
			}
			NetworkGamer myNetworkGamer = this.MyNetworkGamer;
			Console.WriteLine(Strings.Player_Left + ": " + gamer.Gamertag);
			if (gamer != myNetworkGamer && myNetworkGamer.IsHost && this.TerrainServerID == gamer.Id)
			{
				this.AppointNewServer();
			}
			if (gamer != myNetworkGamer && myNetworkGamer.IsHost)
			{
				if (this.IsOnlineGame)
				{
					base.CurrentNetworkSession.ReportClientLeft(gamer.Gamertag);
				}
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					if (!player.FinalSaveRegistered)
					{
						this.TaskScheduler.QueueUserWorkItem(delegate(object state)
						{
							player.SaveInventory(this.SaveDevice, this.CurrentWorld.SavePath);
						}, player);
					}
				}
			}
			if (gamer.Tag != null)
			{
				Player player2 = (Player)gamer.Tag;
				player2.RemoveFromParent();
			}
			base.OnGamerLeft(gamer);
		}

		public void LoadPlayerData()
		{
			CastleMinerZPlayerStats stats = new CastleMinerZPlayerStats();
			stats.GamerTag = DNA.Drawing.UI.Screen.CurrentGamer.Gamertag;
			try
			{
				this.SaveDevice.Load("stats.sav", delegate(Stream stream)
				{
					stats.Load(new BinaryReader(stream));
				});
				if (stats.GamerTag != DNA.Drawing.UI.Screen.CurrentGamer.Gamertag)
				{
					throw new Exception("Stats Error");
				}
				this.PlayerStats = stats;
			}
			catch (Exception)
			{
				this.PlayerStats = new CastleMinerZPlayerStats();
				this.PlayerStats.GamerTag = DNA.Drawing.UI.Screen.CurrentGamer.Gamertag;
				if (GraphicsProfileManager.Instance.IsReach)
				{
					this.PlayerStats.DrawDistance = 0;
				}
			}
		}

		protected override void EndDraw()
		{
			if (this._terrain != null)
			{
				this._terrain.BuildPendingVertexBuffers();
			}
			base.EndDraw();
		}

		public const int NetworkVersion = 3;

		public const string NetworkGameName = "CastleMinerZSteam";

		private static Version GameVersion = Assembly.GetExecutingAssembly().GetName().Version;

		public static CastleMinerZGame Instance;

		private Player _localPlayer;

		public AudioListener Listener = new AudioListener();

		public BlockTerrain _terrain;

		public SpriteFont _nameTagFont;

		public SpriteFont _largeFont;

		public SpriteFont _medFont;

		public SpriteFont _medLargeFont;

		public SpriteFont _smallFont;

		public SpriteFont _systemFont;

		public SpriteFont _consoleFont;

		public SpriteFont _myriadLarge;

		public SpriteFont _myriadMed;

		public SpriteFont _myriadSmall;

		public CastleMinerZPlayerStats PlayerStats = new CastleMinerZPlayerStats();

		public CastleMinerZAchievementManager AcheivmentManager;

		private AvatarDescription _myAvatarDescription;

		public SpriteManager _uiSprites;

		public ScalableFrame ButtonFrame;

		public CastleMinerZControllerMapping _controllerMapping = new CastleMinerZControllerMapping();

		public WorldInfo CurrentWorld;

		public FrontEndScreen FrontEnd;

		public GameScreen GameScreen;

		public Texture2D DialogScreenImage;

		public Texture2D MenuBackdrop;

		public byte TerrainServerID;

		public bool DrawingReflection;

		public LocalNetworkGamer MyNetworkGamer;

		public SaveDevice SaveDevice;

		public GameModeTypes GameMode;

		public bool InfiniteResourceMode;

		public GameDifficultyTypes Difficulty;

		public JoinGamePolicy _joinGamePolicy;

		public bool RequestEndGame;

		public static GlobalSettings GlobalSettings = new GlobalSettings();

		public static bool TrialMode = true;

		public OneShotTimer TrialModeTimer = new OneShotTimer(TimeSpan.FromMinutes(8.0));

		public static string FacebookAccessToken;

		public AudioCategory MusicSounds;

		public AudioCategory DaySounds;

		public AudioCategory NightSounds;

		public AudioCategory CaveSounds;

		public AudioCategory HellSounds;

		public Cue MusicCue;

		public Cue DayCue;

		public Cue NightCue;

		public Cue CaveCue;

		public Cue HellCue;

		public Sprite Logo;

		private bool _waitToExit = true;

		private bool _exitRequested;

		public ScreenGroup mainScreenGroup = new ScreenGroup(false);

		public ScreenGroup overlayScreenGroup = new ScreenGroup(true);

		private ThreadStart _waitForTerrainCallback;

		private CastleMinerZGame.WorldInfoCallback _waitForWorldInfo;

		private bool _saving;

		private object saveLock = new object();

		public static readonly int[] SaveProcessorAffinity = new int[] { 5 };

		private bool _savingTerrain;

		private int _currentFrameNumber;

		private OneShotTimer _worldUpdateTimer = new OneShotTimer(TimeSpan.FromSeconds(5.0));

		private bool _fadeMusic;

		private OneShotTimer musicFadeTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		private Entity holdingGround = new Entity();

		public enum NetworkProps
		{
			Version,
			JoinGame,
			GameMode,
			Difficulty,
			InfiniteResources,
			PVP,
			COUNT
		}

		public enum PVPEnum
		{
			Off,
			Everyone,
			NotFriends
		}

		public delegate void WorldInfoCallback(WorldInfo info);

		public delegate void GotSessionsCallback(AvailableNetworkSessionCollection sessions);

		private class SaveDataInfo
		{
			public WorldInfo Worldinfo;

			public PlayerInventory Inventory;

			public CastleMinerZPlayerStats PlayerStats;
		}

		private struct InventoryFromMessage
		{
			public InventoryFromMessage(PlayerInventory inventory, bool isDefault)
			{
				this.Inventory = inventory;
				this.IsDefault = isDefault;
			}

			public PlayerInventory Inventory;

			public bool IsDefault;
		}
	}
}
