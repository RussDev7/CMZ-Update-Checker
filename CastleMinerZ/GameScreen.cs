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
			ExplosiveFlashEntity explosiveFlashEntity = new ExplosiveFlashEntity(position);
			if (!this._explosiveFlashEntities.Contains(explosiveFlashEntity))
			{
				this._explosiveFlashEntities.Add(explosiveFlashEntity);
				this.mainScene.Children.Add(explosiveFlashEntity);
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
				float num = this._sky.TimeOfDay * 24f;
				float num2 = (num - (float)((int)num)) / 2f;
				int num3 = (int)num;
				if (num3 <= 5 || num3 >= 21)
				{
					return 1f;
				}
				if (num3 >= 9 && num3 <= 17)
				{
					return 0f;
				}
				if (num3 == 7 || num3 == 19)
				{
					return 0.5f;
				}
				if (num3 == 6)
				{
					return 1f - num2;
				}
				if (num3 == 8)
				{
					return 0.5f - num2;
				}
				if (num3 == 18)
				{
					return num2;
				}
				return 0.5f + num2;
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
				float num = value + 1.625f;
				num -= (float)Math.Floor((double)num);
				float num2 = num * 6.2831855f;
				float num3 = 0.2f + 0.8f * (float)Math.Abs(Math.Sin((double)num2));
				float num4 = (float)Math.Sqrt((double)(1f - num3 * num3));
				num2 -= 0.236f;
				float num5 = -(float)Math.Sin((double)num2) * num4;
				float num6 = (float)Math.Cos((double)num2) * num4;
				Vector3 vector = new Vector3(num5, num3, num6);
				this._terrain.VectorToSun = vector;
				float num7 = this._sky.TimeOfDay * 24f;
				float num8 = num7 - (float)((int)num7);
				int num9 = (int)num7;
				float num10 = (this._sky.TimeOfDay + 0.96f + 0.5f) % 1f * 2f;
				if (num10 > 1f)
				{
					num10 = 2f - num10;
				}
				float num11;
				float num12;
				GameScreen.LightColorPack lightColorPack;
				if (num9 <= 5 || num9 >= 21)
				{
					num11 = 0f;
					num12 = 1f;
					lightColorPack = this.nightColors;
				}
				else if (num9 >= 9 && num9 <= 17)
				{
					num11 = 1f;
					num12 = 0f;
					lightColorPack = this.dayColors;
				}
				else if (num9 == 6)
				{
					num11 = 0f;
					num12 = 1f - num8;
					lightColorPack = new GameScreen.LightColorPack(num8, ref this.nightColors, ref this.dawnColors);
				}
				else if (num9 == 7)
				{
					num11 = 0.5f;
					num12 = 0.5f;
					lightColorPack = this.dawnColors;
				}
				else if (num9 == 8)
				{
					num11 = num8;
					num12 = 0f;
					lightColorPack = new GameScreen.LightColorPack(num8, ref this.dawnColors, ref this.dayColors);
				}
				else if (num9 == 18)
				{
					num11 = 1f - num8;
					num12 = 0f;
					lightColorPack = new GameScreen.LightColorPack(num8, ref this.dayColors, ref this.duskColors);
				}
				else if (num9 == 19)
				{
					num11 = 0.5f;
					num12 = 0.5f;
					lightColorPack = this.duskColors;
				}
				else
				{
					num11 = 0f;
					num12 = num8;
					this._game.SetAudio(0f, num8, 0f, 0f);
					lightColorPack = new GameScreen.LightColorPack(num8, ref this.duskColors, ref this.nightColors);
				}
				float num13 = (CastleMinerZGame.Instance.LocalPlayer.LocalPosition.Y + 32f) / 8f;
				if (num13 < 0f)
				{
					num13 = 0f;
				}
				if (num13 > 1f)
				{
					num13 = 1f;
				}
				num13 = 1f - num13;
				this._terrain.FogColor = Color.Lerp(lightColorPack.fog, Color.Black, num13);
				this._terrain.AmbientSunColor = lightColorPack.ambient;
				this._terrain.SunlightColor = lightColorPack.direct;
				this._terrain.SunSpecular = lightColorPack.specular;
				this._terrain.PercentMidnight = num10;
				int num14 = this._terrain.DepthUnderGround(this._game.LocalPlayer.LocalPosition);
				float num15 = Math.Min(1f, (float)num14 / 15f);
				float num16 = 0f;
				if (this._game.LocalPlayer.LocalPosition.Y <= -37f)
				{
					num16 = Math.Min(1f, (-37f - this._game.LocalPlayer.LocalPosition.Y) / 10f);
				}
				this._game.SetAudio(num11 * (1f - num15), num12 * (1f - num15), num15 * (1f - num16), num16);
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
			SceneScreen sceneScreen = new SceneScreen(false, false);
			base.PushScreen(sceneScreen);
			base.PushScreen(this._uiGroup);
			this._uiGroup.PushScreen(this._inGameUI);
			sceneScreen.AfterDraw += this.gameScreen_AfterDraw;
			this.mainScene = new Scene();
			sceneScreen.Scenes.Add(this.mainScene);
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
			sceneScreen.Views.Add(this.mainView);
			this._fpsScene = new Scene();
			sceneScreen.Scenes.Add(this._fpsScene);
			this._localPlayer.FPSMode = true;
			this._fpsScene.Children.Add(this._localPlayer.FPSNode);
			this._fpsView = new CameraView(this._game, this._postProcessView.OffScreenTarget, this._localPlayer.GunEyePointCamera);
			sceneScreen.Views.Add(this._fpsView);
			sceneScreen.Views.Add(this._postProcessView);
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
			PresentationParameters presentationParameters = device.PresentationParameters;
			Size size = new Size(presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
			if (this._lastScreenSize != size)
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
				Matrix view = this.mainView.Camera.View;
				Matrix projection = this.mainView.Camera.GetProjection(e.Device);
				Matrix matrix = view * projection;
				this.spriteBatch.Begin();
				for (int i = 0; i < this._game.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer networkGamer = this._game.CurrentNetworkSession.AllGamers[i];
					if (networkGamer.Tag != null && !networkGamer.IsLocal)
					{
						Player player = (Player)networkGamer.Tag;
						if (player.Visible)
						{
							Vector3 vector = player.LocalPosition + new Vector3(0f, 2f, 0f);
							Vector4 vector2 = Vector4.Transform(vector, matrix);
							if (vector2.Z > 0f)
							{
								Vector3 vector3 = new Vector3(vector2.X / vector2.W, vector2.Y / vector2.W, vector2.Z / vector2.W);
								vector3 *= new Vector3(0.5f, -0.5f, 1f);
								vector3 += new Vector3(0.5f, 0.5f, 0f);
								vector3 *= new Vector3((float)Screen.Adjuster.ScreenRect.Width, (float)Screen.Adjuster.ScreenRect.Height, 1f);
								Vector2 vector4 = this._game._nameTagFont.MeasureString(networkGamer.Gamertag);
								this.spriteBatch.DrawOutlinedText(this._game._nameTagFont, networkGamer.Gamertag, new Vector2(vector3.X, vector3.Y) - vector4 / 2f, Color.White, Color.Black, 1);
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
						string textInput = this._serverPasswordScreen.TextInput;
						if (string.IsNullOrWhiteSpace(textInput))
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
						this._game.CurrentWorld.ServerPassword = textInput;
						this._game.CurrentNetworkSession.Password = textInput;
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
						string textInput2 = this._serverNameScreen.TextInput;
						if (!string.IsNullOrWhiteSpace(textInput2))
						{
							this._game.ServerMessage = textInput2.Trim();
						}
					}
				});
				return;
			case HostOptionItems.PVP:
			{
				this._game.PVPState = (this._game.PVPState + 1) % (CastleMinerZGame.PVPEnum)3;
				string text = "";
				switch (this._game.PVPState)
				{
				case CastleMinerZGame.PVPEnum.Off:
					text = "PVP: " + Strings.Off;
					break;
				case CastleMinerZGame.PVPEnum.Everyone:
					text = "PVP: " + Strings.Everyone;
					break;
				case CastleMinerZGame.PVPEnum.NotFriends:
					text = "PVP: " + Strings.Non_Friends_Only;
					break;
				}
				BroadcastTextMessage.Send(this._game.MyNetworkGamer, text);
				return;
			}
			case HostOptionItems.ChangeJoinPolicy:
			{
				int num = (int)(this._game.JoinGamePolicy + 1);
				num %= 3;
				this._game.JoinGamePolicy = (JoinGamePolicy)num;
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
			float simpleSunlightAtPoint = BlockTerrain.Instance.GetSimpleSunlightAtPoint(this._localPlayer.WorldPosition + new Vector3(0f, 1.2f, 0f));
			BloomSettings bloomSettings = BloomSettings.Lerp(this.DayBloomSettings, this.NightBloomSettings, this.DayNightBlender);
			BloomSettings bloomSettings2 = BloomSettings.Lerp(this.InDoorBloomSettings, bloomSettings, simpleSunlightAtPoint);
			this._postProcessView.BloomSettings = bloomSettings2;
			if (this._uiGroup.CurrentScreen == this.HUD || this.HUD.IsChatting)
			{
				this._postProcessView.BloomSettings = BloomSettings.Lerp(this.DeathBloomSettings, bloomSettings2, this.HUD.PlayerHealth);
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
