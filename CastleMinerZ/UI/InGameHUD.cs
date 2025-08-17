using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Net.GamerServices;
using DNA.Text;
using DNA.Timers;
using DNA.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class InGameHUD : Screen
	{
		public PlayerInventory PlayerInventory
		{
			get
			{
				return this.LocalPlayer.PlayerInventory;
			}
		}

		public void ApplyDamage(float damageAmount, Vector3 damageSource)
		{
			if (!this.LocalPlayer.Dead)
			{
				this.InnaccuracyMultiplier = 1f;
				this.LocalPlayer.ApplyRecoil(Angle.FromDegrees(5f));
				this._damageIndicator.Add(new InGameHUD.DamageIndicator(damageSource));
				SoundManager.Instance.PlayInstance("Hit");
				this.HealthRecoverTimer.Reset();
				this.StaminaRecoverTimer.Reset();
				this.PlayerHealth -= damageAmount;
				if (this.PlayerHealth <= 0f)
				{
					this.PlayerHealth = 0f;
					this.KillPlayer();
				}
			}
		}

		public void UseStamina(float amount)
		{
			this.PlayerStamina -= amount;
			this.StaminaRecoverTimer.Reset();
			if ((double)this.PlayerStamina <= 0.01)
			{
				this.StaminaBlockTimer.Reset();
			}
		}

		public bool CanUseStamina(float amount)
		{
			return !CastleMinerZGame.Instance.IsEnduranceMode && this.StaminaBlockTimer.Expired && amount <= this.PlayerStamina;
		}

		public bool CheckAndUseStamina(float amount)
		{
			if (amount <= 0f)
			{
				return true;
			}
			if (this.CanUseStamina(amount))
			{
				this.UseStamina(amount);
				return true;
			}
			return false;
		}

		private bool WaitToRespawn
		{
			get
			{
				return this._game.GameMode == GameModeTypes.Endurance && !this.timeToRespawn.Expired && this.LocalPlayer.Dead && this._game.IsOnlineGame;
			}
		}

		public bool IsChatting
		{
			get
			{
				return this._game != null && this._game.GameScreen._uiGroup.CurrentScreen == this._chatScreen;
			}
		}

		public void KillPlayer()
		{
			this.LocalPlayer.PlayGrenadeAnim = false;
			this.LocalPlayer.ReadyToThrowGrenade = false;
			CrateFocusMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			SoundManager.Instance.PlayInstance("Fall");
			this.LocalPlayer.Dead = true;
			this.LocalPlayer.FPSMode = false;
			this.LocalPlayer.Avatar.HideHead = true;
			if (CastleMinerZGame.Instance.GameMode != GameModeTypes.Creative)
			{
				if (this._game.Difficulty == GameDifficultyTypes.HARDCORE)
				{
					this.PlayerInventory.DropAll(true);
					this.PlayerInventory.SetDefaultInventory();
				}
				else
				{
					this.PlayerInventory.DropAll(false);
				}
			}
			this.timeToRespawn = new OneShotTimer(TimeSpan.FromSeconds(20.0));
			this.timeToShowRespawnText = new OneShotTimer(TimeSpan.FromSeconds(3.0));
			if (this._game.IsOnlineGame)
			{
				BroadcastTextMessage.Send(this._game.MyNetworkGamer, this.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Fallen);
			}
		}

		public void RespawnPlayer()
		{
			Player player = null;
			float num = float.MaxValue;
			foreach (NetworkGamer networkGamer in this._game.CurrentNetworkSession.RemoteGamers)
			{
				if (networkGamer.Tag != null)
				{
					Player player2 = (Player)networkGamer.Tag;
					if (player2 != null && !player2.Dead)
					{
						float num2 = player2.LocalPosition.LengthSquared();
						if (num2 < num)
						{
							player = player2;
							num = num2;
						}
					}
				}
			}
			if (this._game.GameMode != GameModeTypes.Endurance)
			{
				this.RefreshPlayer();
				this._game.GameScreen.TeleportToLocation(this.LocalPlayer.GetSpawnPoint(), false);
				if (this._game.IsOnlineGame)
				{
					BroadcastTextMessage.Send(this._game.MyNetworkGamer, this.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Respawned);
					return;
				}
			}
			else if (player == null)
			{
				if (this._game.LocalPlayer.Gamer.IsHost && !this._game.IsOnlineGame)
				{
					RestartLevelMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer);
					return;
				}
			}
			else
			{
				this.RefreshPlayer();
				this._game.GameScreen.TeleportToLocation(player.LocalPosition, false);
				if (this._game.IsOnlineGame)
				{
					BroadcastTextMessage.Send(this._game.MyNetworkGamer, this.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Respawned);
				}
			}
		}

		public bool AllPlayersDead()
		{
			Player player = null;
			foreach (NetworkGamer networkGamer in this._game.CurrentNetworkSession.RemoteGamers)
			{
				if (networkGamer.Tag != null)
				{
					Player player2 = (Player)networkGamer.Tag;
					if (player2 != null && !player2.Dead)
					{
						player = player2;
						break;
					}
				}
			}
			return player == null;
		}

		public void RefreshPlayer()
		{
			this.LocalPlayer.Dead = false;
			this.LocalPlayer.FPSMode = true;
			this.PlayerHealth = 1f;
		}

		public InventoryItem ActiveInventoryItem
		{
			get
			{
				return this.PlayerInventory.ActiveInventoryItem;
			}
		}

		public static BlockTypeEnum GetBlock(IntVector3 worldPosition)
		{
			return BlockTerrain.Instance.GetBlockWithChanges(worldPosition);
		}

		public Player LocalPlayer
		{
			get
			{
				return this._game.LocalPlayer;
			}
		}

		private BlockTerrain Terrain
		{
			get
			{
				return this._game._terrain;
			}
		}

		public InGameHUD(CastleMinerZGame game)
			: base(true, false)
		{
			this.CaptureMouse = true;
			this.ShowMouseCursor = false;
			this._triggers.Add(new TransitionMusicTrigger("Song6", 3400f));
			this._triggers.Add(new TransitionMusicTrigger("Song5", 3000f));
			this._triggers.Add(new TransitionMusicTrigger("Song4", 2300f));
			this._triggers.Add(new TransitionMusicTrigger("Song3", 1600f));
			this._triggers.Add(new TransitionMusicTrigger("Song2", 900f));
			this._triggers.Add(new TransitionMusicTrigger("Song1", 200f));
			this._craterFoundTrigger = new CraterFoundTransitionMusicTrigger("SpaceTheme", 1f);
			InGameHUD.Instance = this;
			this._game = game;
			this._damageArrow = this._game._uiSprites["DamageArrow"];
			this._gridSprite = this._game._uiSprites["HudGrid"];
			this._selectorSprite = this._game._uiSprites["Selector"];
			this._crosshair = this._game._uiSprites["CrossHair"];
			this._crosshairTick = this._game._uiSprites["CrossHairTick"];
			this._emptyStaminaBar = this._game._uiSprites["StaminaBarEmpty"];
			this._emptyHealthBar = this._game._uiSprites["HealthBarEmpty"];
			this._fullHealthBar = this._game._uiSprites["HealthBarFull"];
			this._bubbleBar = this._game._uiSprites["BubbleBar"];
			this._sniperScope = this._game._uiSprites["SniperScope"];
			this._missileLocking = this._game._uiSprites["MissleLocking"];
			this._missileLock = this._game._uiSprites["MissleLock"];
			this.console = new ConsoleElement(this._game._consoleFont);
			this.console.GrabConsole();
			this.console.Location = Vector2.Zero;
			this.console.Size = new Vector2((float)this._game.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.3f, (float)this._game.GraphicsDevice.PresentationParameters.BackBufferHeight * 0.25f);
			this._chatScreen = new PlainChatInputScreen(this.console.Bounds.Bottom + 25f);
			this.timeToLightning = new OneShotTimer(TimeSpan.FromSeconds((double)this.rand.Next(5, 10)));
			this.timeToThunder = new OneShotTimer(TimeSpan.FromSeconds((double)((float)this.rand.NextDouble() * 2f)));
			this.lightningFlashCount = this.rand.Next(0, 4);
			this._travelMaxDialog = new PCDialogScreen(Strings.Purchase_Game, Strings.You_must_purchase_the_game_to_travel_further, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._travelMaxDialog.UseDefaultValues();
			this._crateScreen = new CrateScreen(game, this);
		}

		public void DisplayAcheivement(AchievementManager<CastleMinerZPlayerStats>.Achievement acheivement)
		{
			this.AcheivementsToDraw.Enqueue(acheivement);
		}

		private void DrawAcheivement(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			Sprite sprite = this._game._uiSprites["AwardEnd"];
			Sprite sprite2 = this._game._uiSprites["AwardCenter"];
			Sprite sprite3 = this._game._uiSprites["AwardCircle"];
			float num = (float)sprite.Width;
			Vector2 vector = new Vector2(79f, 10f);
			Vector2 vector2 = new Vector2(79f, 37f);
			float num2 = vector.X - num;
			Vector2 vector3 = this._game._systemFont.MeasureString(this._achievementText1);
			Vector2 vector4 = this._game._systemFont.MeasureString(this._achievementText2);
			float num3 = Math.Max(vector3.X, vector4.X) + num2;
			float num4 = num3 + num * 2f;
			float num5 = (float)screenRect.Center.X - num4 / 2f;
			int num6 = (int)this.acheimentDisplayLocation.Y;
			this._achievementLocation = new Vector2(num5, (float)num6);
			sprite.Draw(spriteBatch, new Vector2(num5, (float)num6), Color.White);
			sprite.Draw(spriteBatch, new Vector2(num5 + num3 + num, (float)num6), 1f, Color.White, SpriteEffects.FlipHorizontally);
			sprite2.Draw(spriteBatch, new Rectangle((int)(num5 + num) - 1, num6, (int)(num3 + 2f), sprite2.Height), Color.White);
			sprite3.Draw(spriteBatch, new Vector2(num5, (float)num6), Color.White);
			spriteBatch.DrawString(this._game._systemFont, this._achievementText1, vector + this._achievementLocation, new Color(219, 219, 219));
			spriteBatch.DrawString(this._game._systemFont, this._achievementText2, vector2 + this._achievementLocation, new Color(219, 219, 219));
		}

		private void EquipActiveItem()
		{
			if (this.lastItem == this.ActiveInventoryItem)
			{
				return;
			}
			if (this.lastItem != null)
			{
				this.lastItem.ItemClass.OnItemUnequipped();
			}
			this.ActiveInventoryItem.ItemClass.OnItemEquipped();
			this.lastItem = this.ActiveInventoryItem;
			this.LocalPlayer.Equip(this.ActiveInventoryItem);
			if (this.ActiveInventoryItem is GunInventoryItem)
			{
				GunInventoryItem gunInventoryItem = (GunInventoryItem)this.ActiveInventoryItem;
				this.LocalPlayer.ReloadSound = gunInventoryItem.GunClass.ReloadSound;
			}
		}

		private void UpdateAcheivements(GameTime gameTime)
		{
			this._game.AcheivmentManager.Update();
			if (this.displayedAcheivement == null)
			{
				if (this.AcheivementsToDraw.Count > 0)
				{
					SoundManager.Instance.PlayInstance("Award");
					this.acheivementDisplayTimer.Reset();
					this.displayedAcheivement = this.AcheivementsToDraw.Dequeue();
					BroadcastTextMessage.Send(this._game.MyNetworkGamer, string.Concat(new string[]
					{
						this.LocalPlayer.Gamer.Gamertag,
						" ",
						Strings.Has_earned,
						" '",
						this.displayedAcheivement.Name,
						"'"
					}));
					this._achievementText2 = this.displayedAcheivement.HowToUnlock;
					this._achievementText1 = this.displayedAcheivement.Name;
					return;
				}
			}
			else
			{
				this.acheivementDisplayTimer.Update(gameTime.ElapsedGameTime);
				if (this.acheivementDisplayTimer.Expired)
				{
					this.displayedAcheivement = null;
				}
			}
		}

		private void DrawAcheivements(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (this.displayedAcheivement != null)
			{
				this.DrawAcheivement(device, spriteBatch);
			}
		}

		private float GetTrayAlphaSetting()
		{
			if (CastleMinerZGame.Instance.PlayerStats.SecondTrayFaded)
			{
				return this._secondTrayAlpha;
			}
			return 1f;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (this._hideUI)
			{
				return;
			}
			Rectangle rectangle = new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height);
			spriteBatch.Begin();
			if (!this.LocalPlayer.Dead && this.LocalPlayer.ShoulderedAnimState)
			{
				GunInventoryItemClass gunInventoryItemClass = this.ActiveInventoryItem.ItemClass as GunInventoryItemClass;
				if (gunInventoryItemClass != null && gunInventoryItemClass.Scoped)
				{
					this.LocalPlayer.Avatar.Visible = false;
					if (gunInventoryItemClass is RocketLauncherGuidedInventoryItemClass)
					{
						RocketLauncherGuidedInventoryItemClass rocketLauncherGuidedInventoryItemClass = (RocketLauncherGuidedInventoryItemClass)gunInventoryItemClass;
						if (rocketLauncherGuidedInventoryItemClass.LockedOnToDragon)
						{
							spriteBatch.Draw(this._missileLock, rocketLauncherGuidedInventoryItemClass.LockedOnSpriteLocation, Color.Red);
						}
						else
						{
							spriteBatch.Draw(this._missileLocking, rocketLauncherGuidedInventoryItemClass.LockedOnSpriteLocation, Color.Lime);
						}
					}
					Size size = new Size((int)((float)this._sniperScope.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._sniperScope.Height * Screen.Adjuster.ScaleFactor.Y));
					Vector2 vector = new Vector2((float)(rectangle.Center.X - size.Width / 2), (float)(rectangle.Center.Y - size.Height / 2));
					spriteBatch.Draw(this._sniperScope, new Rectangle((int)vector.X, (int)vector.Y, size.Width, size.Height), Color.White);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, (int)vector.Y), Color.Black);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, (int)vector.Y + size.Height, Screen.Adjuster.ScreenRect.Width, (int)vector.Y), Color.Black);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, (int)vector.Y, (int)vector.X, size.Height), Color.Black);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(Screen.Adjuster.ScreenRect.Width - (int)vector.X, (int)vector.Y, (int)vector.X, size.Height), Color.Black);
				}
				else
				{
					this.LocalPlayer.Avatar.Visible = true;
				}
			}
			else
			{
				this.LocalPlayer.Avatar.Visible = true;
			}
			spriteBatch.End();
			if (rectangle != this._prevTitleSafe)
			{
				this.console.Location = new Vector2((float)rectangle.Left, (float)rectangle.Top);
				this._chatScreen.YLoc = this.console.Bounds.Bottom + 25f;
			}
			this.console.Draw(device, spriteBatch, gameTime, false);
			if (this.showPlayers)
			{
				this.DrawPlayerList(device, spriteBatch, gameTime);
			}
			else
			{
				spriteBatch.Begin();
				int num = (int)((1f - this.PlayerHealth) * 120f);
				int num2 = (int)((1f - this.PlayerHealth) * 102f);
				if (num > 0)
				{
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), new Color(num2, 0, 0, num));
				}
				if (this.LocalPlayer.PercentSubmergedLava > 0f)
				{
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), Color.Lerp(Color.Transparent, Color.Red, this.LocalPlayer.PercentSubmergedLava));
				}
				if (this.LocalPlayer.UnderLava)
				{
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), Color.Red);
				}
				Rectangle rectangle2 = new Rectangle(0, 0, this._damageArrow.Width, this._damageArrow.Height);
				Vector2 vector2 = new Vector2((float)(this._damageArrow.Width / 2), (float)(this._damageArrow.Height + 150));
				Vector2 vector3 = new Vector2((float)rectangle.Center.X, (float)rectangle.Center.Y);
				int i = 0;
				while (i < this._damageIndicator.Count)
				{
					this._damageIndicator[i].drawTimer.Update(gameTime.ElapsedGameTime);
					if (this._damageIndicator[i].drawTimer.Expired)
					{
						this._damageIndicator.RemoveAt(i);
					}
					else
					{
						Vector3 vector4 = this._damageIndicator[i].DamageSource - this.LocalPlayer.LocalPosition;
						vector4 = Vector3.TransformNormal(vector4, this.LocalPlayer.WorldToLocal);
						Angle angle = Angle.ATan2((double)vector4.X, (double)(-(double)vector4.Z));
						this._damageArrow.Draw(spriteBatch, vector3, rectangle2, new Color((int)(139f * this._damageIndicator[i].fadeAmount), 0, 0, (int)(255f * this._damageIndicator[i].fadeAmount)), angle, vector2, 0.75f, SpriteEffects.None, 0f);
						i++;
					}
				}
				if (this.LocalPlayer.Dead && this.timeToShowRespawnText.Expired && this._game.GameScreen._uiGroup.CurrentScreen == this)
				{
					string text = "";
					string text2 = Strings.Click_To_Respawn;
					if (this.WaitToRespawn)
					{
						text = Strings.Respawn_In + ": ";
						text2 = "";
					}
					if (this._game.IsOnlineGame && this.AllPlayersDead() && this._game.GameMode == GameModeTypes.Endurance)
					{
						if (this._game.CurrentNetworkSession.IsHost)
						{
							text = "";
							text2 = Strings.Click_To_Restart;
						}
						else
						{
							text = Strings.Waiting_For_Host_To_Restart;
							text2 = "";
						}
					}
					Vector2 vector5 = this._game._medLargeFont.MeasureString(text + text2);
					Vector2 vector6 = new Vector2((float)rectangle.Center.X - vector5.X / 2f, (float)rectangle.Center.Y - vector5.Y / 2f);
					if (this._game.GameMode == GameModeTypes.Endurance)
					{
						this.sbuilder.Length = 0;
						this.sbuilder.Append(Strings.Distance_Traveled);
						this.sbuilder.Append(": ");
						this.sbuilder.Concat(this.maxDistanceTraveled);
						Vector2 vector7 = this._game._medLargeFont.MeasureString(this.sbuilder);
						vector6 = new Vector2((float)rectangle.Center.X - vector7.X / 2f, (float)rectangle.Center.Y - vector7.Y - vector7.Y / 2f);
						spriteBatch.DrawOutlinedText(this._game._medLargeFont, this.sbuilder, new Vector2(vector6.X, vector6.Y), Color.White, Color.Black, 2);
						this.sbuilder.Length = 0;
						this.sbuilder.Append(" " + Strings.In + " ");
						this.sbuilder.Concat(this.currentDay);
						this.sbuilder.Append((this.currentDay != 1) ? (" " + Strings.Days) : (" " + Strings.Day));
						vector7 = this._game._medLargeFont.MeasureString(this.sbuilder);
						vector6 = new Vector2((float)rectangle.Center.X - vector7.X / 2f, (float)rectangle.Center.Y - vector7.Y / 2f);
						spriteBatch.DrawOutlinedText(this._game._medLargeFont, this.sbuilder, new Vector2(vector6.X, vector6.Y), Color.White, Color.Black, 2);
						vector6 = new Vector2((float)rectangle.Center.X - vector5.X / 2f, (float)rectangle.Center.Y - vector5.Y / 2f + vector7.Y);
					}
					spriteBatch.DrawOutlinedText(this._game._medLargeFont, text, new Vector2(vector6.X, vector6.Y), Color.White, Color.Black, 2);
					vector6.X += this._game._medLargeFont.MeasureString(text).X;
					if (!this.WaitToRespawn || (this.AllPlayersDead() && this._game.CurrentNetworkSession.IsHost && this._game.GameMode == GameModeTypes.Endurance))
					{
						if (this._game.GameMode != GameModeTypes.Endurance || !this.AllPlayersDead() || this._game.CurrentNetworkSession.IsHost)
						{
							spriteBatch.DrawOutlinedText(this._game._medLargeFont, text2, new Vector2(vector6.X, vector6.Y), Color.White, Color.Black, 2);
						}
					}
					else if (this._game.IsOnlineGame && !this.AllPlayersDead() && this._game.GameMode == GameModeTypes.Endurance)
					{
						this.sbuilder.Length = 0;
						this.sbuilder.Concat((int)(21.0 - this.timeToRespawn.ElaspedTime.TotalSeconds));
						spriteBatch.DrawOutlinedText(this._game._medLargeFont, this.sbuilder, new Vector2(vector6.X, vector6.Y), Color.White, Color.Black, 2);
					}
				}
				this.DrawDistanceStr(spriteBatch);
				if (this.ConstructionProbe.AbleToBuild && this.PlayerInventory.ActiveInventoryItem != null)
				{
					BlockTypeEnum block = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
					BlockType type = BlockType.GetType(block);
					spriteBatch.DrawString(this._game._medFont, type.Name, new Vector2((float)rectangle.Right - (this._game._medFont.MeasureString(type.Name).X + 10f) * Screen.Adjuster.ScaleFactor.Y, (float)(this._game._medFont.LineSpacing * 4) * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
				}
				Size size2 = new Size((int)((float)this._gridSprite.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._gridSprite.Height * Screen.Adjuster.ScaleFactor.Y));
				Rectangle rectangle3 = new Rectangle(rectangle.Center.X - size2.Width / 2, rectangle.Bottom - size2.Height - (int)(5f * Screen.Adjuster.ScaleFactor.Y), size2.Width, size2.Height);
				this._gridSprite.Draw(spriteBatch, rectangle3, new Color(1f, 1f, 1f, this.GetTrayAlphaSetting()));
				Vector2 vector8 = new Vector2(-35f * Screen.Adjuster.ScaleFactor.Y, -68f * Screen.Adjuster.ScaleFactor.Y);
				Rectangle rectangle4 = rectangle3;
				rectangle3.X += (int)vector8.X;
				rectangle3.Y += (int)vector8.Y;
				this._gridSprite.Draw(spriteBatch, rectangle3, Color.White);
				this.DrawPlayerStats(spriteBatch);
				if (this.LocalPlayer.Underwater)
				{
					float num3 = this.PlayerOxygen / 1f;
					Vector2 vector9 = new Vector2((float)rectangle3.Center.X, (float)(rectangle3.Top - 30));
					this._bubbleBar.Draw(spriteBatch, new Rectangle((int)(vector9.X + (float)this._bubbleBar.Width * (1f - num3)), (int)vector9.Y, (int)((float)this._bubbleBar.Width * num3), this._bubbleBar.Height), new Rectangle((int)((float)this._bubbleBar.Width * (1f - num3)), 0, (int)((float)this._bubbleBar.Width * num3), this._bubbleBar.Height), Color.White);
				}
				int num4 = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
				for (int j = 0; j < this.PlayerInventory.TrayManager.CurrentTrayLength; j++)
				{
					InventoryItem itemFromCurrentTray = this.PlayerInventory.TrayManager.GetItemFromCurrentTray(j);
					if (itemFromCurrentTray != null)
					{
						int num5 = (int)(59f * Screen.Adjuster.ScaleFactor.Y * (float)j + (float)rectangle3.Left + 2f * Screen.Adjuster.ScaleFactor.Y);
						itemFromCurrentTray.Draw2D(spriteBatch, new Rectangle(num5, rectangle3.Top + (int)(2f * Screen.Adjuster.ScaleFactor.Y), num4, num4));
					}
				}
				for (int k = 0; k < this.PlayerInventory.TrayManager.CurrentTrayLength; k++)
				{
					InventoryItem itemFromNextTray = this.PlayerInventory.TrayManager.GetItemFromNextTray(k);
					if (itemFromNextTray != null)
					{
						int num6 = (int)(59f * Screen.Adjuster.ScaleFactor.Y * (float)k + (float)rectangle4.Left + 2f * Screen.Adjuster.ScaleFactor.Y);
						itemFromNextTray.Draw2D(spriteBatch, new Rectangle(num6, rectangle4.Top + (int)(2f * Screen.Adjuster.ScaleFactor.Y), num4, num4), new Color(1f, 1f, 1f, this.GetTrayAlphaSetting()), true);
					}
				}
				Rectangle rectangle5 = new Rectangle(rectangle3.Left + (int)(7f * Screen.Adjuster.ScaleFactor.Y + 59f * Screen.Adjuster.ScaleFactor.Y * (float)this.PlayerInventory.SelectedInventoryIndex), (int)((float)rectangle3.Top + 7f * Screen.Adjuster.ScaleFactor.Y), num4, num4);
				this._selectorSprite.Draw(spriteBatch, rectangle5, Color.White);
				this.sbuilder.Length = 0;
				if (this.ActiveInventoryItem != null)
				{
					Vector2 vector10 = this._game._medFont.MeasureString(this.ActiveInventoryItem.Name);
					this.ActiveInventoryItem.GetDisplayText(this.sbuilder);
					spriteBatch.DrawString(this._game._medFont, this.sbuilder, new Vector2((float)rectangle3.Left, (float)rectangle3.Y - vector10.Y * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
				}
				if (!this.LocalPlayer.Dead && !this.LocalPlayer.Shouldering)
				{
					GunInventoryItem gunInventoryItem = this.ActiveInventoryItem as GunInventoryItem;
					Angle fieldOfView = this.LocalPlayer.FPSCamera.FieldOfView;
					Angle angle2 = Angle.FromDegrees(0.5f);
					if (gunInventoryItem != null)
					{
						if (this.LocalPlayer.Shouldering && !this.LocalPlayer.Reloading)
						{
							Angle angle3 = Angle.Lerp(gunInventoryItem.GunClass.MinInnaccuracy, gunInventoryItem.GunClass.ShoulderedMinAccuracy, this.LocalPlayer.Avatar.Animations[2].Progress);
							angle2 = angle3 + this.InnaccuracyMultiplier * (gunInventoryItem.GunClass.ShoulderedMaxAccuracy - angle3);
						}
						else
						{
							angle2 = gunInventoryItem.GunClass.MinInnaccuracy + this.InnaccuracyMultiplier * (gunInventoryItem.GunClass.MaxInnaccuracy - gunInventoryItem.GunClass.MinInnaccuracy);
						}
					}
					this._crosshairTickDrawLocation = angle2 / fieldOfView * (float)Screen.Adjuster.ScreenRect.Width;
					float num7 = Math.Max(1f, Screen.Adjuster.ScaleFactor.Y);
					Color color = new Color((1f - this.InnaccuracyMultiplier) / 2f + 0.5f, (1f - this.InnaccuracyMultiplier) / 2f + 0.5f, (1f - this.InnaccuracyMultiplier) / 2f + 0.5f, (1f - this.InnaccuracyMultiplier) / 2f + 0.5f);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)rectangle.Center.X + this._crosshairTickDrawLocation)), (float)((int)((float)rectangle.Center.Y - 1f * num7))), num7, color);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)rectangle.Center.X - (9f * num7 + this._crosshairTickDrawLocation))), (float)((int)((float)rectangle.Center.Y - 1f * num7))), num7, color);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)rectangle.Center.X + 1f * num7)), (float)((int)((float)rectangle.Center.Y - 8f * num7 - this._crosshairTickDrawLocation))), color, Angle.FromDegrees(90f), Vector2.Zero, num7, SpriteEffects.None, 0f);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)rectangle.Center.X + 1f * num7)), (float)((int)((float)rectangle.Center.Y + this._crosshairTickDrawLocation + 1f * num7))), color, Angle.FromDegrees(90f), Vector2.Zero, num7, SpriteEffects.None, 0f);
				}
				if (!this.fadeInGameStart.Expired)
				{
					float num8 = (float)this.fadeInGameStart.ElaspedTime.TotalSeconds;
					num8 = num8 * 1f - num8;
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), new Color(num8, num8, num8, 1f - (float)this.fadeInGameStart.ElaspedTime.TotalSeconds));
				}
				if (!this.drawDayTimer.Expired && !this.LocalPlayer.Dead)
				{
					float num9 = 1f;
					float num10 = 0.33333334f;
					if (this.drawDayTimer.ElaspedTime < TimeSpan.FromSeconds(3.0))
					{
						num9 = this.drawDayTimer.PercentComplete / num10;
					}
					else if (this.drawDayTimer.ElaspedTime > TimeSpan.FromSeconds(6.0))
					{
						num9 = 1f - (this.drawDayTimer.PercentComplete - num10 * 2f) / num10;
					}
					this.sbuilder.Length = 0;
					this.sbuilder.Append(Strings.Day + " ");
					this.sbuilder.Concat(this.currentDay);
					spriteBatch.DrawString(this._game._largeFont, this.sbuilder, new Vector2((float)rectangle.Left, (float)rectangle.Bottom - this._game._largeFont.MeasureString(this.sbuilder).Y), CMZColors.MenuAqua * 0.75f * num9);
				}
				this.DrawAcheivements(device, spriteBatch, gameTime);
				spriteBatch.End();
			}
			this._prevTitleSafe = rectangle;
			base.OnDraw(device, spriteBatch, gameTime);
		}

		public void DrawDistanceStr(SpriteBatch spriteBatch)
		{
			this.distanceBuilder.Length = 0;
			this.distanceBuilder.Concat(this.currentDistanceTraveled);
			this.distanceBuilder.Append("-");
			this.distanceBuilder.Concat(this.maxDistanceTraveled);
			this.distanceBuilder.Append(" ");
			this.distanceBuilder.Append(Strings.Max);
			spriteBatch.DrawString(this._game._medFont, Strings.Distance, new Vector2((float)Screen.Adjuster.ScreenRect.Right - (this._game._medFont.MeasureString(Strings.Distance).X + 10f) * Screen.Adjuster.ScaleFactor.Y, (float)Screen.Adjuster.ScreenRect.Top), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			spriteBatch.DrawString(this._game._medFont, this.distanceBuilder, new Vector2((float)Screen.Adjuster.ScreenRect.Right - (this._game._medFont.MeasureString(this.distanceBuilder).X + 10f) * Screen.Adjuster.ScaleFactor.Y, (float)Screen.Adjuster.ScreenRect.Top + (float)this._game._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
		}

		public void DrawPlayerStats(SpriteBatch spriteBatch)
		{
			if (CastleMinerZGame.Instance.IsEnduranceMode)
			{
				this.DrawHealthBar(spriteBatch, true);
				return;
			}
			this.DrawHealthBar(spriteBatch, false);
			this.DrawStaminaBar(spriteBatch);
		}

		public void DrawHealthBar(SpriteBatch spriteBatch, bool isCenterScreen)
		{
			Color dodgerBlue = Color.DodgerBlue;
			Rectangle rectangle;
			if (isCenterScreen)
			{
				rectangle = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - (float)this._emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Height * Screen.Adjuster.ScaleFactor.Y));
			}
			else
			{
				rectangle = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - (float)this._emptyHealthBar.Width * 2.2f * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Height * Screen.Adjuster.ScaleFactor.Y));
			}
			float num = this.PlayerHealth / 1f;
			this.DrawResourceBar(spriteBatch, this._emptyHealthBar, dodgerBlue, num, rectangle);
		}

		public void DrawStaminaBar(SpriteBatch spriteBatch)
		{
			Color color = Color.Yellow;
			Rectangle rectangle = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X + (float)this._emptyStaminaBar.Width * 0.2f * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyStaminaBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyStaminaBar.Height * Screen.Adjuster.ScaleFactor.Y));
			float num = this.PlayerStamina / 1f;
			if (!this.StaminaBlockTimer.Expired)
			{
				color = Color.OrangeRed;
			}
			this.DrawResourceBar(spriteBatch, this._emptyStaminaBar, color, num, rectangle);
		}

		public void DrawResourceBar(SpriteBatch spriteBatch, Sprite resourceIcon, Color barColor, float ratio, Rectangle barLocation)
		{
			resourceIcon.Draw(spriteBatch, barLocation, barColor);
			barLocation = new Rectangle(barLocation.X + (int)(56f * Screen.Adjuster.ScaleFactor.Y), barLocation.Y, (int)((float)this._fullHealthBar.Width * Screen.Adjuster.ScaleFactor.Y * ratio), (int)((float)this._fullHealthBar.Height * Screen.Adjuster.ScaleFactor.Y));
			spriteBatch.Draw(this._fullHealthBar, new Vector2((float)barLocation.X, (float)barLocation.Y), new Rectangle(0, 0, (int)((float)this._fullHealthBar.Width * ratio), this._fullHealthBar.Height), barColor, Angle.Zero, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			this.sbuilder.Clear();
			this.sbuilder.Append(Math.Truncate((double)(ratio * 100f)));
			this.sbuilder.Append("%");
			spriteBatch.DrawString(this._game._medFont, this.sbuilder, new Vector2((float)barLocation.X, (float)barLocation.Y - 20f * Screen.Adjuster.ScaleFactor.Y), barColor, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
		}

		public bool DrawAbleToBuild()
		{
			IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(this.ConstructionProbe._worldIndex, this.ConstructionProbe._inFace);
			CastleMinerZGame.Instance.LocalPlayer.MovementProbe.SkipEmbedded = false;
			Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			worldPosition.Y += 0.05f;
			CastleMinerZGame.Instance.LocalPlayer.MovementProbe.Init(worldPosition, worldPosition, CastleMinerZGame.Instance.LocalPlayer.PlayerAABB);
			return !BlockTerrain.Instance.ProbeTouchesBlock(CastleMinerZGame.Instance.LocalPlayer.MovementProbe, neighborIndex);
		}

		protected void DoConstructionModeUpdate()
		{
			if (this.PlayerInventory.ActiveInventoryItem == null)
			{
				this.ConstructionProbe.Init(new Vector3(0f), new Vector3(1f), false);
			}
			else
			{
				Matrix localToWorld = this.LocalPlayer.FPSCamera.LocalToWorld;
				Vector3 translation = localToWorld.Translation;
				this.ConstructionProbe.Init(translation, Vector3.Add(translation, Vector3.Multiply(localToWorld.Forward, 5f)), this.PlayerInventory.ActiveInventoryItem.ItemClass.IsMeleeWeapon);
				this.ConstructionProbe.SkipEmbedded = true;
				this.ConstructionProbe.Trace();
			}
			if (!this.ConstructionProbe.AbleToBuild)
			{
				this._game.GameScreen.SelectorEntity.Visible = false;
				return;
			}
			if (this.PlayerInventory.ActiveInventoryItem.ItemClass is BlockInventoryItemClass && !this.DrawAbleToBuild())
			{
				this._game.GameScreen.SelectorEntity.Visible = false;
				return;
			}
			IntVector3 worldIndex = this.ConstructionProbe._worldIndex;
			Vector3 vector = worldIndex + new Vector3(0.5f, 0.5f, 0.5f);
			-this.ConstructionProbe._inNormal;
			Matrix matrix = Matrix.Identity;
			float num = 0.51f;
			switch (this.ConstructionProbe._inFace)
			{
			case BlockFace.POSX:
				matrix = Matrix.CreateWorld(vector + new Vector3(1f, 0f, 0f) * num, -Vector3.UnitY, Vector3.UnitX);
				break;
			case BlockFace.NEGZ:
				matrix = Matrix.CreateWorld(vector + new Vector3(0f, 0f, -1f) * num, -Vector3.UnitX, -Vector3.UnitZ);
				break;
			case BlockFace.NEGX:
				matrix = Matrix.CreateWorld(vector + new Vector3(-1f, 0f, 0f) * num, Vector3.UnitY, -Vector3.UnitX);
				break;
			case BlockFace.POSZ:
				matrix = Matrix.CreateWorld(vector + new Vector3(0f, 0f, 1f) * num, Vector3.UnitX, Vector3.UnitZ);
				break;
			case BlockFace.POSY:
				matrix = Matrix.CreateWorld(vector + new Vector3(0f, 1f, 0f) * num, Vector3.UnitX, Vector3.UnitY);
				break;
			case BlockFace.NEGY:
				matrix = Matrix.CreateWorld(vector + new Vector3(0f, -1f, 0f) * num, -Vector3.UnitX, -Vector3.UnitY);
				break;
			}
			this._game.GameScreen.CrackBox.LocalPosition = worldIndex + new Vector3(0.5f, -0.002f, 0.5f);
			this._game.GameScreen.SelectorEntity.LocalToParent = matrix;
			this._game.GameScreen.SelectorEntity.Visible = true;
		}

		public void Reset()
		{
			this.lastTOD = -1f;
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (CastleMinerZGame.Instance.GameScreen.DoUpdate)
			{
				this.LocalPlayer.UpdateGunEyePointCamera(this.GunEyePointCameraLocation);
				if (this.ActiveInventoryItem is GPSItem)
				{
					GPSItem gpsitem = (GPSItem)this.ActiveInventoryItem;
					this._game.GameScreen.GPSMarker.Visible = true;
					this._game.GameScreen.GPSMarker.LocalPosition = gpsitem.PointToLocation + new Vector3(0.5f, 1f, 0.5f);
					this._game.GameScreen.GPSMarker.color = gpsitem.color;
				}
				else
				{
					this._game.GameScreen.GPSMarker.Visible = false;
				}
				if (this.ActiveInventoryItem.ItemClass is RocketLauncherGuidedInventoryItemClass)
				{
					RocketLauncherGuidedInventoryItemClass rocketLauncherGuidedInventoryItemClass = (RocketLauncherGuidedInventoryItemClass)this.ActiveInventoryItem.ItemClass;
					rocketLauncherGuidedInventoryItemClass.CheckIfLocked(gameTime.ElapsedGameTime);
				}
				for (int i = 0; i < this._tntWaitingToExplode.Count; i++)
				{
					this._tntWaitingToExplode[i].Update(gameTime.ElapsedGameTime);
					if (this._tntWaitingToExplode[i].Timer.Expired)
					{
						this._tntWaitingToExplode.RemoveAt(i);
						i--;
					}
				}
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(this.ActiveInventoryItem.ItemClass.ID);
				itemStats.TimeHeld += gameTime.ElapsedGameTime;
				if (!this.fadeInGameStart.Expired)
				{
					this.fadeInGameStart.Update(gameTime.ElapsedGameTime);
				}
				this.drawDayTimer.Update(gameTime.ElapsedGameTime);
				if (this.lastTOD < 0.4f && this._game.GameScreen.TimeOfDay > 0.4f)
				{
					this.currentDay = (int)this._game.GameScreen.Day + 1;
					if (this._game.GameMode == GameModeTypes.Endurance && this.currentDay > 1)
					{
						CastleMinerZGame.Instance.PlayerStats.MaxDaysSurvived++;
					}
					SoundManager.Instance.PlayInstance("HorrorStinger");
					this.drawDayTimer.Reset();
				}
				this.lastTOD = this._game.GameScreen.TimeOfDay;
				for (int j = 0; j < this._triggers.Count; j++)
				{
					this._triggers[j].Update();
				}
				this._craterFoundTrigger.Update();
				if (this._craterFoundTrigger.Triggered)
				{
					this._resetCraterFoundTriggerTimer.Update(gameTime.ElapsedGameTime);
					if (this._resetCraterFoundTriggerTimer.Expired)
					{
						this._resetCraterFoundTriggerTimer.Reset();
						if (BlockTerrain.Instance.DepthUnderSpaceRock(this.LocalPlayer.LocalPosition) == 0)
						{
							this._craterFoundTrigger.Reset();
						}
					}
				}
				if (this.LocalPlayer.Dead && !this.timeToShowRespawnText.Expired)
				{
					this.timeToShowRespawnText.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
				}
				else if (this.WaitToRespawn)
				{
					this.timeToRespawn.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
				}
				this.EquipActiveItem();
				Vector2 vector = new Vector2(0f, 0f);
				Vector2 vector2 = new Vector2(this._game.LocalPlayer.LocalPosition.X, this._game.LocalPlayer.LocalPosition.Z);
				this.currentDistanceTraveled = (int)Vector2.Distance(vector, vector2);
				if (CastleMinerZGame.TrialMode)
				{
					if (this.currentDistanceTraveled <= 300)
					{
						this.trialMaxPosition = this.LocalPlayer.LocalPosition;
					}
					else if (this.currentDistanceTraveled > 301)
					{
						this.LocalPlayer.LocalPosition = this.trialMaxPosition;
						this._game.GameScreen._uiGroup.ShowPCDialogScreen(this._travelMaxDialog, delegate
						{
							if (this._travelMaxDialog.OptionSelected != -1)
							{
								Process.Start("http://www.digitaldnagames.com/Buy/CastleMinerZ.aspx");
							}
						});
					}
				}
				if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
				{
					CastleMinerZGame.Instance.PlayerStats.MaxDistanceTraveled = Math.Max(CastleMinerZGame.Instance.PlayerStats.MaxDistanceTraveled, (float)this.currentDistanceTraveled);
					CastleMinerZGame.Instance.PlayerStats.MaxDepth = Math.Min(CastleMinerZGame.Instance.PlayerStats.MaxDepth, this._game.LocalPlayer.LocalPosition.Y);
				}
				if (!this.gameBegun && this._game.GameMode == GameModeTypes.Endurance && this._game.IsOnlineGame)
				{
					if (this._game.CurrentNetworkSession.IsHost)
					{
						if (this.maxDistanceTraveled >= 100 || this.currentDay > 1)
						{
							this._game.CurrentNetworkSession.SessionProperties[1] = new int?(1);
							if (this._game.IsOnlineGame)
							{
								this._game.CurrentNetworkSession.UpdateHostSession(null, null, null, this._game.CurrentNetworkSession.SessionProperties);
							}
							this.gameBegun = true;
							Console.WriteLine(Strings.The_Game_Has_Begun___No_Other_Players_Can_Join);
						}
					}
					else if (this._game.CurrentNetworkSession.SessionProperties[1] == 1)
					{
						this.gameBegun = true;
						Console.WriteLine(Strings.The_Game_Has_Begun___No_Other_Players_Can_Join);
					}
				}
				if (this.currentDistanceTraveled > this.maxDistanceTraveled)
				{
					this.maxDistanceTraveled = this.currentDistanceTraveled;
				}
				Vector3 vector3 = new Vector3(0f, 0f, 1f);
				Vector3 vector4 = new Vector3(vector2.X, 0f, vector2.Y);
				this.compassRotation = vector3.AngleBetween(vector4);
				if (this.LocalPlayer.InLava)
				{
					if (!this.lavaSoundPlayed)
					{
						this.lavaSoundPlayed = true;
						SoundManager.Instance.PlayInstance("Douse");
					}
					this.lavaDamageTimer.Update(gameTime.ElapsedGameTime);
					if (this.lavaDamageTimer.Expired)
					{
						this.ApplyDamage(0.25f, this.LocalPlayer.WorldPosition - new Vector3(0f, 10f, 0f));
						this.lavaDamageTimer.Reset();
						this.lavaSoundPlayed = false;
					}
				}
				else
				{
					this.lavaDamageTimer.Reset();
				}
				if (this.LocalPlayer.Underwater)
				{
					this.PlayerOxygen -= this.OxygenDecayRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
					if (this.PlayerOxygen < 0f)
					{
						this.PlayerOxygen = 0f;
						this.PlayerHealth -= this.OxygenHealthPenaltyRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
						this.HealthRecoverTimer.Reset();
					}
				}
				else
				{
					this.PlayerOxygen = 1f;
				}
				if (!this.LocalPlayer.Dead)
				{
					this.HealthRecoverTimer.Update(gameTime.ElapsedGameTime);
					if (this.PlayerHealth < 1f && this.HealthRecoverTimer.Expired)
					{
						this.PlayerHealth += this.HealthRecoverRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
						if (this.PlayerHealth > 1f)
						{
							this.PlayerHealth = 1f;
						}
					}
					this.StaminaBlockTimer.Update(gameTime.ElapsedGameTime);
					this.StaminaRecoverTimer.Update(gameTime.ElapsedGameTime);
					if (this.PlayerStamina < 1f)
					{
						float num = (this.StaminaRecoverTimer.Expired ? this.StaminaRecoverRate : (this.StaminaRecoverRate * this.StaminaDamagedRecoveryModifier));
						this.PlayerStamina += num * (float)gameTime.ElapsedGameTime.TotalSeconds;
						if (this.PlayerStamina > 1f)
						{
							this.PlayerStamina = 1f;
						}
					}
				}
				this._periodicSaveTimer.Update(gameTime.ElapsedGameTime);
				if (this._periodicSaveTimer.Expired)
				{
					this._periodicSaveTimer.Reset();
					this._game.SaveData();
				}
				int num2 = this._game._terrain.DepthUnderGround(this._game.LocalPlayer.LocalPosition);
				if (this.lightningFlashCount <= 0 || this._game.LocalPlayer.LocalPosition.Y <= -32f)
				{
					CastleMinerZGame.Instance.GameScreen._sky.drawLightning = false;
				}
				if (this.timeToLightning.Expired)
				{
					if (this.lightningFlashCount > 0 && !CastleMinerZGame.Instance.GameScreen._sky.drawLightning)
					{
						CastleMinerZGame.Instance.GameScreen._sky.drawLightning = true;
						this.lightningFlashCount--;
						this.timeToLightning = new OneShotTimer(TimeSpan.FromSeconds(this.rand.NextDouble() / 4.0 + 0.10000000149011612));
					}
					else if (this.lightningFlashCount > 0 && CastleMinerZGame.Instance.GameScreen._sky.drawLightning)
					{
						CastleMinerZGame.Instance.GameScreen._sky.drawLightning = false;
					}
					else if (this.timeToThunder.Expired)
					{
						if (num2 < 4)
						{
							if (this.lightningFlashCount < 3)
							{
								SoundManager.Instance.PlayInstance("thunderLow");
							}
							else
							{
								SoundManager.Instance.PlayInstance("thunderHigh");
							}
						}
						this.timeToThunder = new OneShotTimer(TimeSpan.FromSeconds((double)((float)this.rand.NextDouble() * 2f)));
						this.timeToLightning = new OneShotTimer(TimeSpan.FromSeconds((double)this.rand.Next(10, 40)));
						this.lightningFlashCount = this.rand.Next(0, 4);
					}
					else
					{
						this.timeToThunder.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
					}
				}
				else if (this._game.LocalPlayer.LocalPosition.Y > -32f)
				{
					this.timeToLightning.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds));
					CastleMinerZGame.Instance.GameScreen._sky.drawLightning = false;
				}
				this.DoConstructionModeUpdate();
				this.UpdateAcheivements(gameTime);
				this.lastPosition = this.LocalPlayer.LocalPosition;
				base.OnUpdate(game, gameTime);
			}
		}

		public void Shoot(GunInventoryItemClass gun)
		{
			Matrix localToWorld = this.LocalPlayer.FPSCamera.LocalToWorld;
			GameMessageManager.Instance.Send(GameMessageType.LocalPlayerFiredGun, null, gun);
			if (gun is RocketLauncherBaseInventoryItemClass)
			{
				RocketLauncherBaseInventoryItemClass rocketLauncherBaseInventoryItemClass = gun as RocketLauncherBaseInventoryItemClass;
				FireRocketMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, localToWorld, gun.ID, rocketLauncherBaseInventoryItemClass.IsGuided());
				return;
			}
			if (gun is PumpShotgunInventoryItemClass || gun is LaserShotgunClass)
			{
				Angle angle;
				if (this.LocalPlayer.Shouldering)
				{
					angle = gun.ShoulderedMinAccuracy + this.InnaccuracyMultiplier * (gun.ShoulderedMaxAccuracy - gun.ShoulderedMinAccuracy);
				}
				else
				{
					angle = gun.MinInnaccuracy + this.InnaccuracyMultiplier * (gun.MaxInnaccuracy - gun.MinInnaccuracy);
				}
				ShotgunShotMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, localToWorld, angle, gun.ID, gun.NeedsDropCompensation);
				return;
			}
			Angle angle2;
			if (this.LocalPlayer.Shouldering)
			{
				angle2 = gun.ShoulderedMinAccuracy + this.InnaccuracyMultiplier * (gun.ShoulderedMaxAccuracy - gun.ShoulderedMinAccuracy);
			}
			else
			{
				angle2 = gun.MinInnaccuracy + this.InnaccuracyMultiplier * (gun.MaxInnaccuracy - gun.MinInnaccuracy);
			}
			GunshotMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, localToWorld, angle2, gun.ID, gun.NeedsDropCompensation);
		}

		public void MeleePlayer(InventoryItem tool, Player player)
		{
			MeleePlayerMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, player.Gamer, this.ActiveInventoryItem.ItemClass.ID, this.ConstructionProbe.GetIntersection());
			ParticleEmitter particleEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			TracerManager.Instance.Scene.Children.Add(particleEmitter);
			particleEmitter.LocalPosition = this.ConstructionProbe.GetIntersection();
			particleEmitter.DrawPriority = 900;
			if (tool.InflictDamage())
			{
				this.PlayerInventory.Remove(tool);
			}
		}

		public void Melee(InventoryItem tool)
		{
			byte b;
			if (this._game.LocalPlayer != null && this._game.LocalPlayer.ValidGamer)
			{
				b = this._game.LocalPlayer.Gamer.Id;
			}
			else
			{
				b = byte.MaxValue;
			}
			this.ConstructionProbe.EnemyHit.TakeDamage(this.ConstructionProbe.GetIntersection(), Vector3.Normalize(this.ConstructionProbe._end - this.ConstructionProbe._start), tool.ItemClass, b);
			ParticleEmitter particleEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			TracerManager.Instance.Scene.Children.Add(particleEmitter);
			particleEmitter.LocalPosition = this.ConstructionProbe.GetIntersection();
			particleEmitter.DrawPriority = 900;
			if (tool.InflictDamage())
			{
				this.PlayerInventory.Remove(tool);
			}
		}

		private bool IsValidDigTarget(BlockTypeEnum blockType, IntVector3 worldPos)
		{
			return blockType != BlockTypeEnum.TeleportStation || this.LocalPlayer.PlayerInventory.GetTeleportAtWorldIndex(worldPos * Vector3.One) != null;
		}

		public void Dig(InventoryItem tool, bool effective)
		{
			if (!this.ConstructionProbe._collides)
			{
				return;
			}
			if (!BlockTerrain.Instance.OkayToBuildHere(this.ConstructionProbe._worldIndex))
			{
				return;
			}
			BlockTypeEnum block = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
			if (BlockType.GetType(block).CanBeDug && this.IsValidDigTarget(block, this.ConstructionProbe._worldIndex))
			{
				DigMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, false, this.ConstructionProbe.GetIntersection(), this.ConstructionProbe._inNormal, block);
				if (effective)
				{
					if (BlockType.IsContainer(block))
					{
						DestroyCrateMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex);
						Crate crate;
						if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(this.ConstructionProbe._worldIndex, out crate))
						{
							crate.EjectContents();
						}
					}
					if (block == BlockTypeEnum.TeleportStation)
					{
						DestroyCustomBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, block);
						this.LocalPlayer.RemoveTeleportStationObject(this.ConstructionProbe._worldIndex + Vector3.Zero);
					}
					CastleMinerZGame.Instance.PlayerStats.DugBlock(block);
					GameMessageManager.Instance.Send(GameMessageType.LocalPlayerMinedBlock, block, tool);
					if (BlockType.ShouldDropLoot(block))
					{
						PossibleLootType.ProcessLootBlockOutput(block, this.ConstructionProbe._worldIndex);
					}
					else
					{
						IntVector3 intVector = this.ConstructionProbe._worldIndex;
						if (BlockType.IsUpperDoor(block))
						{
							intVector += new IntVector3(0, -1, 0);
						}
						InventoryItem inventoryItem = tool.CreatesWhenDug(block, intVector);
						if (inventoryItem != null)
						{
							PickupManager.Instance.CreatePickup(inventoryItem, intVector + Vector3.Zero, false, false);
						}
					}
					AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.Empty);
					for (BlockFace blockFace = BlockFace.POSX; blockFace < BlockFace.NUM_FACES; blockFace++)
					{
						IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(this.ConstructionProbe._worldIndex, blockFace);
						BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(neighborIndex);
						if (BlockType.GetType(blockWithChanges).Facing == blockFace)
						{
							InventoryItem inventoryItem2 = BlockInventoryItemClass.CreateBlockItem(BlockType.GetType(blockWithChanges).ParentBlockType, 1, neighborIndex);
							PickupManager.Instance.CreatePickup(inventoryItem2, IntVector3.ToVector3(neighborIndex) + new Vector3(0.5f, 0.5f, 0.5f), false, false);
							AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, neighborIndex, BlockTypeEnum.Empty);
							this.CheckToRemoveDoorConnections(BlockType.GetType(blockWithChanges).ParentBlockType, neighborIndex);
						}
					}
					this.CheckToRemoveDoorConnections(block, this.ConstructionProbe._worldIndex);
				}
				else
				{
					GameMessageManager.Instance.Send(GameMessageType.LocalPlayerPickedAtBlock, block, tool);
				}
				if (tool.InflictDamage())
				{
					this.PlayerInventory.Remove(tool);
				}
			}
		}

		private void CheckToRemoveDoorConnections(BlockTypeEnum removedBlockType, IntVector3 location)
		{
			IntVector3 intVector = IntVector3.Zero;
			if (BlockType.IsLowerDoor(removedBlockType))
			{
				IntVector3 intVector2 = location + new IntVector3(0, 1, 0);
				intVector = location;
				AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, intVector2, BlockTypeEnum.Empty);
			}
			if (BlockType.IsUpperDoor(removedBlockType))
			{
				intVector = location + new IntVector3(0, -1, 0);
				AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, intVector, BlockTypeEnum.Empty);
			}
			if (intVector != IntVector3.Zero)
			{
				DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector, removedBlockType);
			}
		}

		public IntVector3 Build(BlockInventoryItem blockItem, bool validateOnly = false)
		{
			IntVector3 zero = IntVector3.Zero;
			if (!this.ConstructionProbe._collides)
			{
				return zero;
			}
			IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(this.ConstructionProbe._worldIndex, this.ConstructionProbe._inFace);
			BlockTypeEnum block = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
			if (!BlockTerrain.Instance.OkayToBuildHere(neighborIndex))
			{
				return zero;
			}
			BlockType type = BlockType.GetType(block);
			if (!type.CanBuildOn)
			{
				return zero;
			}
			if (!blockItem.CanPlaceHere(neighborIndex, this.ConstructionProbe._inFace))
			{
				return zero;
			}
			bool flag = true;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (networkGamer != null)
					{
						Player player = (Player)networkGamer.Tag;
						if (player != null)
						{
							player.MovementProbe.SkipEmbedded = false;
							Vector3 worldPosition = player.WorldPosition;
							worldPosition.Y += 0.05f;
							player.MovementProbe.Init(worldPosition, worldPosition, player.PlayerAABB);
							if (BlockTerrain.Instance.ProbeTouchesBlock(player.MovementProbe, neighborIndex))
							{
								flag = false;
								break;
							}
						}
					}
				}
				if (flag)
				{
					if (validateOnly)
					{
						return neighborIndex;
					}
					BoundingBox boundingBox = default(BoundingBox);
					boundingBox.Min = IntVector3.ToVector3(neighborIndex) + new Vector3(0.01f, 0.01f, 0.01f);
					boundingBox.Max = boundingBox.Min + new Vector3(0.98f, 0.98f, 0.98f);
					if (!EnemyManager.Instance.TouchesZombies(boundingBox))
					{
						BlockTypeEnum block2 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
						DigMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, true, this.ConstructionProbe.GetIntersection(), this.ConstructionProbe._inNormal, block2);
						blockItem.AlterBlock(this.LocalPlayer, neighborIndex, this.ConstructionProbe._inFace);
						return neighborIndex;
					}
				}
			}
			return zero;
		}

		public override void OnLostFocus()
		{
			new GameTime(TimeSpan.FromSeconds(0.001), TimeSpan.FromSeconds(0.001));
			CastleMinerZGame.Instance._controllerMapping.ClearAllControls();
			base.OnLostFocus();
		}

		protected void DrawPlayerList(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont medFont = this._game._medFont;
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			spriteBatch.Begin();
			int count = this._game.CurrentNetworkSession.AllGamers.Count;
			int maxGamers = this._game.CurrentNetworkSession.MaxGamers;
			this._builder.Length = 0;
			this._builder.Append(Strings.Players + " ").Concat(count).Append("/")
				.Concat(maxGamers);
			Vector2 vector = medFont.MeasureString(this._builder);
			spriteBatch.DrawOutlinedText(medFont, this._builder, new Vector2((float)screenRect.Right - vector.X, (float)screenRect.Bottom - vector.Y), Color.White, Color.Black, 2);
			float[] array = new float[1];
			float num = 0f;
			num += (array[0] = medFont.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float num2 = ((float)Screen.Adjuster.ScreenRect.Width - num) / 2f;
			float num3 = (float)screenRect.Top;
			spriteBatch.DrawOutlinedText(medFont, Strings.Player, new Vector2(num2, num3), Color.Orange, Color.Black, 2);
			num3 += (float)medFont.LineSpacing;
			for (int i = 0; i < this._game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = this._game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null)
				{
					Player player = (Player)networkGamer.Tag;
					spriteBatch.DrawOutlinedText(medFont, player.Gamer.Gamertag, new Vector2(num2, num3), player.Gamer.IsLocal ? Color.Red : Color.White, Color.Black, 2);
					if (player.Profile != null)
					{
						float num4 = (float)medFont.LineSpacing * 0.9f;
						float num5 = (float)medFont.LineSpacing - num4;
						if (player.GamerPicture != null)
						{
							spriteBatch.Draw(player.GamerPicture, new Rectangle((int)(num2 - (float)medFont.LineSpacing), (int)(num3 + num5), (int)num4, (int)num4), Color.White);
						}
					}
					num3 += (float)medFont.LineSpacing;
				}
			}
			spriteBatch.End();
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			CastleMinerZGame.Instance.PlayerStats.GetItemStats(this.ActiveInventoryItem.ItemClass.ID);
			CastleMinerZControllerMapping controllerMapping = this._game._controllerMapping;
			controllerMapping.Sensitivity = this._game.PlayerStats.controllerSensitivity;
			controllerMapping.InvertY = this._game.PlayerStats.InvertYAxis;
			controllerMapping.ProcessInput(inputManager.Keyboard, inputManager.Mouse, controller);
			float num = 5f;
			Vector2 vector = new Vector2(this.maxGunCameraShift, this.maxGunCameraShift);
			if (this.LocalPlayer.Shouldering)
			{
				vector /= 2f;
			}
			Vector2 vector2 = controllerMapping.Aiming * vector;
			this.GunEyePointCameraLocation += (vector2 - this.GunEyePointCameraLocation) * num * (float)gameTime.ElapsedGameTime.TotalSeconds;
			GunInventoryItem gunInventoryItem = this.ActiveInventoryItem as GunInventoryItem;
			if (gunInventoryItem != null)
			{
				if (!this.LocalPlayer.InContact)
				{
					if (this.InnaccuracyMultiplier < 1f)
					{
						this.InnaccuracyMultiplier += gunInventoryItem.GunClass.InnaccuracySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					if (this.InnaccuracyMultiplier > 1f)
					{
						this.InnaccuracyMultiplier = 1f;
					}
				}
				else if ((double)controllerMapping.Movement.X < -0.1 || (double)controllerMapping.Movement.X > 0.1 || (double)controllerMapping.Movement.Y < -0.1 || (double)controllerMapping.Movement.Y > 0.1)
				{
					if (this.InnaccuracyMultiplier < 1f)
					{
						float num2 = MathHelper.Max(Math.Abs(controllerMapping.Movement.X), Math.Abs(controllerMapping.Movement.Y)) * gunInventoryItem.GunClass.InnaccuracySpeed;
						this.InnaccuracyMultiplier += num2 * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					if (this.InnaccuracyMultiplier > 1f)
					{
						this.InnaccuracyMultiplier = 1f;
					}
				}
				else
				{
					if (this.InnaccuracyMultiplier > 0f)
					{
						this.InnaccuracyMultiplier -= gunInventoryItem.GunClass.InnaccuracySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					if (this.InnaccuracyMultiplier < 0f)
					{
						this.InnaccuracyMultiplier = 0f;
					}
				}
			}
			if (controller.PressedButtons.Start || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				SoundManager.Instance.PlayInstance("Click");
				this._game.GameScreen.ShowInGameMenu();
			}
			else if (this._game.IsOnlineGame && !this.IsChatting && controllerMapping.TextChat.Released)
			{
				this._game.GameScreen._uiGroup.PushScreen(this._chatScreen);
			}
			else if (!this.LocalPlayer.Dead && controllerMapping.BlockUI.Pressed)
			{
				this._game.GameScreen.ShowBlockPicker();
				SoundManager.Instance.PlayInstance("Click");
			}
			this.LocalPlayer.ProcessInput(this._game._controllerMapping, gameTime);
			if (this._game.IsOnlineGame)
			{
				if (controllerMapping.PlayersScreen.Pressed)
				{
					this.showPlayers = true;
				}
				else if (!controllerMapping.PlayersScreen.Held)
				{
					this.showPlayers = false;
				}
			}
			else
			{
				this.showPlayers = false;
			}
			this._game.ShowTitleSafeArea = !this._hideUI;
			this.PlayerInventory.Update(gameTime);
			this._game.GameScreen.CrackBox.CrackAmount = 0f;
			if (this.LocalPlayer.Dead)
			{
				this.LocalPlayer.UsingTool = false;
				if ((controllerMapping.Jump.Pressed || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || inputManager.Mouse.LeftButtonPressed) && this.timeToShowRespawnText.Expired)
				{
					if (this._game.IsOnlineGame && this.AllPlayersDead() && this._game.GameMode == GameModeTypes.Endurance)
					{
						if (this._game.CurrentNetworkSession.IsHost)
						{
							RestartLevelMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer);
							BroadcastTextMessage.Send(this._game.MyNetworkGamer, this.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Restarted_The_Game);
						}
					}
					else if (!this.WaitToRespawn)
					{
						this.RespawnPlayer();
					}
				}
			}
			else
			{
				if (controllerMapping.NextItem.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex++;
					this.PlayerInventory.SelectedInventoryIndex = this.PlayerInventory.SelectedInventoryIndex % 8;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.PrevoiusItem.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex--;
					if (this.PlayerInventory.SelectedInventoryIndex < 0)
					{
						this.PlayerInventory.SelectedInventoryIndex = 8 + this.PlayerInventory.SelectedInventoryIndex;
					}
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot1.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 0;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot2.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 1;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot3.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 2;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot4.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 3;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot5.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 4;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot6.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 5;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot7.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 6;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				else if (controllerMapping.Slot8.Pressed && !this.LocalPlayer.UsingAnimationPlaying)
				{
					SoundManager.Instance.PlayInstance("Click");
					this.PlayerInventory.SelectedInventoryIndex = 7;
					this.LocalPlayer.Shouldering = false;
					this.PlayerInventory.ActiveInventoryItem.DigTime = TimeSpan.Zero;
				}
				if (controllerMapping.DropQuickbarItem.Pressed)
				{
					this.PlayerInventory.DropOneSelectedTrayItem();
				}
				if (controllerMapping.SwitchTray.Pressed)
				{
					this.PlayerInventory.SwitchCurrentTray();
				}
				if (controllerMapping.Activate.Pressed && this.ConstructionProbe._collides)
				{
					BlockTypeEnum block = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
					if (BlockType.IsContainer(block))
					{
						Crate crate = CastleMinerZGame.Instance.CurrentWorld.GetCrate(this.ConstructionProbe._worldIndex, true);
						this._crateScreen.CurrentCrate = crate;
						this._game.GameScreen._uiGroup.PushScreen(this._crateScreen);
						SoundManager.Instance.PlayInstance("Click");
					}
					else if (BlockType.IsSpawnerClickable(block))
					{
						Spawner spawner = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(this.ConstructionProbe._worldIndex, true, block);
						if (this._game.IsOnlineGame)
						{
							BroadcastTextMessage.Send(this._game.MyNetworkGamer, this.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_triggered_a_monster_spawner);
						}
						spawner.StartSpawner(block);
						SoundManager.Instance.PlayInstance("Click");
					}
					else
					{
						BlockTypeEnum blockTypeEnum = block;
						switch (blockTypeEnum)
						{
						case BlockTypeEnum.NormalLowerDoorClosedZ:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorOpenZ, BlockTypeEnum.NormalUpperDoorOpen);
							goto IL_0E9C;
						case BlockTypeEnum.NormalLowerDoorClosedX:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorOpenX, BlockTypeEnum.NormalUpperDoorOpen);
							goto IL_0E9C;
						case BlockTypeEnum.NormalLowerDoor:
							break;
						case BlockTypeEnum.NormalUpperDoorClosed:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, true);
							AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.NormalUpperDoorOpen);
							BlockTypeEnum block2 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (block2 == BlockTypeEnum.NormalLowerDoorClosedX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorOpenX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorOpenZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							goto IL_0E9C;
						}
						case BlockTypeEnum.NormalLowerDoorOpenZ:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorClosedZ, BlockTypeEnum.NormalUpperDoorClosed);
							goto IL_0E9C;
						case BlockTypeEnum.NormalLowerDoorOpenX:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorClosedX, BlockTypeEnum.NormalUpperDoorClosed);
							goto IL_0E9C;
						case BlockTypeEnum.NormalUpperDoorOpen:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, false);
							AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.NormalUpperDoorClosed);
							BlockTypeEnum block3 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (block3 == BlockTypeEnum.NormalLowerDoorOpenX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorClosedX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorClosedZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							goto IL_0E9C;
						}
						case BlockTypeEnum.TNT:
							this.SetFuseForExplosive(this.ConstructionProbe._worldIndex, ExplosiveTypes.TNT);
							goto IL_0E9C;
						case BlockTypeEnum.C4:
							this.SetFuseForExplosive(this.ConstructionProbe._worldIndex, ExplosiveTypes.C4);
							goto IL_0E9C;
						default:
							switch (blockTypeEnum)
							{
							case BlockTypeEnum.StrongLowerDoorClosedZ:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorOpenZ, BlockTypeEnum.StrongUpperDoorOpen);
								goto IL_0E9C;
							case BlockTypeEnum.StrongLowerDoorClosedX:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorOpenX, BlockTypeEnum.StrongUpperDoorOpen);
								goto IL_0E9C;
							case BlockTypeEnum.StrongLowerDoor:
								break;
							case BlockTypeEnum.StrongUpperDoorClosed:
							{
								DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, true);
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.StrongUpperDoorOpen);
								BlockTypeEnum block4 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
								if (block4 == BlockTypeEnum.StrongLowerDoorClosedX)
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorOpenX);
								}
								else
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorOpenZ);
								}
								SoundManager.Instance.PlayInstance("Click");
								goto IL_0E9C;
							}
							case BlockTypeEnum.StrongLowerDoorOpenZ:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorClosedZ, BlockTypeEnum.StrongUpperDoorClosed);
								goto IL_0E9C;
							case BlockTypeEnum.StrongLowerDoorOpenX:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorClosedX, BlockTypeEnum.StrongUpperDoorClosed);
								goto IL_0E9C;
							case BlockTypeEnum.StrongUpperDoorOpen:
							{
								DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, false);
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.StrongUpperDoorClosed);
								BlockTypeEnum block5 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
								if (block5 == BlockTypeEnum.StrongLowerDoorOpenX)
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorClosedX);
								}
								else
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorClosedZ);
								}
								SoundManager.Instance.PlayInstance("Click");
								goto IL_0E9C;
							}
							default:
								switch (blockTypeEnum)
								{
								case BlockTypeEnum.EnemySpawnAltar:
									goto IL_0E9C;
								case BlockTypeEnum.TeleportStation:
									this.PlayerInventory.ShowTeleportStationMenu(this.ConstructionProbe._worldIndex + Vector3.Zero);
									goto IL_0E9C;
								}
								break;
							}
							break;
						}
						SoundManager.Instance.PlayInstance("Error");
					}
				}
				IL_0E9C:
				if (this.ActiveInventoryItem == null)
				{
					this.LocalPlayer.UsingTool = false;
				}
				else
				{
					this.ActiveInventoryItem.ProcessInput(this, controllerMapping);
					this.PlayerInventory.RemoveEmptyItems();
				}
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		private void UseDoor(BlockTypeEnum bottomDoorPiece, BlockTypeEnum topDoorPiece)
		{
			LocalNetworkGamer localNetworkGamer = (LocalNetworkGamer)this.LocalPlayer.Gamer;
			DoorOpenCloseMessage.Send(localNetworkGamer, this.ConstructionProbe._worldIndex, true);
			AlterBlockMessage.Send(localNetworkGamer, this.ConstructionProbe._worldIndex, bottomDoorPiece);
			AlterBlockMessage.Send(localNetworkGamer, this.ConstructionProbe._worldIndex + new IntVector3(0, 1, 0), topDoorPiece);
			SoundManager.Instance.PlayInstance("Click");
		}

		public void SetFuseForExplosive(IntVector3 location, ExplosiveTypes explosiveType)
		{
			Explosive explosive = new Explosive(location, explosiveType);
			if (!this._tntWaitingToExplode.Contains(explosive))
			{
				this._tntWaitingToExplode.Add(explosive);
				AddExplosiveFlashMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, location);
			}
		}

		public const float MaxHealth = 1f;

		public const float MaxStamina = 1f;

		public const float MaxOxygen = 1f;

		private const float PROBE_LENGTH = 5f;

		public static InGameHUD Instance;

		public int maxDistanceTraveled;

		public float PlayerHealth = 1f;

		public float HealthRecoverRate = 0.75f;

		public OneShotTimer HealthRecoverTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		public float PlayerStamina = 1f;

		public float StaminaRecoverRate = 0.25f;

		public float StaminaDamagedRecoveryModifier;

		public OneShotTimer StaminaRecoverTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		public OneShotTimer StaminaBlockTimer = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		public float PlayerOxygen = 1f;

		public float OxygenDecayRate = 0.1f;

		public float OxygenHealthPenaltyRate = 0.2f;

		private float maxGunCameraShift = 0.04f;

		private Vector2 GunEyePointCameraLocation = Vector2.Zero;

		private int lightningFlashCount;

		private OneShotTimer timeToLightning;

		private OneShotTimer timeToThunder;

		private OneShotTimer timeToRespawn = new OneShotTimer(TimeSpan.FromSeconds(20.0));

		private OneShotTimer timeToShowRespawnText = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		private OneShotTimer fadeInGameStart = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		private Random rand = new Random();

		private List<Explosive> _tntWaitingToExplode = new List<Explosive>();

		public float InnaccuracyMultiplier;

		private List<DNA.Triggers.Trigger> _triggers = new List<DNA.Triggers.Trigger>();

		private bool gameBegun;

		private Angle compassRotation;

		private Vector3 trialMaxPosition;

		private PCDialogScreen _travelMaxDialog;

		private CrateScreen _crateScreen;

		public OneShotTimer drawDayTimer = new OneShotTimer(TimeSpan.FromSeconds(9.0));

		public int currentDay;

		private List<InGameHUD.DamageIndicator> _damageIndicator = new List<InGameHUD.DamageIndicator>();

		private Sprite _damageArrow;

		private Sprite _crosshairTick;

		private float _crosshairTickDrawLocation;

		private float _secondTrayAlpha = 0.4f;

		public ConstructionProbeClass ConstructionProbe = new ConstructionProbeClass();

		public ConsoleElement console;

		public ConsoleElement lootConsole;

		private CastleMinerZGame _game;

		private Sprite _gridSprite;

		private Sprite _selectorSprite;

		private Sprite _crosshair;

		private Sprite _emptyStaminaBar;

		private Sprite _emptyHealthBar;

		private Sprite _fullHealthBar;

		private Sprite _bubbleBar;

		private Sprite _sniperScope;

		private Sprite _missileLocking;

		private Sprite _missileLock;

		private Queue<AchievementManager<CastleMinerZPlayerStats>.Achievement> AcheivementsToDraw = new Queue<AchievementManager<CastleMinerZPlayerStats>.Achievement>();

		private AchievementManager<CastleMinerZPlayerStats>.Achievement displayedAcheivement;

		private OneShotTimer _resetCraterFoundTriggerTimer = new OneShotTimer(TimeSpan.FromMinutes(1.0));

		private DNA.Triggers.Trigger _craterFoundTrigger;

		private OneShotTimer acheivementDisplayTimer = new OneShotTimer(TimeSpan.FromSeconds(10.0));

		private Vector2 acheimentDisplayLocation = new Vector2(453f, 439f);

		private string _achievementText1 = "";

		private string _achievementText2 = "";

		private int currentDistanceTraveled;

		private Vector2 _achievementLocation;

		private InventoryItem lastItem;

		private StringBuilder sbuilder = new StringBuilder();

		private StringBuilder distanceBuilder = new StringBuilder();

		private Rectangle _prevTitleSafe = Rectangle.Empty;

		private EulerAngle freeFlyCameraRotation = default(EulerAngle);

		private OneShotTimer lavaDamageTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private bool lavaSoundPlayed;

		private Vector3 lastPosition = Vector3.Zero;

		private OneShotTimer _periodicSaveTimer = new OneShotTimer(TimeSpan.FromSeconds(10.0));

		private float lastTOD = -1f;

		private StringBuilder _builder = new StringBuilder();

		private bool _hideUI;

		private bool showPlayers;

		private PlainChatInputScreen _chatScreen;

		public class DamageIndicator
		{
			public float fadeAmount
			{
				get
				{
					if ((double)this.drawTimer.PercentComplete < 0.67)
					{
						return 1f;
					}
					return (1f - this.drawTimer.PercentComplete) * 3f;
				}
			}

			public DamageIndicator(Vector3 source)
			{
				this.DamageSource = source;
				this.drawTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));
			}

			public Vector3 DamageSource;

			public OneShotTimer drawTimer;
		}
	}
}
