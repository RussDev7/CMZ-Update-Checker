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
			MenuBackdropScreen backdrop = new MenuBackdropScreen(game);
			base.PushScreen(backdrop);
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
			ChooseOnlineGameScreen.OnlineGameMenuItem item = (ChooseOnlineGameScreen.OnlineGameMenuItem)this._chooseOnlineGameScreen.SelectedItem;
			this._chooseOnlineGameScreen.ShutdownHostDiscovery();
			this.JoinGame(item.NetworkSession, item.Password);
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
			GameDifficultyTypes item = (GameDifficultyTypes)e.MenuItem.Tag;
			this._game.Difficulty = item;
			this._uiGroup.PushScreen(this._chooseSavedWorldScreen);
		}

		private void _gameModeMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			if (e.MenuItem.Tag == null)
			{
				this._uiGroup.PopScreen();
				return;
			}
			GameModeTypes item = (GameModeTypes)e.MenuItem.Tag;
			this._game.GameMode = item;
			this._game.InfiniteResourceMode = false;
			this._game.Difficulty = GameDifficultyTypes.EASY;
			this._game.JoinGamePolicy = JoinGamePolicy.Anyone;
			if (this._localGame)
			{
				switch (item)
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
				switch (item)
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
			WorldTypeIDs previous = this._game.CurrentWorld._terrainVersion;
			this.WorldManager.TakeOwnership(this._game.CurrentWorld);
			this._game.CurrentWorld._terrainVersion = WorldTypeIDs.CastleMinerZ;
			if (previous != this._game.CurrentWorld._terrainVersion)
			{
				this._game.BeginLoadTerrain(this._game.CurrentWorld, true);
			}
			this.HostGame(this._localGame);
		}

		public void ShowUIDialog(string title, string message, bool drawbehind)
		{
			PCDialogScreen dialog = new PCDialogScreen(title, message, null, false, this._game.DialogScreenImage, this._game._myriadMed, drawbehind, this._game.ButtonFrame);
			dialog.UseDefaultValues();
			this._uiGroup.ShowPCDialogScreen(dialog, null);
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
					string password;
					bool cancelled;
					string errorString;
					if (this._serverPasswordScreen.OptionSelected != -1)
					{
						password = this._serverPasswordScreen.TextInput;
						cancelled = false;
						errorString = null;
					}
					else
					{
						password = "";
						cancelled = true;
						errorString = Strings.Action_was_cancelled;
					}
					callback(cancelled, password, errorString, context);
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
			Rectangle titleArea = Screen.Adjuster.ScreenRect;
			this._flashTimer.Update(e.GameTime.ElapsedGameTime);
			if (this._flashTimer.Expired)
			{
				this._flashTimer.Reset();
				this._flashDir = !this._flashDir;
			}
			float blender = (this._flashDir ? this._flashTimer.PercentComplete : (1f - this._flashTimer.PercentComplete));
			Color color = Color.Lerp(CMZColors.MenuGreen, Color.White, blender);
			Rectangle logoRect = new Rectangle(titleArea.Center.X - (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.Y / 2f), titleArea.Center.Y - (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y));
			this.SpriteBatch.Begin();
			this._game.Logo.Draw(this.SpriteBatch, logoRect, Color.White);
			string url = "www.CastleMinerZ.com";
			Vector2 size = this._game._medFont.MeasureString(url);
			this.SpriteBatch.DrawOutlinedText(this._game._medFont, url, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - size.X / 2f, (float)Screen.Adjuster.ScreenRect.Bottom - size.Y), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			size = this._largeFont.MeasureString(Strings.Start_Game);
			this.SpriteBatch.DrawOutlinedText(this._largeFont, Strings.Start_Game, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - size.X / 2f, (float)logoRect.Bottom), color, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
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
						DateTime time = DateTime.Now;
						if (Screen.CurrentGamer == null)
						{
							return;
						}
						this.SetupNewGamer(Screen.CurrentGamer, this._game.SaveDevice);
						TimeSpan loadingTime = DateTime.Now - time;
						if (Screen.CurrentGamer != null)
						{
							this._uiGroup.PushScreen(this._mainMenu);
							if (loadingTime > TimeSpan.FromSeconds(20.0))
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
			WorldManager _worldManager = this.WorldManager;
			if (_worldManager == null)
			{
				return;
			}
			WorldInfo[] worlds = _worldManager.GetWorlds();
			for (int i = 0; i < worlds.Length; i++)
			{
				if (Screen.CurrentGamer == null)
				{
					return;
				}
				string gamertag = Screen.CurrentGamer.Gamertag;
				if (worlds[i].OwnerGamerTag != gamertag)
				{
					_worldManager.Delete(worlds[i]);
					this.CurrentWorldsCount--;
				}
				if (this.Cancel)
				{
					break;
				}
			}
			int index = 0;
			while (WorldInfo.CorruptWorlds.Count > 0)
			{
				try
				{
					this._game.SaveDevice.DeleteDirectory(WorldInfo.CorruptWorlds[index]);
				}
				catch
				{
				}
				WorldInfo.CorruptWorlds.RemoveAt(index);
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
			Vector2 CancelSize = this._game._medFont.MeasureString(Strings.Press_Esc_to_Cancel);
			int ypos = (int)((float)Screen.Adjuster.ScreenRect.Height - CancelSize.Y);
			int xpos = (int)((float)Screen.Adjuster.ScreenRect.Width - CancelSize.X);
			this.SpriteBatch.Begin();
			this.SpriteBatch.DrawOutlinedText(this._game._medFont, Strings.Press_Esc_to_Cancel, new Vector2((float)xpos, (float)ypos), Color.White, Color.Black, 1);
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
			float progress;
			if (this.OriginalWorldsCount > 0)
			{
				progress = 1f - (float)this.CurrentWorldsCount / (float)this.OriginalWorldsCount;
			}
			else
			{
				progress = 1f;
			}
			this.optimizeStorageWaitScreen.Progress = (int)(100f * progress);
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
			string[] dirs = Directory.GetDirectories(path);
			foreach (string dir in dirs)
			{
				FrontEndScreen.GetFiles(dir, returnedFiles);
			}
			string[] files = Directory.GetFiles(path);
			foreach (string file in files)
			{
				returnedFiles.Add(file);
			}
		}

		private void UpdateSaves(string path, byte[] newKey)
		{
			string markFile = Path.Combine(path, "save.version");
			if (File.Exists(markFile))
			{
				return;
			}
			MD5HashProvider hasher = new MD5HashProvider();
			byte[] oldKey = hasher.Compute(Encoding.UTF8.GetBytes(Screen.CurrentGamer.Gamertag + "CMZ778")).Data;
			SaveDevice oldDevice = new FileSystemSaveDevice(path, oldKey);
			SaveDevice newDevice = new FileSystemSaveDevice(path, newKey);
			List<string> files = new List<string>();
			FrontEndScreen.GetFiles(path, files);
			foreach (string file in files)
			{
				byte[] data;
				try
				{
					data = oldDevice.LoadData(file);
				}
				catch
				{
					continue;
				}
				newDevice.Save(file, data, true, true);
			}
			using (FileStream stream = File.Open(markFile, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write("VER");
				writer.Write(1);
			}
		}

		private void SetupSaveDevice(SuccessCallback callback)
		{
			WaitScreen waitScreen = new WaitScreen(Strings.Opening_Storage_Device);
			this._uiGroup.PushScreen(waitScreen);
			this.CloseSaveDevice();
			ulong steamID = CastleMinerZGame.Instance.LicenseServices.SteamUserID;
			MD5HashProvider hasher = new MD5HashProvider();
			byte[] key = hasher.Compute(Encoding.UTF8.GetBytes(steamID.ToString() + "CMZ778")).Data;
			string dataPath = GlobalSettings.GetAppDataDirectory();
			string finalPath = Path.Combine(dataPath, steamID.ToString());
			try
			{
				string namedPath = Path.Combine(dataPath, Screen.CurrentGamer.Gamertag);
				if (Directory.Exists(namedPath))
				{
					Directory.Move(namedPath, finalPath);
				}
			}
			catch (Exception)
			{
			}
			this.UpdateSaves(finalPath, key);
			this._game.SaveDevice = new FileSystemSaveDevice(finalPath, key);
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
						return;
					}
					this._uiGroup.PopScreen();
					this.ShowUIDialog(Strings.Hosting_Error, Strings.There_was_an_error_hosting_the_game_, false);
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
			float progress = this._game.LoadProgress;
			string msg = Strings.Loading_The_World____ + Strings.Please_Wait___;
			float barWidth = (float)Screen.Adjuster.ScreenRect.Width * 0.8f;
			float leftStart = (float)Screen.Adjuster.ScreenRect.Left + ((float)Screen.Adjuster.ScreenRect.Width - barWidth) / 2f;
			Sprite bar = this._game._uiSprites["Bar"];
			Vector2 size = this._largeFont.MeasureString(msg);
			Vector2 position = new Vector2(leftStart, (float)(Screen.Adjuster.ScreenRect.Height / 2) + size.Y);
			float ypos = position.Y + (float)this._largeFont.LineSpacing + 10f * Screen.Adjuster.ScaleFactor.Y;
			Rectangle location = new Rectangle((int)leftStart, (int)ypos, (int)barWidth, this._largeFont.LineSpacing);
			int xloc = location.Left;
			int yloc = location.Top;
			float num = (float)location.Width / (float)bar.Width;
			this.SpriteBatch.Begin();
			int logoWidth = (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.X);
			int logoHeight = (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y);
			Rectangle logoRect = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - logoWidth / 2, 0, logoWidth, logoHeight);
			this._game.Logo.Draw(this.SpriteBatch, logoRect, Color.White);
			this.SpriteBatch.DrawOutlinedText(this._largeFont, msg, position, Color.White, Color.Black, 1);
			this.SpriteBatch.Draw(this._game.DummyTexture, new Rectangle(xloc - 2, yloc - 2, location.Width + 4, location.Height + 4), Color.White);
			this.SpriteBatch.Draw(this._game.DummyTexture, new Rectangle(xloc, yloc, location.Width, location.Height), Color.Black);
			int sourceWidth = (int)((float)bar.Width * progress);
			bar.Draw(this.SpriteBatch, new Rectangle(xloc, yloc, (int)((float)location.Width * progress), location.Height), new Rectangle(bar.Width - sourceWidth, 0, sourceWidth, bar.Height), Color.White);
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
			string msg = Strings.Connecting____ + Strings.Please_Wait___;
			Vector2 size = this._largeFont.MeasureString(msg);
			Vector2 position = new Vector2((float)(Screen.Adjuster.ScreenRect.Width / 2) - size.X / 2f, (float)(Screen.Adjuster.ScreenRect.Height / 2) + size.Y);
			this.textFlashTimer.Update(e.GameTime.ElapsedGameTime);
			Color currentColor = Color.Lerp(Color.Green, Color.White, this.textFlashTimer.PercentComplete);
			if (this.textFlashTimer.Expired)
			{
				this.textFlashTimer.Reset();
			}
			this.SpriteBatch.Begin();
			int logoWidth = (int)((float)this._game.Logo.Width * Screen.Adjuster.ScaleFactor.X);
			int logoHeight = (int)((float)this._game.Logo.Height * Screen.Adjuster.ScaleFactor.Y);
			Rectangle logoRect = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - logoWidth / 2, 0, logoWidth, logoHeight);
			this._game.Logo.Draw(this.SpriteBatch, logoRect, Color.White);
			this.SpriteBatch.DrawOutlinedText(this._largeFont, msg, position, currentColor, Color.Black, 1);
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
			string msg = Strings.Session_Ended;
			Vector2 size = this._largeFont.MeasureString(msg);
			int lineSpacing = this._largeFont.LineSpacing;
			this.SpriteBatch.DrawOutlinedText(this._largeFont, msg, new Vector2(640f - size.X / 2f, 360f - size.Y / 2f), Color.White, Color.Black, 2);
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
