using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using DNA.Net.MatchMaking;
using DNA.Profiling;
using DNA.Security.Cryptography;
using DNA.Text;
using DNA.Timers;
using Facebook;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ
{
	public class FrontEndScreen : ScreenGroup
	{
		public FrontEndScreen(CastleMinerZGame game)
			: base(false)
		{
			this._versionString = Strings.Version + " " + game.Version.ToString();
			this._releaseNotesScreen = new ReleaseNotesScreen(game, this._versionString);
			this._optionsScreen = new OptionsScreen(false, this._uiGroup);
			this._game = game;
			this._largeFont = game._largeFont;
			this.SpriteBatch = new SpriteBatch(game.GraphicsDevice);
			MenuBackdropScreen menuBackdropScreen = new MenuBackdropScreen(game);
			base.PushScreen(menuBackdropScreen);
			base.PushScreen(this._uiGroup);
			this._uiGroup.PushScreen(this._startScreen);
			this._startScreen.ClickSound = "Click";
			this._startScreen.OnStartPressed += this._startScreen_OnStartPressed;
			this._startScreen.AfterDraw += this._startScreen_AfterDraw;
			this._startScreen.OnBackPressed += this._startScreen_OnBackPressed;
			this._mainMenu = new MainMenu(game);
			this._mainMenu.MenuItemSelected += this._mainMenu_MenuItemSelected;
			this._gameModeMenu = new GameModeMenu(game);
			this._gameModeMenu.MenuItemSelected += this._gameModeMenu_MenuItemSelected;
			this._quitDialog = new PCDialogScreen(Strings.Quit_Game, Strings.Are_you_sure_you_want_to_quit, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._quitDialog.UseDefaultValues();
			this._undeadNotKilledDialog = new PCDialogScreen(Strings.Kill_The_Undead_Dragon, Strings.Unlock_this_game_mode_by_killing_the_Undead_Dragon_in_Endurance_Mode, null, false, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._undeadNotKilledDialog.UseDefaultValues();
			this._modeNotUnlockedDialog = new PCDialogScreen(Strings.Trial_Mode, Strings.You_must_purchase_the_game_before_you_can_play_in_this_game_mode_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._modeNotUnlockedDialog.UseDefaultValues();
			this._onlinePlayNotPurchasedDialog = new PCDialogScreen(Strings.Trial_Mode, Strings.You_must_purchase_the_game_before_you_can_play_online_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._onlinePlayNotPurchasedDialog.UseDefaultValues();
			this._optimizeStorageDialog = new PCDialogScreen(Strings.Optimize_Storage, Strings.To_decrease_load_time_it_is_recommended_that_you_optimize_your_storage__Would_you_like_to_do_this_now___this_may_take_several_minutes_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._optimizeStorageDialog.UseDefaultValues();
			this._playerNameInput = new PCKeyboardInputScreen(this._game, "  ", Strings.Enter_Your_Name + ":", this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._playerNameInput.ClickSound = "Click";
			this._playerNameInput.OpenSound = "Popup";
			this._serverPasswordScreen = new PCKeyboardInputScreen(this._game, Strings.Server_Password, Strings.Enter_a_password_for_this_server + ": ", this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._serverPasswordScreen.ClickSound = "Click";
			this._serverPasswordScreen.OpenSound = "Popup";
			this._difficultyLevelScreen = new DifficultyLevelScreen(game);
			this._difficultyLevelScreen.MenuItemSelected += this._difficultyLevelScreen_MenuItemSelected;
			this._connectingScreen.BeforeDraw += this._connectingScreen_BeforeDraw;
			this._chooseOnlineGameScreen = new ChooseOnlineGameScreen();
			this._chooseOnlineGameScreen.Clicked += this._chooseOnlineGameScreen_Clicked;
			this._chooseSavedWorldScreen = new ChooseSavedWorldScreen();
			this._chooseSavedWorldScreen.Clicked += this._chooseSavedWorldScreen_Clicked;
			this._chooseAnotherGameScreen.BeforeDraw += this._chooseAnotherGameScreen_BeforeDraw;
			this._chooseAnotherGameScreen.ProcessingPlayerInput += this._chooseAnotherGameScreen_ProcessingPlayerInput;
			this._loadingScreen.BeforeDraw += this._loadingScreen_BeforeDraw;
			this.optimizeStorageWaitScreen = new WaitScreen(Strings.Optimizing_Storage___, true, new ThreadStart(this.DeleteWorlds), null);
			this.optimizeStorageWaitScreen.Updating += this.optimizeStorageWaitScreen_Updating;
			this.optimizeStorageWaitScreen.ProcessingPlayerInput += this.optimizeStorageWaitScreen_ProcessingPlayerInput;
			this.optimizeStorageWaitScreen.AfterDraw += this.optimizeStorageWaitScreen_AfterDraw;
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			this._uiGroup.PopScreen();
		}

		private void _chooseSavedWorldScreen_Clicked(object sender, EventArgs e)
		{
			ChooseSavedWorldScreen.SavedWorldItem info = (ChooseSavedWorldScreen.SavedWorldItem)this._chooseSavedWorldScreen.SelectedItem;
			if (info.World.OwnerGamerTag != Screen.CurrentGamer.Gamertag)
			{
				this._uiGroup.ShowPCDialogScreen(this._chooseSavedWorldScreen._takeOverTerrain, delegate
				{
					if (this._chooseSavedWorldScreen._takeOverTerrain.OptionSelected != -1)
					{
						this.WorldManager.TakeOwnership(info.World);
						this._game.BeginLoadTerrain(info.World, true);
						this.HostGame(this._localGame);
					}
				});
				return;
			}
			if (info.World.InfiniteResourceMode != this._game.InfiniteResourceMode)
			{
				this._uiGroup.ShowPCDialogScreen(this._chooseSavedWorldScreen._infiniteModeConversion, delegate
				{
					if (this._chooseSavedWorldScreen._infiniteModeConversion.OptionSelected != -1)
					{
						this.WorldManager.TakeOwnership(info.World);
						this._game.BeginLoadTerrain(info.World, true);
						this.HostGame(this._localGame);
					}
				});
				return;
			}
			this._game.BeginLoadTerrain(info.World, true);
			this.HostGame(this._localGame);
		}

		public override void OnPushed()
		{
			base.OnPushed();
		}

		private void _chooseOnlineGameScreen_Clicked(object sender, EventArgs e)
		{
			ChooseOnlineGameScreen.OnlineGameMenuItem onlineGameMenuItem = (ChooseOnlineGameScreen.OnlineGameMenuItem)this._chooseOnlineGameScreen.SelectedItem;
			this._chooseOnlineGameScreen.ShutdownHostDiscovery();
			this.JoinGame(onlineGameMenuItem.NetworkSession, onlineGameMenuItem.Password);
		}

		private void _startScreen_OnBackPressed(object sender, EventArgs e)
		{
			this._game.Exit();
		}

		public void PushReleaseNotesScreen()
		{
			this._uiGroup.PushScreen(this._releaseNotesScreen);
		}

		private void _difficultyLevelScreen_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (e.MenuItem.Tag == null)
			{
				this._uiGroup.PopScreen();
				return;
			}
			GameDifficultyTypes gameDifficultyTypes = (GameDifficultyTypes)e.MenuItem.Tag;
			this._game.Difficulty = gameDifficultyTypes;
			this._uiGroup.PushScreen(this._chooseSavedWorldScreen);
		}

		private void _gameModeMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (e.MenuItem.Tag == null)
			{
				this._uiGroup.PopScreen();
				return;
			}
			GameModeTypes gameModeTypes = (GameModeTypes)e.MenuItem.Tag;
			this._game.GameMode = gameModeTypes;
			this._game.InfiniteResourceMode = false;
			this._game.Difficulty = GameDifficultyTypes.EASY;
			this._game.JoinGamePolicy = JoinGamePolicy.Anyone;
			if (this._localGame)
			{
				switch (gameModeTypes)
				{
				case GameModeTypes.Endurance:
					this.startWorld();
					return;
				case GameModeTypes.Survival:
					if (CastleMinerZGame.TrialMode)
					{
						this._uiGroup.ShowPCDialogScreen(this._modeNotUnlockedDialog, delegate
						{
							if (this._modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						return;
					}
					this._game.GameMode = GameModeTypes.Survival;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					return;
				case GameModeTypes.DragonEndurance:
					if (CastleMinerZGame.TrialMode)
					{
						this._uiGroup.ShowPCDialogScreen(this._modeNotUnlockedDialog, delegate
						{
							if (this._modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						return;
					}
					if (this._game.PlayerStats.UndeadDragonKills > 0 || this._game.LicenseServices.GetAddOn(AddOnIDs.DragonEndurance) != null)
					{
						this._uiGroup.PushScreen(this._chooseSavedWorldScreen);
						return;
					}
					this._uiGroup.ShowPCDialogScreen(this._undeadNotKilledDialog, delegate
					{
					});
					return;
				case GameModeTypes.Creative:
					if (CastleMinerZGame.TrialMode)
					{
						this._uiGroup.ShowPCDialogScreen(this._modeNotUnlockedDialog, delegate
						{
							if (this._modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						return;
					}
					this._game.GameMode = GameModeTypes.Creative;
					this._game.InfiniteResourceMode = true;
					this._game.Difficulty = GameDifficultyTypes.EASY;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					return;
				case GameModeTypes.Exploration:
					if (CastleMinerZGame.TrialMode)
					{
						this._uiGroup.ShowPCDialogScreen(this._modeNotUnlockedDialog, delegate
						{
							if (this._modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						return;
					}
					this._game.GameMode = GameModeTypes.Exploration;
					this._game.InfiniteResourceMode = true;
					this._game.Difficulty = GameDifficultyTypes.EASY;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					return;
				case GameModeTypes.Scavenger:
					if (CastleMinerZGame.TrialMode)
					{
						this._uiGroup.ShowPCDialogScreen(this._modeNotUnlockedDialog, delegate
						{
							if (this._modeNotUnlockedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
						return;
					}
					this._game.GameMode = GameModeTypes.Scavenger;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					this._game.BeginLoadTerrain(null, true);
					return;
				default:
					return;
				}
			}
			else
			{
				switch (gameModeTypes)
				{
				case GameModeTypes.Endurance:
					this.startWorld();
					return;
				case GameModeTypes.Survival:
					this._game.GameMode = GameModeTypes.Survival;
					this._game.Difficulty = GameDifficultyTypes.EASY;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					return;
				case GameModeTypes.DragonEndurance:
					if (this._game.PlayerStats.UndeadDragonKills > 0 || this._game.LicenseServices.GetAddOn(AddOnIDs.DragonEndurance) != null)
					{
						this._uiGroup.PushScreen(this._chooseSavedWorldScreen);
						return;
					}
					this._uiGroup.ShowPCDialogScreen(this._undeadNotKilledDialog, delegate
					{
					});
					return;
				case GameModeTypes.Creative:
					this._game.GameMode = GameModeTypes.Creative;
					this._game.InfiniteResourceMode = true;
					this._game.Difficulty = GameDifficultyTypes.EASY;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					return;
				case GameModeTypes.Exploration:
					this._game.GameMode = GameModeTypes.Exploration;
					this._game.InfiniteResourceMode = true;
					this._game.Difficulty = GameDifficultyTypes.EASY;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					return;
				case GameModeTypes.Scavenger:
					this._game.GameMode = GameModeTypes.Scavenger;
					this._game.Difficulty = GameDifficultyTypes.EASY;
					this._uiGroup.PushScreen(this._difficultyLevelScreen);
					this._game.BeginLoadTerrain(null, true);
					return;
				default:
					return;
				}
			}
		}

		public void startWorld()
		{
			WorldTypeIDs terrainVersion = this._game.CurrentWorld._terrainVersion;
			this.WorldManager.TakeOwnership(this._game.CurrentWorld);
			this._game.CurrentWorld._terrainVersion = WorldTypeIDs.CastleMinerZ;
			if (terrainVersion != this._game.CurrentWorld._terrainVersion)
			{
				this._game.BeginLoadTerrain(this._game.CurrentWorld, true);
			}
			this.HostGame(this._localGame);
		}

		public void ShowUIDialog(string title, string message, bool drawbehind)
		{
			PCDialogScreen pcdialogScreen = new PCDialogScreen(title, message, null, false, this._game.DialogScreenImage, this._game._myriadMed, drawbehind, this._game.ButtonFrame);
			pcdialogScreen.UseDefaultValues();
			this._uiGroup.ShowPCDialogScreen(pcdialogScreen, null);
		}

		private void JoinCallback(bool success, string message)
		{
			if (success)
			{
				this._game.GetWorldInfo(delegate(WorldInfo worldInfo)
				{
					this._uiGroup.PopScreen();
					this.WorldManager.RegisterNetworkWorld(worldInfo);
					this._game.BeginLoadTerrain(worldInfo, false);
					this._uiGroup.PushScreen(this._loadingScreen);
					this._game.WaitForTerrainLoad(delegate
					{
						this._uiGroup.PopScreen();
						this._game.StartGame();
					});
				});
				return;
			}
			this.PopToMainMenu(Screen.CurrentGamer, null);
			if (message == "Connection failed: GamerAlreadyConnected")
			{
				this.ShowUIDialog(Strings.Connection_Error, Strings.A_gamer_logged_in_with_these_credentials_is_already_playing_in_this_session_, false);
				return;
			}
			if (message == null)
			{
				this.ShowUIDialog(Strings.Connection_Error, Strings.There_was_an_unspecified_error_connecting_, false);
				return;
			}
			this.ShowUIDialog(Strings.Connection_Error, message, false);
		}

		private void JoinGame(AvailableNetworkSession session, string password)
		{
			this._uiGroup.PushScreen(this._connectingScreen);
			this._game.JoinGame(session, new SignedInGamer[] { Screen.CurrentGamer }, new SuccessCallbackWithMessage(this.JoinCallback), "CastleMinerZSteam", 4, password);
		}

		private void GetPasswordForInvitedGameCallback(ClientSessionInfo sessionInfo, object context, SetPasswordForInvitedGameCallback callback)
		{
			if (sessionInfo.SessionProperties[1] != 0)
			{
				TaskDispatcher.Instance.AddTaskForMainThread(delegate
				{
					callback(true, "", Strings.Game_can_no_longer_be_joined, context);
				});
				return;
			}
			if (sessionInfo.PasswordProtected)
			{
				this._serverPasswordScreen.DefaultText = "";
				this._game.FrontEnd.ShowPCDialogScreen(this._serverPasswordScreen, delegate
				{
					string text;
					bool flag;
					string text2;
					if (this._serverPasswordScreen.OptionSelected != -1)
					{
						text = this._serverPasswordScreen.TextInput;
						flag = false;
						text2 = null;
					}
					else
					{
						text = "";
						flag = true;
						text2 = Strings.Action_was_cancelled;
					}
					callback(flag, text, text2, context);
				});
				return;
			}
			TaskDispatcher.Instance.AddTaskForMainThread(delegate
			{
				callback(false, "", null, context);
			});
		}

		public void JoinInvitedGame(ulong lobbyId)
		{
			this._uiGroup.PushScreen(this._connectingScreen);
			this._game.JoinInvitedGame(lobbyId, 4, "CastleMinerZSteam", new SignedInGamer[] { Screen.CurrentGamer }, new SuccessCallbackWithMessage(this.JoinCallback), new GetPasswordForInvitedGameCallback(this.GetPasswordForInvitedGameCallback));
		}

		private void _startScreen_AfterDraw(object sender, DrawEventArgs e)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			this._flashTimer.Update(e.GameTime.ElapsedGameTime);
			if (this._flashTimer.Expired)
			{
				this._flashTimer.Reset();
				this._flashDir = !this._flashDir;
			}
			float num = (this._flashDir ? this._flashTimer.PercentComplete : (1f - this._flashTimer.PercentComplete));
			Color color = Color.Lerp(CMZColors.MenuGreen, Color.White, num);
			Rectangle rectangle = new Rectangle(screenRect.Center.X - (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.Y / 2f), screenRect.Center.Y - (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y));
			this.SpriteBatch.Begin();
			this._game.Logo.Draw(this.SpriteBatch, rectangle, Color.White);
			string text = "www.CastleMinerZ.com";
			Vector2 vector = this._game._medFont.MeasureString(text);
			this.SpriteBatch.DrawOutlinedText(this._game._medFont, text, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - vector.X / 2f, (float)Screen.Adjuster.ScreenRect.Bottom - vector.Y), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			vector = this._largeFont.MeasureString(Strings.Start_Game);
			this.SpriteBatch.DrawOutlinedText(this._largeFont, Strings.Start_Game, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - vector.X / 2f, (float)rectangle.Bottom), color, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			this.SpriteBatch.DrawOutlinedText(this._game._consoleFont, this._versionString, new Vector2(0f, 0f), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			this.SpriteBatch.End();
		}

		private void _startScreen_OnStartPressed(object sender, EventArgs e)
		{
			this.SetupSaveDevice(delegate(bool success)
			{
				if (success)
				{
					WaitScreen.DoWait(this._uiGroup, Strings.Loading_Player_Info___, delegate
					{
						DateTime now = DateTime.Now;
						if (Screen.CurrentGamer == null)
						{
							return;
						}
						this.SetupNewGamer(Screen.CurrentGamer, this._game.SaveDevice);
						TimeSpan timeSpan = DateTime.Now - now;
						if (Screen.CurrentGamer != null)
						{
							this._uiGroup.PushScreen(this._mainMenu);
							if (timeSpan > TimeSpan.FromSeconds(20.0))
							{
								this._uiGroup.ShowPCDialogScreen(this._optimizeStorageDialog, delegate
								{
									if (this._optimizeStorageDialog.OptionSelected != -1)
									{
										this.OptimizeStorage();
									}
								});
							}
						}
						this._game.CurrentWorld.ServerMessage = Screen.CurrentGamer.Gamertag + "'s " + Strings.Server;
					}, delegate
					{
						ulong lobbyId = CommandLineArgs.Get<CastleMinerZArgs>().InvitedLobbyID;
						if (lobbyId != 0UL)
						{
							TaskDispatcher.Instance.AddTaskForMainThread(delegate
							{
								CommandLineArgs.Get<CastleMinerZArgs>().InvitedLobbyID = 0UL;
								this.JoinInvitedGame(lobbyId);
							});
						}
					});
				}
			});
		}

		public void OptimizeStorage()
		{
			WaitScreen.DoWait(this._uiGroup, Strings.Optimizing_Storage___, delegate
			{
				this.Cancel = false;
				WorldInfo[] worlds = this.WorldManager.GetWorlds();
				this.OriginalWorldsCount = 0;
				for (int i = 0; i < worlds.Length; i++)
				{
					string gamertag = Screen.CurrentGamer.Gamertag;
					if (worlds[i].OwnerGamerTag != gamertag)
					{
						this.OriginalWorldsCount++;
					}
				}
				this.OriginalWorldsCount += WorldInfo.CorruptWorlds.Count;
				this.CurrentWorldsCount = this.OriginalWorldsCount;
				this.optimizeStorageWaitScreen.Progress = 0;
				this.optimizeStorageWaitScreen.Start(this._uiGroup);
			}, null);
			this.PopToStartScreen();
		}

		private void DeleteWorlds()
		{
			WorldManager worldManager = this.WorldManager;
			if (worldManager == null)
			{
				return;
			}
			WorldInfo[] worlds = worldManager.GetWorlds();
			for (int i = 0; i < worlds.Length; i++)
			{
				if (Screen.CurrentGamer == null)
				{
					return;
				}
				string gamertag = Screen.CurrentGamer.Gamertag;
				if (worlds[i].OwnerGamerTag != gamertag)
				{
					worldManager.Delete(worlds[i]);
					this.CurrentWorldsCount--;
				}
				if (this.Cancel)
				{
					break;
				}
			}
			int num = 0;
			while (WorldInfo.CorruptWorlds.Count > 0)
			{
				try
				{
					this._game.SaveDevice.DeleteDirectory(WorldInfo.CorruptWorlds[num]);
				}
				catch
				{
				}
				WorldInfo.CorruptWorlds.RemoveAt(num);
				this.CurrentWorldsCount--;
				if (this.Cancel)
				{
					break;
				}
			}
			this._game.SaveDevice.Flush();
		}

		private void optimizeStorageWaitScreen_AfterDraw(object sender, DrawEventArgs e)
		{
			Vector2 vector = this._game._medFont.MeasureString(Strings.Press_Esc_to_Cancel);
			int num = (int)((float)Screen.Adjuster.ScreenRect.Height - vector.Y);
			int num2 = (int)((float)Screen.Adjuster.ScreenRect.Width - vector.X);
			this.SpriteBatch.Begin();
			this.SpriteBatch.DrawOutlinedText(this._game._medFont, Strings.Press_Esc_to_Cancel, new Vector2((float)num2, (float)num), Color.White, Color.Black, 1);
			this.SpriteBatch.End();
		}

		private void optimizeStorageWaitScreen_ProcessingPlayerInput(object sender, ControllerInputEventArgs e)
		{
			if (e.Controller.PressedButtons.B || e.Controller.PressedButtons.Back || e.Keyboard.WasKeyPressed(Keys.Escape))
			{
				this.Cancel = true;
				this.optimizeStorageWaitScreen.Message = Strings.Canceling___;
				this.optimizeStorageWaitScreen._drawProgress = false;
			}
		}

		private void optimizeStorageWaitScreen_Updating(object sender, UpdateEventArgs e)
		{
			float num;
			if (this.OriginalWorldsCount > 0)
			{
				num = 1f - (float)this.CurrentWorldsCount / (float)this.OriginalWorldsCount;
			}
			else
			{
				num = 1f;
			}
			this.optimizeStorageWaitScreen.Progress = (int)(100f * num);
		}

		private void CloseSaveDevice()
		{
			if (this._game.SaveDevice != null)
			{
				this._game.SaveDevice.Dispose();
				this._game.SaveDevice = null;
			}
		}

		private static void GetFiles(string path, List<string> returnedFiles)
		{
			string[] directories = Directory.GetDirectories(path);
			foreach (string text in directories)
			{
				FrontEndScreen.GetFiles(text, returnedFiles);
			}
			string[] files = Directory.GetFiles(path);
			foreach (string text2 in files)
			{
				returnedFiles.Add(text2);
			}
		}

		private void UpdateSaves(string path, byte[] newKey)
		{
			string text = Path.Combine(path, "save.version");
			if (File.Exists(text))
			{
				return;
			}
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			byte[] data = md5HashProvider.Compute(Encoding.UTF8.GetBytes(Screen.CurrentGamer.Gamertag + "CMZ778")).Data;
			SaveDevice saveDevice = new FileSystemSaveDevice(path, data);
			SaveDevice saveDevice2 = new FileSystemSaveDevice(path, newKey);
			List<string> list = new List<string>();
			FrontEndScreen.GetFiles(path, list);
			foreach (string text2 in list)
			{
				byte[] array;
				try
				{
					array = saveDevice.LoadData(text2);
				}
				catch
				{
					continue;
				}
				saveDevice2.Save(text2, array, true, true);
			}
			using (FileStream fileStream = File.Open(text, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				binaryWriter.Write("VER");
				binaryWriter.Write(1);
			}
		}

		private void SetupSaveDevice(SuccessCallback callback)
		{
			WaitScreen waitScreen = new WaitScreen(Strings.Opening_Storage_Device);
			this._uiGroup.PushScreen(waitScreen);
			this.CloseSaveDevice();
			ulong steamUserID = CastleMinerZGame.Instance.LicenseServices.SteamUserID;
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			byte[] data = md5HashProvider.Compute(Encoding.UTF8.GetBytes(steamUserID.ToString() + "CMZ778")).Data;
			string appDataDirectory = GlobalSettings.GetAppDataDirectory();
			string text = Path.Combine(appDataDirectory, steamUserID.ToString());
			try
			{
				string text2 = Path.Combine(appDataDirectory, Screen.CurrentGamer.Gamertag);
				if (Directory.Exists(text2))
				{
					Directory.Move(text2, text);
				}
			}
			catch (Exception)
			{
			}
			this.UpdateSaves(text, data);
			this._game.SaveDevice = new FileSystemSaveDevice(text, data);
			callback(true);
			waitScreen.PopMe();
		}

		public void BeginSetupNewGamer(SignedInGamer gamer)
		{
			this.WorldManager = null;
			this._game.SetupNewGamer(gamer);
		}

		public void EndSetupNewGamer(SignedInGamer gamer, SaveDevice saveDevice)
		{
			this.WorldManager = new WorldManager(gamer, saveDevice);
		}

		public void SetupNewGamer(SignedInGamer gamer, SaveDevice saveDevice)
		{
			this.BeginSetupNewGamer(gamer);
			this.EndSetupNewGamer(gamer, saveDevice);
		}

		private bool _allowConnectionCallbackAlt(PlayerID playerID, ulong id)
		{
			return !this._game.PlayerStats.BanList.ContainsKey(id);
		}

		private void HostGame(bool local)
		{
			this._uiGroup.PushScreen(this._loadingScreen);
			this._game.WaitForTerrainLoad(delegate
			{
				this._uiGroup.PopScreen();
				this._uiGroup.PushScreen(this._connectingScreen);
				this._game.HostGame(local, delegate(bool result)
				{
					if (result)
					{
						this._game.TerrainServerID = this._game.MyNetworkGamer.Id;
						this._game.StartGame();
						this._game.CurrentNetworkSession.AllowConnectionCallbackAlt = new NetworkSession.AllowConnectionCallbackDelegateAlt(this._allowConnectionCallbackAlt);
						if (CastleMinerZGame.Instance.CurrentNetworkSession.ExternalIPString != null && this._game.PlayerStats.PostOnHost)
						{
							CastleMinerZGame.Instance.TaskScheduler.QueueUserWorkItem(delegate
							{
								try
								{
									new FacebookClient(CastleMinerZGame.FacebookAccessToken);
									new PostToWall
									{
										Message = Strings.Hosting_at_internet_address + ": " + CastleMinerZGame.Instance.CurrentNetworkSession.ExternalIPString + " #CMZServer",
										Link = "http://castleminerz.com/",
										Description = Strings.Travel_with_your_friends_in_a_huge__ever_changing_world_and_craft_modern_weapons_to_defend_yourself_from_dragons_and_the_zombie_horde_,
										ActionName = Strings.Download_Now,
										ActionURL = "http://castleminerz.com/Download.html",
										ImageURL = "http://digitaldnagames.com/Images/CastleMinerZBox.jpg",
										AccessToken = CastleMinerZGame.FacebookAccessToken
									}.Post();
								}
								catch
								{
								}
							});
							return;
						}
					}
					else
					{
						this._uiGroup.PopScreen();
						this.ShowUIDialog(Strings.Hosting_Error, Strings.There_was_an_error_hosting_the_game_, false);
					}
				});
			});
		}

		private void _mainMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			ThreadStart threadStart = null;
			if (this.WorldManager == null)
			{
				return;
			}
			switch ((MainMenuItems)e.MenuItem.Tag)
			{
			case MainMenuItems.HostOnline:
				if (CastleMinerZGame.TrialMode)
				{
					this._uiGroup.ShowPCDialogScreen(this._onlinePlayNotPurchasedDialog, delegate
					{
						if (this._onlinePlayNotPurchasedDialog.OptionSelected != -1)
						{
							Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
						}
					});
					return;
				}
				this._localGame = false;
				this._hostGame = true;
				this._uiGroup.PushScreen(this._gameModeMenu);
				return;
			case MainMenuItems.JoinOnline:
				if (CastleMinerZGame.TrialMode)
				{
					ScreenGroup uiGroup = this._uiGroup;
					PCDialogScreen onlinePlayNotPurchasedDialog = this._onlinePlayNotPurchasedDialog;
					if (threadStart == null)
					{
						threadStart = delegate
						{
							if (this._onlinePlayNotPurchasedDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						};
					}
					uiGroup.ShowPCDialogScreen(onlinePlayNotPurchasedDialog, threadStart);
					return;
				}
				this._localGame = false;
				this._hostGame = false;
				this._game.GetNetworkSessions(delegate(AvailableNetworkSessionCollection result)
				{
					this._chooseOnlineGameScreen.Populate(result);
					this._uiGroup.PopScreen();
					this._uiGroup.PushScreen(this._chooseOnlineGameScreen);
				});
				this._uiGroup.PushScreen(this._connectingScreen);
				return;
			case MainMenuItems.PlayOffline:
				this._localGame = true;
				this._uiGroup.PushScreen(this._gameModeMenu);
				return;
			case MainMenuItems.Purchase:
				Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
				return;
			case MainMenuItems.Quit:
				this.ConfirmExit();
				return;
			case MainMenuItems.Options:
				this._uiGroup.PushScreen(this._optionsScreen);
				return;
			default:
				return;
			}
		}

		public void ConfirmExit()
		{
			this._uiGroup.ShowPCDialogScreen(this._quitDialog, delegate
			{
				if (this._quitDialog.OptionSelected != -1)
				{
					this._game.Exit();
				}
			});
		}

		private void _loadingScreen_BeforeDraw(object sender, DrawEventArgs e)
		{
			float loadProgress = this._game.LoadProgress;
			string text = Strings.Loading_The_World____ + Strings.Please_Wait___;
			float num = (float)Screen.Adjuster.ScreenRect.Width * 0.8f;
			float num2 = (float)Screen.Adjuster.ScreenRect.Left + ((float)Screen.Adjuster.ScreenRect.Width - num) / 2f;
			Sprite sprite = this._game._uiSprites["Bar"];
			Vector2 vector = this._largeFont.MeasureString(text);
			Vector2 vector2 = new Vector2(num2, (float)(Screen.Adjuster.ScreenRect.Height / 2) + vector.Y);
			float num3 = vector2.Y + (float)this._largeFont.LineSpacing + 10f * Screen.Adjuster.ScaleFactor.Y;
			Rectangle rectangle = new Rectangle((int)num2, (int)num3, (int)num, this._largeFont.LineSpacing);
			int left = rectangle.Left;
			int top = rectangle.Top;
			float num4 = (float)rectangle.Width / (float)sprite.Width;
			this.SpriteBatch.Begin();
			int num5 = (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.X);
			int num6 = (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y);
			Rectangle rectangle2 = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - num5 / 2, 0, num5, num6);
			this._game.Logo.Draw(this.SpriteBatch, rectangle2, Color.White);
			this.SpriteBatch.DrawOutlinedText(this._largeFont, text, vector2, Color.White, Color.Black, 1);
			this.SpriteBatch.Draw(this._game.DummyTexture, new Rectangle(left - 2, top - 2, rectangle.Width + 4, rectangle.Height + 4), Color.White);
			this.SpriteBatch.Draw(this._game.DummyTexture, new Rectangle(left, top, rectangle.Width, rectangle.Height), Color.Black);
			int num7 = (int)((float)sprite.Width * loadProgress);
			sprite.Draw(this.SpriteBatch, new Rectangle(left, top, (int)((float)rectangle.Width * loadProgress), rectangle.Height), new Rectangle(sprite.Width - num7, 0, num7, sprite.Height), Color.White);
			this.textFlashTimer.Update(e.GameTime.ElapsedGameTime);
			Color.Lerp(Color.Green, Color.White, this.textFlashTimer.PercentComplete);
			if (this.textFlashTimer.Expired)
			{
				this.textFlashTimer.Reset();
			}
			this.SpriteBatch.End();
		}

		private void _connectingScreen_BeforeDraw(object sender, DrawEventArgs e)
		{
			string text = Strings.Connecting____ + Strings.Please_Wait___;
			Vector2 vector = this._largeFont.MeasureString(text);
			Vector2 vector2 = new Vector2((float)(Screen.Adjuster.ScreenRect.Width / 2) - vector.X / 2f, (float)(Screen.Adjuster.ScreenRect.Height / 2) + vector.Y);
			this.textFlashTimer.Update(e.GameTime.ElapsedGameTime);
			Color color = Color.Lerp(Color.Green, Color.White, this.textFlashTimer.PercentComplete);
			if (this.textFlashTimer.Expired)
			{
				this.textFlashTimer.Reset();
			}
			this.SpriteBatch.Begin();
			int num = (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.X);
			int num2 = (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y);
			Rectangle rectangle = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - num / 2, 0, num, num2);
			this._game.Logo.Draw(this.SpriteBatch, rectangle, Color.White);
			this.SpriteBatch.DrawOutlinedText(this._largeFont, text, vector2, color, Color.Black, 1);
			this.SpriteBatch.End();
		}

		private void _chooseAnotherGameScreen_ProcessingPlayerInput(object sender, ControllerInputEventArgs e)
		{
			if (e.Controller.PressedButtons.A || e.Controller.PressedButtons.B || e.Controller.PressedButtons.Back || e.Keyboard.WasKeyPressed(Keys.Escape) || e.Keyboard.WasKeyPressed(Keys.Enter) || e.Mouse.LeftButtonPressed)
			{
				this._uiGroup.PopScreen();
			}
		}

		private void _chooseAnotherGameScreen_BeforeDraw(object sender, DrawEventArgs e)
		{
			this.SpriteBatch.Begin();
			string session_Ended = Strings.Session_Ended;
			Vector2 vector = this._largeFont.MeasureString(session_Ended);
			int lineSpacing = this._largeFont.LineSpacing;
			this.SpriteBatch.DrawOutlinedText(this._largeFont, session_Ended, new Vector2(640f - vector.X / 2f, 360f - vector.Y / 2f), Color.White, Color.Black, 2);
			this.SpriteBatch.End();
		}

		public void PopToStartScreen()
		{
			while (this._uiGroup.CurrentScreen != this._startScreen && this._uiGroup.CurrentScreen != null)
			{
				this._uiGroup.PopScreen();
			}
			if (this._uiGroup.CurrentScreen == null)
			{
				this._uiGroup.PushScreen(this._startScreen);
			}
			this._game.SetAudio(1f, 0f, 0f, 0f);
			this._game.PlayMusic("Theme");
		}

		public void PopToMainMenu(SignedInGamer gamer, SuccessCallback callback)
		{
			while (this._uiGroup.CurrentScreen != this._mainMenu && this._uiGroup.CurrentScreen != null)
			{
				this._uiGroup.PopScreen();
			}
			Screen.SelectedPlayerIndex = new PlayerIndex?(gamer.PlayerIndex);
			if (this._uiGroup.CurrentScreen == null && this._game.SaveDevice != null)
			{
				this.CloseSaveDevice();
			}
			this._game.SetAudio(1f, 0f, 0f, 0f);
			this._game.PlayMusic("Theme");
			if (this._uiGroup.CurrentScreen == null)
			{
				this._uiGroup.PushScreen(this._startScreen);
			}
			if (this._game.SaveDevice == null)
			{
				this.SetupSaveDevice(delegate(bool success)
				{
					this._uiGroup.PushScreen(this._mainMenu);
					callback(success);
				});
				return;
			}
			if (callback != null)
			{
				callback(true);
			}
		}

		public CastleMinerZGame _game;

		public ScreenGroup _uiGroup = new ScreenGroup(true);

		private ChooseSavedWorldScreen _chooseSavedWorldScreen;

		private Screen _connectingScreen = new Screen(true, false);

		private Screen _loadingScreen = new Screen(true, false);

		private MainMenu _mainMenu;

		private OptionsScreen _optionsScreen;

		private AchievementScreen<CastleMinerZPlayerStats> _achievementScreen;

		private GameModeMenu _gameModeMenu;

		private PCDialogScreen _undeadNotKilledDialog;

		private PCDialogScreen _modeNotUnlockedDialog;

		private PCDialogScreen _onlinePlayNotPurchasedDialog;

		private DifficultyLevelScreen _difficultyLevelScreen;

		private SinglePlayerStartScreen _startScreen = new SinglePlayerStartScreen(false);

		private Screen _chooseAnotherGameScreen = new Screen(true, false);

		private ReleaseNotesScreen _releaseNotesScreen;

		private SpriteBatch SpriteBatch;

		public SpriteFont _largeFont;

		private PromoCode.PromoCodeManager _promoManager;

		private CheatCode.CheatCodeManager _cheatcodeManager;

		public WorldManager WorldManager;

		public ChooseOnlineGameScreen _chooseOnlineGameScreen;

		public PCDialogScreen _optimizeStorageDialog;

		private PCKeyboardInputScreen _playerNameInput;

		private PCKeyboardInputScreen _serverPasswordScreen;

		private PCDialogScreen _quitDialog;

		private WaitScreen optimizeStorageWaitScreen;

		private int CurrentWorldsCount;

		private bool Cancel;

		private int OriginalWorldsCount;

		private bool _localGame;

		private bool _hostGame;

		private bool _draw2DInventoryAtlas;

		private ProfilerPrimitiveBatch _primitiveBatch;

		private float _draw2DInventoryBackgroundBrightness;

		private string _versionString;

		private bool _flashDir;

		private OneShotTimer _flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private bool _scrollWheelSaved;

		private int _scrollWheelValue;

		private OneShotTimer textFlashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5), true);
	}
}
