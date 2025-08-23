using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Drawing.UI;
using DNA.Net.GamerServices;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class GameScreen : ScreenGroup
	{
		public void AddExplosiveFlashModel(IntVector3 position)
		{
			ExplosiveFlashEntity flash = new ExplosiveFlashEntity(position);
			if (!this._explosiveFlashEntities.Contains(flash))
			{
				this._explosiveFlashEntities.Add(flash);
				this.mainScene.Children.Add(flash);
			}
		}

		public void RemoveExplosiveFlashModel(IntVector3 position)
		{
			for (int i = 0; i < this._explosiveFlashEntities.Count; i++)
			{
				if (this._explosiveFlashEntities[i].BlockPosition == position)
				{
					this._explosiveFlashEntities[i].RemoveFromParent();
					this._explosiveFlashEntities.RemoveAt(i);
					return;
				}
			}
		}

		public InGameHUD HUD
		{
			get
			{
				return this._inGameUI;
			}
		}

		public bool IsBlockPickerUp
		{
			get
			{
				return this._uiGroup.Contains(this._blockCraftingScreen);
			}
		}

		public float TimeOfDay
		{
			get
			{
				return this._sky.TimeOfDay;
			}
		}

		public float DayNightBlender
		{
			get
			{
				float hourf = this._sky.TimeOfDay * 24f;
				float blender = (hourf - (float)((int)hourf)) / 2f;
				int hour = (int)hourf;
				if (hour <= 5 || hour >= 21)
				{
					return 1f;
				}
				if (hour >= 9 && hour <= 17)
				{
					return 0f;
				}
				if (hour == 7 || hour == 19)
				{
					return 0.5f;
				}
				if (hour == 6)
				{
					return 1f - blender;
				}
				if (hour == 8)
				{
					return 0.5f - blender;
				}
				if (hour == 18)
				{
					return blender;
				}
				return 0.5f + blender;
			}
		}

		public float Day
		{
			get
			{
				return this._sky.Day;
			}
			set
			{
				this._sky.Day = value;
				float dayT = value + 1.625f;
				dayT -= (float)Math.Floor((double)dayT);
				float angle = dayT * 6.2831855f;
				float elev = 0.2f + 0.8f * (float)Math.Abs(Math.Sin((double)angle));
				float offset = (float)Math.Sqrt((double)(1f - elev * elev));
				angle -= 0.236f;
				float offx = -(float)Math.Sin((double)angle) * offset;
				float offz = (float)Math.Cos((double)angle) * offset;
				Vector3 vecToSun = new Vector3(offx, elev, offz);
				this._terrain.VectorToSun = vecToSun;
				float hourf = this._sky.TimeOfDay * 24f;
				float blender = hourf - (float)((int)hourf);
				int hour = (int)hourf;
				float percentMidnight = (this._sky.TimeOfDay + 0.96f + 0.5f) % 1f * 2f;
				if (percentMidnight > 1f)
				{
					percentMidnight = 2f - percentMidnight;
				}
				float dayAmount;
				float nightAmount;
				GameScreen.LightColorPack finalColors;
				if (hour <= 5 || hour >= 21)
				{
					dayAmount = 0f;
					nightAmount = 1f;
					finalColors = this.nightColors;
				}
				else if (hour >= 9 && hour <= 17)
				{
					dayAmount = 1f;
					nightAmount = 0f;
					finalColors = this.dayColors;
				}
				else if (hour == 6)
				{
					dayAmount = 0f;
					nightAmount = 1f - blender;
					finalColors = new GameScreen.LightColorPack(blender, ref this.nightColors, ref this.dawnColors);
				}
				else if (hour == 7)
				{
					dayAmount = 0.5f;
					nightAmount = 0.5f;
					finalColors = this.dawnColors;
				}
				else if (hour == 8)
				{
					dayAmount = blender;
					nightAmount = 0f;
					finalColors = new GameScreen.LightColorPack(blender, ref this.dawnColors, ref this.dayColors);
				}
				else if (hour == 18)
				{
					dayAmount = 1f - blender;
					nightAmount = 0f;
					finalColors = new GameScreen.LightColorPack(blender, ref this.dayColors, ref this.duskColors);
				}
				else if (hour == 19)
				{
					dayAmount = 0.5f;
					nightAmount = 0.5f;
					finalColors = this.duskColors;
				}
				else
				{
					dayAmount = 0f;
					nightAmount = blender;
					this._game.SetAudio(0f, blender, 0f, 0f);
					finalColors = new GameScreen.LightColorPack(blender, ref this.duskColors, ref this.nightColors);
				}
				float tintAmount = (CastleMinerZGame.Instance.LocalPlayer.LocalPosition.Y + 32f) / 8f;
				if (tintAmount < 0f)
				{
					tintAmount = 0f;
				}
				if (tintAmount > 1f)
				{
					tintAmount = 1f;
				}
				tintAmount = 1f - tintAmount;
				this._terrain.FogColor = Color.Lerp(finalColors.fog, Color.Black, tintAmount);
				this._terrain.AmbientSunColor = finalColors.ambient;
				this._terrain.SunlightColor = finalColors.direct;
				this._terrain.SunSpecular = finalColors.specular;
				this._terrain.PercentMidnight = percentMidnight;
				int depth = this._terrain.DepthUnderGround(this._game.LocalPlayer.LocalPosition);
				float caveblender = Math.Min(1f, (float)depth / 15f);
				float hellblender = 0f;
				if (this._game.LocalPlayer.LocalPosition.Y <= -37f)
				{
					hellblender = Math.Min(1f, (-37f - this._game.LocalPlayer.LocalPosition.Y) / 10f);
				}
				this._game.SetAudio(dayAmount * (1f - caveblender), nightAmount * (1f - caveblender), caveblender * (1f - hellblender), hellblender);
			}
		}

		public void ShowInGameMenu()
		{
			this._uiGroup.PushScreen(this._inGameMenu);
		}

		public void ShowBlockPicker()
		{
			this._uiGroup.PushScreen(this._blockCraftingScreen);
		}

		public GameScreen(CastleMinerZGame game, Player localPlayer)
			: base(false)
		{
			this._game = game;
			this._terrain = this._game._terrain;
			this._localPlayer = localPlayer;
		}

		public void Inialize()
		{
			this._serverNameScreen = new PCKeyboardInputScreen(this._game, Strings.Server_Message, Strings.Enter_a_server_message + ": ", this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._serverNameScreen.ClickSound = "Click";
			this._serverNameScreen.OpenSound = "Popup";
			this._serverPasswordScreen = new PCKeyboardInputScreen(this._game, Strings.Server_Password, Strings.Enter_a_password_for_this_server + ": ", this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._serverPasswordScreen.ClickSound = "Click";
			this._serverPasswordScreen.OpenSound = "Popup";
			this._teleportMenu = new TeleportMenu(this._game);
			this._teleportMenu.MenuItemSelected += this._teleportMenu_MenuItemSelected;
			this._inGameUI = new InGameHUD(this._game);
			this._blockCraftingScreen = new CraftingScreen(this._game, this._inGameUI);
			this._optionsScreen = new OptionsScreen(true, this._uiGroup);
			this._inGameMenu = new InGameMenu(this._game);
			this._inGameMenu.MenuItemSelected += this._inGameMenu_MenuItemSelected;
			SceneScreen gameScreen = new SceneScreen(false, false);
			base.PushScreen(gameScreen);
			base.PushScreen(this._uiGroup);
			this._uiGroup.PushScreen(this._inGameUI);
			gameScreen.AfterDraw += this.gameScreen_AfterDraw;
			this.mainScene = new Scene();
			gameScreen.Scenes.Add(this.mainScene);
			this._sky = new CastleMinerSky();
			this.mainScene.Children.Add(this._terrain);
			this.mainScene.Children.Add(this._localPlayer);
			this._localPlayer.Children.Add(this._sky);
			this.SelectorEntity = new Selector();
			this.mainScene.Children.Add(this.SelectorEntity);
			this.CrackBox = new CrackBoxEntity();
			this.mainScene.Children.Add(this.CrackBox);
			this.GPSMarker = new GPSMarkerEntity();
			this.mainScene.Children.Add(this.GPSMarker);
			this.GPSMarker.Visible = false;
			PresentationParameters presentationParameters = this._game.GraphicsDevice.PresentationParameters;
			this._lastScreenSize = new Size(this._game.GraphicsDevice.PresentationParameters.BackBufferWidth, this._game.GraphicsDevice.PresentationParameters.BackBufferHeight);
			this._gameMessageManager = new GameMessageManager();
			this.mainScene.Children.Add(this._gameMessageManager);
			this._enemyManager = new EnemyManager();
			this.mainScene.Children.Add(this._enemyManager);
			this._tracerManager = new TracerManager();
			this.mainScene.Children.Add(this._tracerManager);
			this._pickupManager = new PickupManager();
			this.mainScene.Children.Add(this._pickupManager);
			this._itemBlockManager = new ItemBlockEntityManager();
			this.mainScene.Children.Add(this._itemBlockManager);
			this._postProcessView = new PostProcessView(this._game, this._game.OffScreenBuffer);
			this.mainView = new CameraView(this._game, this._postProcessView.OffScreenTarget, this._localPlayer.FPSCamera);
			this.mainView.BeforeDraw += this.PreDrawMain;
			gameScreen.Views.Add(this.mainView);
			this._fpsScene = new Scene();
			gameScreen.Scenes.Add(this._fpsScene);
			this._localPlayer.FPSMode = true;
			this._fpsScene.Children.Add(this._localPlayer.FPSNode);
			this._fpsView = new CameraView(this._game, this._postProcessView.OffScreenTarget, this._localPlayer.GunEyePointCamera);
			gameScreen.Views.Add(this._fpsView);
			gameScreen.Views.Add(this._postProcessView);
		}

		public void PopToHUD()
		{
			while (this._uiGroup.CurrentScreen != this.HUD)
			{
				this._uiGroup.PopScreen();
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			PresentationParameters pp = device.PresentationParameters;
			Size screenSize = new Size(pp.BackBufferWidth, pp.BackBufferHeight);
			if (this._lastScreenSize != screenSize)
			{
				this._postProcessView.SetDestinationTarget(this._game.OffScreenBuffer);
				this.mainView.SetDestinationTarget(this._postProcessView.OffScreenTarget);
				this._fpsView.SetDestinationTarget(this._postProcessView.OffScreenTarget);
			}
			base.OnDraw(device, spriteBatch, gameTime);
		}

		public void SwitchFreeFlyCameras()
		{
			if (this._fpsView.Enabled)
			{
				this._fpsView.Enabled = false;
				this.mainView.Camera = this.FreeFlyCamera;
				this._localPlayer.FPSMode = false;
				this.FreeFlyCameraEnabled = true;
				return;
			}
			this._fpsView.Enabled = true;
			this.mainView.Camera = this._localPlayer.FPSCamera;
			this._localPlayer.FPSMode = true;
			this.FreeFlyCameraEnabled = false;
		}

		private void PreDrawReflection(object sender, DrawEventArgs args)
		{
			if (this._terrain != null)
			{
				this._terrain.DrawDistance = 0f;
			}
		}

		private void PreDrawMain(object sender, DrawEventArgs args)
		{
			if (this._terrain != null)
			{
				this._terrain.DrawDistance = (float)this._game.PlayerStats.DrawDistance / 4f;
			}
		}

		public static bool FilterWorldGeo(Entity e)
		{
			return !(e is CastleMinerToolModel) && !(e is BaseZombie) && !(e is TorchEntity) && !(e is ParticleEmitter) && CameraView.FilterDistortions(e);
		}

		private void gameScreen_AfterDraw(object sender, DrawEventArgs e)
		{
			if (this.spriteBatch == null)
			{
				this.spriteBatch = new SpriteBatch(e.Device);
			}
			if (this._game.CurrentNetworkSession != null)
			{
				Matrix viewMat = this.mainView.Camera.View;
				Matrix projMat = this.mainView.Camera.GetProjection(e.Device);
				Matrix viewProj = viewMat * projMat;
				this.spriteBatch.Begin();
				for (int i = 0; i < this._game.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer gamer = this._game.CurrentNetworkSession.AllGamers[i];
					if (gamer.Tag != null && !gamer.IsLocal)
					{
						Player player = (Player)gamer.Tag;
						if (player.Visible)
						{
							Vector3 worldPos = player.LocalPosition + new Vector3(0f, 2f, 0f);
							Vector4 spos = Vector4.Transform(worldPos, viewProj);
							if (spos.Z > 0f)
							{
								Vector3 screenPos = new Vector3(spos.X / spos.W, spos.Y / spos.W, spos.Z / spos.W);
								screenPos *= new Vector3(0.5f, -0.5f, 1f);
								screenPos += new Vector3(0.5f, 0.5f, 0f);
								screenPos *= new Vector3((float)Screen.Adjuster.ScreenRect.Width, (float)Screen.Adjuster.ScreenRect.Height, 1f);
								Vector2 textSize = this._game._nameTagFont.MeasureString(gamer.Gamertag);
								this.spriteBatch.DrawOutlinedText(this._game._nameTagFont, gamer.Gamertag, new Vector2(screenPos.X, screenPos.Y) - textSize / 2f, Color.White, Color.Black, 1);
							}
						}
					}
				}
				this.spriteBatch.End();
			}
		}

		public void TeleportToLocation(Vector3 Location, bool spawnOnTop)
		{
			this._game.LocalPlayer.LocalPosition = Location;
			EnemyManager.Instance.ResetFarthestDistance();
			this._terrain.CenterOn(this._game.LocalPlayer.LocalPosition, true);
			InGameWaitScreen.ShowScreen(this._game, this, Strings.Please_Wait___, spawnOnTop, () => this._terrain.MinimallyLoaded);
		}

		private void _teleportMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			switch ((TeleportMenuItems)e.MenuItem.Tag)
			{
			case TeleportMenuItems.Quit:
				this._uiGroup.PopScreen();
				this._uiGroup.PopScreen();
				return;
			case TeleportMenuItems.Surface:
				this._game.MakeAboveGround(true);
				this._uiGroup.PopScreen();
				this._uiGroup.PopScreen();
				InGameWaitScreen.ShowScreen(this._game, this, Strings.Please_Wait___, true, () => this._terrain.MinimallyLoaded);
				return;
			case TeleportMenuItems.Origin:
				this._uiGroup.PopScreen();
				this._uiGroup.PopScreen();
				this.TeleportToLocation(WorldInfo.DefaultStartLocation, true);
				return;
			case TeleportMenuItems.Player:
				SelectPlayerScreen.SelectPlayer(this._game, this._uiGroup, false, false, delegate(Player player)
				{
					if (player != null)
					{
						this._game.LocalPlayer.LocalPosition = player.LocalPosition;
						this._terrain.CenterOn(this._game.LocalPlayer.LocalPosition, true);
					}
					this._uiGroup.PopScreen();
					this._uiGroup.PopScreen();
					InGameWaitScreen.ShowScreen(this._game, this, Strings.Please_Wait___, false, () => this._terrain.MinimallyLoaded);
				});
				return;
			default:
				return;
			}
		}

		private void _hostOptionsMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			switch ((HostOptionItems)e.MenuItem.Tag)
			{
			case HostOptionItems.Return:
				this._uiGroup.PopScreen();
				return;
			case HostOptionItems.Password:
				this._serverPasswordScreen.DefaultText = this._game.CurrentWorld.ServerPassword;
				this._uiGroup.ShowPCDialogScreen(this._serverPasswordScreen, delegate
				{
					if (this._serverPasswordScreen.OptionSelected != -1)
					{
						string password = this._serverPasswordScreen.TextInput;
						if (string.IsNullOrWhiteSpace(password))
						{
							if (this._game.IsOnlineGame)
							{
								this._game.CurrentNetworkSession.UpdateHostSession(null, new bool?(false), null, null);
							}
							this._game.CurrentWorld.ServerPassword = "";
							this._game.CurrentNetworkSession.Password = null;
							return;
						}
						if (this._game.IsOnlineGame)
						{
							this._game.CurrentNetworkSession.UpdateHostSession(null, new bool?(true), null, null);
						}
						this._game.CurrentWorld.ServerPassword = password;
						this._game.CurrentNetworkSession.Password = password;
					}
				});
				return;
			case HostOptionItems.KickPlayer:
				SelectPlayerScreen.SelectPlayer(this._game, this._uiGroup, false, false, delegate(Player player)
				{
					if (player != null)
					{
						BroadcastTextMessage.Send(this._game.MyNetworkGamer, player.Gamer.Gamertag + " " + Strings.has_been_kicked_by_the_host);
						KickMessage.Send(this._game.MyNetworkGamer, player.Gamer, false);
					}
				});
				return;
			case HostOptionItems.BanPlayer:
				SelectPlayerScreen.SelectPlayer(this._game, this._uiGroup, false, false, delegate(Player player)
				{
					if (player != null)
					{
						BroadcastTextMessage.Send(this._game.MyNetworkGamer, player.Gamer.Gamertag + " " + Strings.has_been_banned_by_the_host);
						KickMessage.Send(this._game.MyNetworkGamer, player.Gamer, true);
						this._game.PlayerStats.BanList[player.Gamer.AlternateAddress] = DateTime.UtcNow;
						this._game.SaveData();
					}
				});
				return;
			case HostOptionItems.ClearBanList:
				this._game.PlayerStats.BanList.Clear();
				this._game.SaveData();
				return;
			case HostOptionItems.Restart:
				RestartLevelMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer);
				BroadcastTextMessage.Send(this._game.MyNetworkGamer, this._game.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Restarted_The_Game);
				return;
			case HostOptionItems.ServerMessage:
				this._serverNameScreen.DefaultText = this._game.ServerMessage;
				this._uiGroup.ShowPCDialogScreen(this._serverNameScreen, delegate
				{
					if (this._serverNameScreen.OptionSelected != -1)
					{
						string name = this._serverNameScreen.TextInput;
						if (!string.IsNullOrWhiteSpace(name))
						{
							this._game.ServerMessage = name.Trim();
						}
					}
				});
				return;
			case HostOptionItems.PVP:
			{
				this._game.PVPState = (this._game.PVPState + 1) % (CastleMinerZGame.PVPEnum)3;
				string txt = "";
				switch (this._game.PVPState)
				{
				case CastleMinerZGame.PVPEnum.Off:
					txt = "PVP: " + Strings.Off;
					break;
				case CastleMinerZGame.PVPEnum.Everyone:
					txt = "PVP: " + Strings.Everyone;
					break;
				case CastleMinerZGame.PVPEnum.NotFriends:
					txt = "PVP: " + Strings.Non_Friends_Only;
					break;
				}
				BroadcastTextMessage.Send(this._game.MyNetworkGamer, txt);
				return;
			}
			case HostOptionItems.ChangeJoinPolicy:
			{
				int joinpolicy = (int)(this._game.JoinGamePolicy + 1);
				joinpolicy %= 3;
				this._game.JoinGamePolicy = (JoinGamePolicy)joinpolicy;
				return;
			}
			default:
				return;
			}
		}

		public void AddPlayer(Player player)
		{
			this.mainScene.Children.Add(player);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (this.exitCount > 0)
			{
				this.exitCount--;
				if (this.exitCount <= 0)
				{
					this._game.EndGame(true);
					this.exitCount = 0;
					return;
				}
			}
			if (this._localPlayer.Gamer.IsHost && this._game.IsOnlineGame)
			{
				this._sessionAliveTimer.Update(gameTime.ElapsedGameTime);
				if (this._sessionAliveTimer.Expired)
				{
					this._game.CurrentNetworkSession.ReportSessionAlive();
					this._sessionAliveTimer.Reset();
				}
			}
			float day = this.Day;
			this.Day += (float)(gameTime.ElapsedGameTime.TotalSeconds / GameScreen.LengthOfDay.TotalSeconds);
			float sunLightLevel = BlockTerrain.Instance.GetSimpleSunlightAtPoint(this._localPlayer.WorldPosition + new Vector3(0f, 1.2f, 0f));
			BloomSettings outDoorBloomSettings = BloomSettings.Lerp(this.DayBloomSettings, this.NightBloomSettings, this.DayNightBlender);
			BloomSettings baseBloomSettings = BloomSettings.Lerp(this.InDoorBloomSettings, outDoorBloomSettings, sunLightLevel);
			this._postProcessView.BloomSettings = baseBloomSettings;
			if (this._uiGroup.CurrentScreen == this.HUD || this.HUD.IsChatting)
			{
				this._postProcessView.BloomSettings = BloomSettings.Lerp(this.DeathBloomSettings, baseBloomSettings, this.HUD.PlayerHealth);
			}
			else
			{
				this._postProcessView.BloomSettings = this.HUDBloomSettings;
			}
			base.Update(game, gameTime);
		}

		private void _inGameMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (this.exitCount != 0)
			{
				return;
			}
			switch ((InGameMenuItems)e.MenuItem.Tag)
			{
			case InGameMenuItems.Return:
				this._uiGroup.PopScreen();
				if (this.IsBlockPickerUp)
				{
					this._uiGroup.PopScreen();
				}
				break;
			case InGameMenuItems.Teleport:
				this._uiGroup.PushScreen(this._teleportMenu);
				return;
			case InGameMenuItems.MyBlocks:
				this._uiGroup.PopScreen();
				if (!this.IsBlockPickerUp)
				{
					this._uiGroup.PushScreen(this._blockCraftingScreen);
					return;
				}
				break;
			case InGameMenuItems.Invite:
				break;
			case InGameMenuItems.Options:
				this._uiGroup.PushScreen(this._optionsScreen);
				return;
			case InGameMenuItems.Quit:
				if (this._game.LocalPlayer.Dead)
				{
					InGameHUD.Instance.RespawnPlayer();
				}
				if (this._localPlayer.Gamer.IsHost)
				{
					this._localPlayer.SaveInventory(this._game.SaveDevice, this._game.CurrentWorld.SavePath);
					this._localPlayer.FinalSaveRegistered = true;
				}
				else
				{
					InventoryStoreOnServerMessage.Send((LocalNetworkGamer)this._localPlayer.Gamer, this._localPlayer.PlayerInventory, true);
				}
				this.exitCount = 60;
				return;
			default:
				return;
			}
		}

		public const float MINUTES_PER_DAY = 16f;

		public BloomSettings DayBloomSettings = new BloomSettings(0.8f, 4f, 1.25f, 1f, 1f, 1f);

		public BloomSettings NightBloomSettings = new BloomSettings(0.25f, 4f, 1.25f, 1f, 1f, 1f);

		public BloomSettings InDoorBloomSettings = new BloomSettings(0.25f, 4f, 1.25f, 1f, 1f, 1f);

		public BloomSettings DeathBloomSettings = new BloomSettings(0.1f, 8f, 2f, 0.3f, 0.1f, 1f);

		public BloomSettings FlashBloomSettings = new BloomSettings(0f, 4f, 20f, 1f, 0.25f, 1f);

		public BloomSettings ConcussionBloomSettings = new BloomSettings(0.2f, 4f, 5f, 1f, 0.8f, 1f);

		public BloomSettings HUDBloomSettings = new BloomSettings(0f, 12f, 1f, 0f, 1f, 1f);

		public static readonly TimeSpan LengthOfDay = TimeSpan.FromMinutes(16.0);

		private OneShotTimer _sessionAliveTimer = new OneShotTimer(TimeSpan.FromMinutes(1.0));

		private CastleMinerZGame _game;

		private BlockTerrain _terrain;

		public CastleMinerSky _sky;

		private InGameMenu _inGameMenu;

		private OptionsScreen _optionsScreen;

		private TeleportMenu _teleportMenu;

		private InGameHUD _inGameUI;

		public ScreenGroup _uiGroup = new ScreenGroup(true);

		private CameraView mainView;

		private PostProcessView _postProcessView;

		private EnemyManager _enemyManager;

		private TracerManager _tracerManager;

		private PickupManager _pickupManager;

		private ItemBlockEntityManager _itemBlockManager;

		private GameMessageManager _gameMessageManager;

		public CraftingScreen _blockCraftingScreen;

		public Scene mainScene;

		public Selector SelectorEntity;

		public CrackBoxEntity CrackBox;

		public GPSMarkerEntity GPSMarker;

		public PerspectiveCamera FreeFlyCamera = new PerspectiveCamera();

		public bool FreeFlyCameraEnabled;

		private CameraView _fpsView;

		private List<ExplosiveFlashEntity> _explosiveFlashEntities = new List<ExplosiveFlashEntity>();

		private GameScreen.LightColorPack dawnColors = new GameScreen.LightColorPack(new Color(5, 10, 12), new Color(36, 39, 35), new Color(143, 74, 70), new Color(196, 158, 158));

		private GameScreen.LightColorPack duskColors = new GameScreen.LightColorPack(new Color(13, 15, 17), new Color(36, 39, 35), new Color(143, 74, 70), new Color(196, 158, 158));

		private GameScreen.LightColorPack dayColors = new GameScreen.LightColorPack(new Color(18, 26, 28), new Color(0.28f, 0.28f, 0.23f), new Color(0.82f, 0.7f, 0.5f), new Color(1f, 0.8f, 0.8f));

		private GameScreen.LightColorPack nightColors = new GameScreen.LightColorPack(new Color(0, 5, 11), new Color(4, 27, 52), new Color(10, 60, 106), new Color(128, 128, 220));

		private Player _localPlayer;

		private PCKeyboardInputScreen _serverNameScreen;

		private PCKeyboardInputScreen _serverPasswordScreen;

		private Size _lastScreenSize;

		public Scene _fpsScene;

		private SpriteBatch spriteBatch;

		public int exitCount;

		public struct LightColorPack
		{
			public LightColorPack(Color f, Color a, Color d, Color s)
			{
				this.fog = f;
				this.direct = d;
				this.ambient = a;
				this.specular = s;
			}

			public LightColorPack(float lerp, ref GameScreen.LightColorPack fromColor, ref GameScreen.LightColorPack toColor)
			{
				this.fog = Color.Lerp(fromColor.fog, toColor.fog, lerp);
				this.direct = Color.Lerp(fromColor.direct, toColor.direct, lerp);
				this.ambient = Color.Lerp(fromColor.ambient, toColor.ambient, lerp);
				this.specular = Color.Lerp(fromColor.specular, toColor.specular, lerp);
			}

			public Color fog;

			public Color direct;

			public Color ambient;

			public Color specular;
		}
	}
}
