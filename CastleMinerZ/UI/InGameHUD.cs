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
			Player closetPlayer = null;
			float closetDist = float.MaxValue;
			foreach (NetworkGamer gamer in this._game.CurrentNetworkSession.RemoteGamers)
			{
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					if (player != null && !player.Dead)
					{
						float dist = player.LocalPosition.LengthSquared();
						if (dist < closetDist)
						{
							closetPlayer = player;
							closetDist = dist;
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
			else if (closetPlayer == null)
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
				this._game.GameScreen.TeleportToLocation(closetPlayer.LocalPosition, false);
				if (this._game.IsOnlineGame)
				{
					BroadcastTextMessage.Send(this._game.MyNetworkGamer, this.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Respawned);
				}
			}
		}

		public bool AllPlayersDead()
		{
			Player closetPlayer = null;
			foreach (NetworkGamer gamer in this._game.CurrentNetworkSession.RemoteGamers)
			{
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					if (player != null && !player.Dead)
					{
						closetPlayer = player;
						break;
					}
				}
			}
			return closetPlayer == null;
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
			Rectangle titleSafe = Screen.Adjuster.ScreenRect;
			Sprite endSprite = this._game._uiSprites["AwardEnd"];
			Sprite centerSprite = this._game._uiSprites["AwardCenter"];
			Sprite circle = this._game._uiSprites["AwardCircle"];
			float endWidth = (float)endSprite.Width;
			Vector2 textOffset = new Vector2(79f, 10f);
			Vector2 textOffset2 = new Vector2(79f, 37f);
			float indent = textOffset.X - endWidth;
			Vector2 fontSize = this._game._systemFont.MeasureString(this._achievementText1);
			Vector2 fontSize2 = this._game._systemFont.MeasureString(this._achievementText2);
			float barwidth = Math.Max(fontSize.X, fontSize2.X) + indent;
			float hsize = barwidth + endWidth * 2f;
			float hpos = (float)titleSafe.Center.X - hsize / 2f;
			int yloc = (int)this.acheimentDisplayLocation.Y;
			this._achievementLocation = new Vector2(hpos, (float)yloc);
			endSprite.Draw(spriteBatch, new Vector2(hpos, (float)yloc), Color.White);
			endSprite.Draw(spriteBatch, new Vector2(hpos + barwidth + endWidth, (float)yloc), 1f, Color.White, SpriteEffects.FlipHorizontally);
			centerSprite.Draw(spriteBatch, new Rectangle((int)(hpos + endWidth) - 1, yloc, (int)(barwidth + 2f), centerSprite.Height), Color.White);
			circle.Draw(spriteBatch, new Vector2(hpos, (float)yloc), Color.White);
			spriteBatch.DrawString(this._game._systemFont, this._achievementText1, textOffset + this._achievementLocation, new Color(219, 219, 219));
			spriteBatch.DrawString(this._game._systemFont, this._achievementText2, textOffset2 + this._achievementLocation, new Color(219, 219, 219));
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
				GunInventoryItem gitm = (GunInventoryItem)this.ActiveInventoryItem;
				this.LocalPlayer.ReloadSound = gitm.GunClass.ReloadSound;
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
			Rectangle titleSafe = new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height);
			spriteBatch.Begin();
			if (!this.LocalPlayer.Dead && this.LocalPlayer.ShoulderedAnimState)
			{
				GunInventoryItemClass activeGunClass = this.ActiveInventoryItem.ItemClass as GunInventoryItemClass;
				if (activeGunClass != null && activeGunClass.Scoped)
				{
					this.LocalPlayer.Avatar.Visible = false;
					if (activeGunClass is RocketLauncherGuidedInventoryItemClass)
					{
						RocketLauncherGuidedInventoryItemClass rocketLauncherClass = (RocketLauncherGuidedInventoryItemClass)activeGunClass;
						if (rocketLauncherClass.LockedOnToDragon)
						{
							spriteBatch.Draw(this._missileLock, rocketLauncherClass.LockedOnSpriteLocation, Color.Red);
						}
						else
						{
							spriteBatch.Draw(this._missileLocking, rocketLauncherClass.LockedOnSpriteLocation, Color.Lime);
						}
					}
					Size scopeSize = new Size((int)((float)this._sniperScope.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._sniperScope.Height * Screen.Adjuster.ScaleFactor.Y));
					Vector2 location = new Vector2((float)(titleSafe.Center.X - scopeSize.Width / 2), (float)(titleSafe.Center.Y - scopeSize.Height / 2));
					spriteBatch.Draw(this._sniperScope, new Rectangle((int)location.X, (int)location.Y, scopeSize.Width, scopeSize.Height), Color.White);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, (int)location.Y), Color.Black);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, (int)location.Y + scopeSize.Height, Screen.Adjuster.ScreenRect.Width, (int)location.Y), Color.Black);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, (int)location.Y, (int)location.X, scopeSize.Height), Color.Black);
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(Screen.Adjuster.ScreenRect.Width - (int)location.X, (int)location.Y, (int)location.X, scopeSize.Height), Color.Black);
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
			if (titleSafe != this._prevTitleSafe)
			{
				this.console.Location = new Vector2((float)titleSafe.Left, (float)titleSafe.Top);
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
				int transparency = (int)((1f - this.PlayerHealth) * 120f);
				int red = (int)((1f - this.PlayerHealth) * 102f);
				if (transparency > 0)
				{
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), new Color(red, 0, 0, transparency));
				}
				if (this.LocalPlayer.PercentSubmergedLava > 0f)
				{
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), Color.Lerp(Color.Transparent, Color.Red, this.LocalPlayer.PercentSubmergedLava));
				}
				if (this.LocalPlayer.UnderLava)
				{
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), Color.Red);
				}
				Rectangle sourceRectangle = new Rectangle(0, 0, this._damageArrow.Width, this._damageArrow.Height);
				Vector2 origin = new Vector2((float)(this._damageArrow.Width / 2), (float)(this._damageArrow.Height + 150));
				Vector2 position = new Vector2((float)titleSafe.Center.X, (float)titleSafe.Center.Y);
				int indicatorIndex = 0;
				while (indicatorIndex < this._damageIndicator.Count)
				{
					this._damageIndicator[indicatorIndex].drawTimer.Update(gameTime.ElapsedGameTime);
					if (this._damageIndicator[indicatorIndex].drawTimer.Expired)
					{
						this._damageIndicator.RemoveAt(indicatorIndex);
					}
					else
					{
						Vector3 toDamage = this._damageIndicator[indicatorIndex].DamageSource - this.LocalPlayer.LocalPosition;
						toDamage = Vector3.TransformNormal(toDamage, this.LocalPlayer.WorldToLocal);
						Angle angle = Angle.ATan2((double)toDamage.X, (double)(-(double)toDamage.Z));
						this._damageArrow.Draw(spriteBatch, position, sourceRectangle, new Color((int)(139f * this._damageIndicator[indicatorIndex].fadeAmount), 0, 0, (int)(255f * this._damageIndicator[indicatorIndex].fadeAmount)), angle, origin, 0.75f, SpriteEffects.None, 0f);
						indicatorIndex++;
					}
				}
				if (this.LocalPlayer.Dead && this.timeToShowRespawnText.Expired && this._game.GameScreen._uiGroup.CurrentScreen == this)
				{
					string deadStr = "";
					string deadStr2 = Strings.Click_To_Respawn;
					if (this.WaitToRespawn)
					{
						deadStr = Strings.Respawn_In + ": ";
						deadStr2 = "";
					}
					if (this._game.IsOnlineGame && this.AllPlayersDead() && this._game.GameMode == GameModeTypes.Endurance)
					{
						if (this._game.CurrentNetworkSession.IsHost)
						{
							deadStr = "";
							deadStr2 = Strings.Click_To_Restart;
						}
						else
						{
							deadStr = Strings.Waiting_For_Host_To_Restart;
							deadStr2 = "";
						}
					}
					Vector2 strSize = this._game._medLargeFont.MeasureString(deadStr + deadStr2);
					Vector2 drawPosition = new Vector2((float)titleSafe.Center.X - strSize.X / 2f, (float)titleSafe.Center.Y - strSize.Y / 2f);
					if (this._game.GameMode == GameModeTypes.Endurance)
					{
						this.sbuilder.Length = 0;
						this.sbuilder.Append(Strings.Distance_Traveled);
						this.sbuilder.Append(": ");
						this.sbuilder.Concat(this.maxDistanceTraveled);
						Vector2 endurStrSize = this._game._medLargeFont.MeasureString(this.sbuilder);
						drawPosition = new Vector2((float)titleSafe.Center.X - endurStrSize.X / 2f, (float)titleSafe.Center.Y - endurStrSize.Y - endurStrSize.Y / 2f);
						spriteBatch.DrawOutlinedText(this._game._medLargeFont, this.sbuilder, new Vector2(drawPosition.X, drawPosition.Y), Color.White, Color.Black, 2);
						this.sbuilder.Length = 0;
						this.sbuilder.Append(" " + Strings.In + " ");
						this.sbuilder.Concat(this.currentDay);
						this.sbuilder.Append((this.currentDay != 1) ? (" " + Strings.Days) : (" " + Strings.Day));
						endurStrSize = this._game._medLargeFont.MeasureString(this.sbuilder);
						drawPosition = new Vector2((float)titleSafe.Center.X - endurStrSize.X / 2f, (float)titleSafe.Center.Y - endurStrSize.Y / 2f);
						spriteBatch.DrawOutlinedText(this._game._medLargeFont, this.sbuilder, new Vector2(drawPosition.X, drawPosition.Y), Color.White, Color.Black, 2);
						drawPosition = new Vector2((float)titleSafe.Center.X - strSize.X / 2f, (float)titleSafe.Center.Y - strSize.Y / 2f + endurStrSize.Y);
					}
					spriteBatch.DrawOutlinedText(this._game._medLargeFont, deadStr, new Vector2(drawPosition.X, drawPosition.Y), Color.White, Color.Black, 2);
					drawPosition.X += this._game._medLargeFont.MeasureString(deadStr).X;
					if (!this.WaitToRespawn || (this.AllPlayersDead() && this._game.CurrentNetworkSession.IsHost && this._game.GameMode == GameModeTypes.Endurance))
					{
						if (this._game.GameMode != GameModeTypes.Endurance || !this.AllPlayersDead() || this._game.CurrentNetworkSession.IsHost)
						{
							spriteBatch.DrawOutlinedText(this._game._medLargeFont, deadStr2, new Vector2(drawPosition.X, drawPosition.Y), Color.White, Color.Black, 2);
						}
					}
					else if (this._game.IsOnlineGame && !this.AllPlayersDead() && this._game.GameMode == GameModeTypes.Endurance)
					{
						this.sbuilder.Length = 0;
						this.sbuilder.Concat((int)(21.0 - this.timeToRespawn.ElaspedTime.TotalSeconds));
						spriteBatch.DrawOutlinedText(this._game._medLargeFont, this.sbuilder, new Vector2(drawPosition.X, drawPosition.Y), Color.White, Color.Black, 2);
					}
				}
				this.DrawDistanceStr(spriteBatch);
				if (this.ConstructionProbe.AbleToBuild && this.PlayerInventory.ActiveInventoryItem != null)
				{
					BlockTypeEnum bte = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
					BlockType bt = BlockType.GetType(bte);
					spriteBatch.DrawString(this._game._medFont, bt.Name, new Vector2((float)titleSafe.Right - (this._game._medFont.MeasureString(bt.Name).X + 10f) * Screen.Adjuster.ScaleFactor.Y, (float)(this._game._medFont.LineSpacing * 4) * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
				}
				Size gridSize = new Size((int)((float)this._gridSprite.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._gridSprite.Height * Screen.Adjuster.ScaleFactor.Y));
				Rectangle gridRect = new Rectangle(titleSafe.Center.X - gridSize.Width / 2, titleSafe.Bottom - gridSize.Height - (int)(5f * Screen.Adjuster.ScaleFactor.Y), gridSize.Width, gridSize.Height);
				this._gridSprite.Draw(spriteBatch, gridRect, new Color(1f, 1f, 1f, this.GetTrayAlphaSetting()));
				Vector2 mainTrayOffset = new Vector2(-35f * Screen.Adjuster.ScaleFactor.Y, -68f * Screen.Adjuster.ScaleFactor.Y);
				Rectangle gridRect2 = gridRect;
				gridRect.X += (int)mainTrayOffset.X;
				gridRect.Y += (int)mainTrayOffset.Y;
				this._gridSprite.Draw(spriteBatch, gridRect, Color.White);
				this.DrawPlayerStats(spriteBatch);
				if (this.LocalPlayer.Underwater)
				{
					float bubbleScale = this.PlayerOxygen / 1f;
					Vector2 bubbleBarLocation = new Vector2((float)gridRect.Center.X, (float)(gridRect.Top - 30));
					this._bubbleBar.Draw(spriteBatch, new Rectangle((int)(bubbleBarLocation.X + (float)this._bubbleBar.Width * (1f - bubbleScale)), (int)bubbleBarLocation.Y, (int)((float)this._bubbleBar.Width * bubbleScale), this._bubbleBar.Height), new Rectangle((int)((float)this._bubbleBar.Width * (1f - bubbleScale)), 0, (int)((float)this._bubbleBar.Width * bubbleScale), this._bubbleBar.Height), Color.White);
				}
				int size = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
				for (int i = 0; i < this.PlayerInventory.TrayManager.CurrentTrayLength; i++)
				{
					InventoryItem item = this.PlayerInventory.TrayManager.GetItemFromCurrentTray(i);
					if (item != null)
					{
						int xpos = (int)(59f * Screen.Adjuster.ScaleFactor.Y * (float)i + (float)gridRect.Left + 2f * Screen.Adjuster.ScaleFactor.Y);
						item.Draw2D(spriteBatch, new Rectangle(xpos, gridRect.Top + (int)(2f * Screen.Adjuster.ScaleFactor.Y), size, size));
					}
				}
				for (int j = 0; j < this.PlayerInventory.TrayManager.CurrentTrayLength; j++)
				{
					InventoryItem item2 = this.PlayerInventory.TrayManager.GetItemFromNextTray(j);
					if (item2 != null)
					{
						int xpos2 = (int)(59f * Screen.Adjuster.ScaleFactor.Y * (float)j + (float)gridRect2.Left + 2f * Screen.Adjuster.ScaleFactor.Y);
						item2.Draw2D(spriteBatch, new Rectangle(xpos2, gridRect2.Top + (int)(2f * Screen.Adjuster.ScaleFactor.Y), size, size), new Color(1f, 1f, 1f, this.GetTrayAlphaSetting()), true);
					}
				}
				Rectangle selectorRect = new Rectangle(gridRect.Left + (int)(7f * Screen.Adjuster.ScaleFactor.Y + 59f * Screen.Adjuster.ScaleFactor.Y * (float)this.PlayerInventory.SelectedInventoryIndex), (int)((float)gridRect.Top + 7f * Screen.Adjuster.ScaleFactor.Y), size, size);
				this._selectorSprite.Draw(spriteBatch, selectorRect, Color.White);
				this.sbuilder.Length = 0;
				if (this.ActiveInventoryItem != null)
				{
					Vector2 textSize = this._game._medFont.MeasureString(this.ActiveInventoryItem.Name);
					this.ActiveInventoryItem.GetDisplayText(this.sbuilder);
					spriteBatch.DrawString(this._game._medFont, this.sbuilder, new Vector2((float)gridRect.Left, (float)gridRect.Y - textSize.Y * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuAqua * 0.75f, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
				}
				if (!this.LocalPlayer.Dead && !this.LocalPlayer.Shouldering)
				{
					GunInventoryItem ActiveGun = this.ActiveInventoryItem as GunInventoryItem;
					Angle fov = this.LocalPlayer.FPSCamera.FieldOfView;
					Angle innacuracy = Angle.FromDegrees(0.5f);
					if (ActiveGun != null)
					{
						if (this.LocalPlayer.Shouldering && !this.LocalPlayer.Reloading)
						{
							Angle accuracy = Angle.Lerp(ActiveGun.GunClass.MinInnaccuracy, ActiveGun.GunClass.ShoulderedMinAccuracy, this.LocalPlayer.Avatar.Animations[2].Progress);
							innacuracy = accuracy + this.InnaccuracyMultiplier * (ActiveGun.GunClass.ShoulderedMaxAccuracy - accuracy);
						}
						else
						{
							innacuracy = ActiveGun.GunClass.MinInnaccuracy + this.InnaccuracyMultiplier * (ActiveGun.GunClass.MaxInnaccuracy - ActiveGun.GunClass.MinInnaccuracy);
						}
					}
					this._crosshairTickDrawLocation = innacuracy / fov * (float)Screen.Adjuster.ScreenRect.Width;
					float scale = Math.Max(1f, Screen.Adjuster.ScaleFactor.Y);
					Color crosshairTickColor = new Color((1f - this.InnaccuracyMultiplier) / 2f + 0.5f, (1f - this.InnaccuracyMultiplier) / 2f + 0.5f, (1f - this.InnaccuracyMultiplier) / 2f + 0.5f, (1f - this.InnaccuracyMultiplier) / 2f + 0.5f);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)titleSafe.Center.X + this._crosshairTickDrawLocation)), (float)((int)((float)titleSafe.Center.Y - 1f * scale))), scale, crosshairTickColor);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)titleSafe.Center.X - (9f * scale + this._crosshairTickDrawLocation))), (float)((int)((float)titleSafe.Center.Y - 1f * scale))), scale, crosshairTickColor);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)titleSafe.Center.X + 1f * scale)), (float)((int)((float)titleSafe.Center.Y - 8f * scale - this._crosshairTickDrawLocation))), crosshairTickColor, Angle.FromDegrees(90f), Vector2.Zero, scale, SpriteEffects.None, 0f);
					spriteBatch.Draw(this._crosshairTick, new Vector2((float)((int)((float)titleSafe.Center.X + 1f * scale)), (float)((int)((float)titleSafe.Center.Y + this._crosshairTickDrawLocation + 1f * scale))), crosshairTickColor, Angle.FromDegrees(90f), Vector2.Zero, scale, SpriteEffects.None, 0f);
				}
				if (!this.fadeInGameStart.Expired)
				{
					float colorValue = (float)this.fadeInGameStart.ElaspedTime.TotalSeconds;
					colorValue = colorValue * 1f - colorValue;
					spriteBatch.Draw(this._game.DummyTexture, new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height), new Color(colorValue, colorValue, colorValue, 1f - (float)this.fadeInGameStart.ElaspedTime.TotalSeconds));
				}
				if (!this.drawDayTimer.Expired && !this.LocalPlayer.Dead)
				{
					float blender = 1f;
					float spn = 0.33333334f;
					if (this.drawDayTimer.ElaspedTime < TimeSpan.FromSeconds(3.0))
					{
						blender = this.drawDayTimer.PercentComplete / spn;
					}
					else if (this.drawDayTimer.ElaspedTime > TimeSpan.FromSeconds(6.0))
					{
						blender = 1f - (this.drawDayTimer.PercentComplete - spn * 2f) / spn;
					}
					this.sbuilder.Length = 0;
					this.sbuilder.Append(Strings.Day + " ");
					this.sbuilder.Concat(this.currentDay);
					spriteBatch.DrawString(this._game._largeFont, this.sbuilder, new Vector2((float)titleSafe.Left, (float)titleSafe.Bottom - this._game._largeFont.MeasureString(this.sbuilder).Y), CMZColors.MenuAqua * 0.75f * blender);
				}
				this.DrawAcheivements(device, spriteBatch, gameTime);
				spriteBatch.End();
			}
			this._prevTitleSafe = titleSafe;
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
			Color barColor = Color.DodgerBlue;
			Rectangle healthBarLocation;
			if (isCenterScreen)
			{
				healthBarLocation = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - (float)this._emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Height * Screen.Adjuster.ScaleFactor.Y));
			}
			else
			{
				healthBarLocation = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - (float)this._emptyHealthBar.Width * 2.2f * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyHealthBar.Height * Screen.Adjuster.ScaleFactor.Y));
			}
			float healthScale = this.PlayerHealth / 1f;
			this.DrawResourceBar(spriteBatch, this._emptyHealthBar, barColor, healthScale, healthBarLocation);
		}

		public void DrawStaminaBar(SpriteBatch spriteBatch)
		{
			Color staminaColor = Color.Yellow;
			Rectangle barLocation = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X + (float)this._emptyStaminaBar.Width * 0.2f * Screen.Adjuster.ScaleFactor.Y / 2f), (int)((float)Screen.Adjuster.ScreenRect.Top + 20f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyStaminaBar.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._emptyStaminaBar.Height * Screen.Adjuster.ScaleFactor.Y));
			float statScale = this.PlayerStamina / 1f;
			if (!this.StaminaBlockTimer.Expired)
			{
				staminaColor = Color.OrangeRed;
			}
			this.DrawResourceBar(spriteBatch, this._emptyStaminaBar, staminaColor, statScale, barLocation);
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
			IntVector3 addSpot = BlockTerrain.Instance.GetNeighborIndex(this.ConstructionProbe._worldIndex, this.ConstructionProbe._inFace);
			CastleMinerZGame.Instance.LocalPlayer.MovementProbe.SkipEmbedded = false;
			Vector3 pos = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			pos.Y += 0.05f;
			CastleMinerZGame.Instance.LocalPlayer.MovementProbe.Init(pos, pos, CastleMinerZGame.Instance.LocalPlayer.PlayerAABB);
			return !BlockTerrain.Instance.ProbeTouchesBlock(CastleMinerZGame.Instance.LocalPlayer.MovementProbe, addSpot);
		}

		protected void DoConstructionModeUpdate()
		{
			if (this.PlayerInventory.ActiveInventoryItem == null)
			{
				this.ConstructionProbe.Init(new Vector3(0f), new Vector3(1f), false);
			}
			else
			{
				Matrix i = this.LocalPlayer.FPSCamera.LocalToWorld;
				Vector3 eyePos = i.Translation;
				this.ConstructionProbe.Init(eyePos, Vector3.Add(eyePos, Vector3.Multiply(i.Forward, 5f)), this.PlayerInventory.ActiveInventoryItem.ItemClass.IsMeleeWeapon);
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
			Vector3 blockPosition = worldIndex + new Vector3(0.5f, 0.5f, 0.5f);
			-this.ConstructionProbe._inNormal;
			Matrix worldMat = Matrix.Identity;
			float offset = 0.51f;
			switch (this.ConstructionProbe._inFace)
			{
			case BlockFace.POSX:
				worldMat = Matrix.CreateWorld(blockPosition + new Vector3(1f, 0f, 0f) * offset, -Vector3.UnitY, Vector3.UnitX);
				break;
			case BlockFace.NEGZ:
				worldMat = Matrix.CreateWorld(blockPosition + new Vector3(0f, 0f, -1f) * offset, -Vector3.UnitX, -Vector3.UnitZ);
				break;
			case BlockFace.NEGX:
				worldMat = Matrix.CreateWorld(blockPosition + new Vector3(-1f, 0f, 0f) * offset, Vector3.UnitY, -Vector3.UnitX);
				break;
			case BlockFace.POSZ:
				worldMat = Matrix.CreateWorld(blockPosition + new Vector3(0f, 0f, 1f) * offset, Vector3.UnitX, Vector3.UnitZ);
				break;
			case BlockFace.POSY:
				worldMat = Matrix.CreateWorld(blockPosition + new Vector3(0f, 1f, 0f) * offset, Vector3.UnitX, Vector3.UnitY);
				break;
			case BlockFace.NEGY:
				worldMat = Matrix.CreateWorld(blockPosition + new Vector3(0f, -1f, 0f) * offset, -Vector3.UnitX, -Vector3.UnitY);
				break;
			}
			this._game.GameScreen.CrackBox.LocalPosition = worldIndex + new Vector3(0.5f, -0.002f, 0.5f);
			this._game.GameScreen.SelectorEntity.LocalToParent = worldMat;
			this._game.GameScreen.SelectorEntity.Visible = true;
		}

		public void Reset()
		{
			this.lastTOD = -1f;
		}

		public void ExternalUpdate(DNAGame game, GameTime gameTime)
		{
			this.OnUpdate(game, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (CastleMinerZGame.Instance.GameScreen.DoUpdate)
			{
				this.LocalPlayer.UpdateGunEyePointCamera(this.GunEyePointCameraLocation);
				if (this.ActiveInventoryItem is GPSItem)
				{
					GPSItem gps = (GPSItem)this.ActiveInventoryItem;
					this._game.GameScreen.GPSMarker.Visible = true;
					this._game.GameScreen.GPSMarker.LocalPosition = gps.PointToLocation + new Vector3(0.5f, 1f, 0.5f);
					this._game.GameScreen.GPSMarker.color = gps.color;
				}
				else
				{
					this._game.GameScreen.GPSMarker.Visible = false;
				}
				if (this.ActiveInventoryItem.ItemClass is RocketLauncherGuidedInventoryItemClass)
				{
					RocketLauncherGuidedInventoryItemClass rocketLauncher = (RocketLauncherGuidedInventoryItemClass)this.ActiveInventoryItem.ItemClass;
					rocketLauncher.CheckIfLocked(gameTime.ElapsedGameTime);
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
				Vector2 origin = new Vector2(0f, 0f);
				Vector2 playerPosition = new Vector2(this._game.LocalPlayer.LocalPosition.X, this._game.LocalPlayer.LocalPosition.Z);
				this.currentDistanceTraveled = (int)Vector2.Distance(origin, playerPosition);
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
				Vector3 northVec = new Vector3(0f, 0f, 1f);
				Vector3 playerVec = new Vector3(playerPosition.X, 0f, playerPosition.Y);
				this.compassRotation = northVec.AngleBetween(playerVec);
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
						float recoverRate = (this.StaminaRecoverTimer.Expired ? this.StaminaRecoverRate : (this.StaminaRecoverRate * this.StaminaDamagedRecoveryModifier));
						this.PlayerStamina += recoverRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
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
				int depth = this._game._terrain.DepthUnderGround(this._game.LocalPlayer.LocalPosition);
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
						if (depth < 4)
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
			Matrix i = this.LocalPlayer.FPSCamera.LocalToWorld;
			GameMessageManager.Instance.Send(GameMessageType.LocalPlayerFiredGun, null, gun);
			if (gun is RocketLauncherBaseInventoryItemClass)
			{
				RocketLauncherBaseInventoryItemClass rl = gun as RocketLauncherBaseInventoryItemClass;
				FireRocketMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, i, gun.ID, rl.IsGuided());
				return;
			}
			if (gun is PumpShotgunInventoryItemClass || gun is LaserShotgunClass)
			{
				Angle innaccuracy;
				if (this.LocalPlayer.Shouldering)
				{
					innaccuracy = gun.ShoulderedMinAccuracy + this.InnaccuracyMultiplier * (gun.ShoulderedMaxAccuracy - gun.ShoulderedMinAccuracy);
				}
				else
				{
					innaccuracy = gun.MinInnaccuracy + this.InnaccuracyMultiplier * (gun.MaxInnaccuracy - gun.MinInnaccuracy);
				}
				ShotgunShotMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, i, innaccuracy, gun.ID, gun.NeedsDropCompensation);
				return;
			}
			Angle innaccuracy2;
			if (this.LocalPlayer.Shouldering)
			{
				innaccuracy2 = gun.ShoulderedMinAccuracy + this.InnaccuracyMultiplier * (gun.ShoulderedMaxAccuracy - gun.ShoulderedMinAccuracy);
			}
			else
			{
				innaccuracy2 = gun.MinInnaccuracy + this.InnaccuracyMultiplier * (gun.MaxInnaccuracy - gun.MinInnaccuracy);
			}
			GunshotMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, i, innaccuracy2, gun.ID, gun.NeedsDropCompensation);
		}

		public void MeleePlayer(InventoryItem tool, Player player)
		{
			MeleePlayerMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, player.Gamer, this.ActiveInventoryItem.ItemClass.ID, this.ConstructionProbe.GetIntersection());
			ParticleEmitter smokeEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			smokeEmitter.Reset();
			smokeEmitter.Emitting = true;
			TracerManager.Instance.Scene.Children.Add(smokeEmitter);
			smokeEmitter.LocalPosition = this.ConstructionProbe.GetIntersection();
			smokeEmitter.DrawPriority = 900;
			if (tool.InflictDamage())
			{
				this.PlayerInventory.Remove(tool);
			}
		}

		public void Melee(InventoryItem tool)
		{
			byte localid;
			if (this._game.LocalPlayer != null && this._game.LocalPlayer.ValidGamer)
			{
				localid = this._game.LocalPlayer.Gamer.Id;
			}
			else
			{
				localid = byte.MaxValue;
			}
			this.ConstructionProbe.EnemyHit.TakeDamage(this.ConstructionProbe.GetIntersection(), Vector3.Normalize(this.ConstructionProbe._end - this.ConstructionProbe._start), tool.ItemClass, localid);
			ParticleEmitter smokeEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			smokeEmitter.Reset();
			smokeEmitter.Emitting = true;
			TracerManager.Instance.Scene.Children.Add(smokeEmitter);
			smokeEmitter.LocalPosition = this.ConstructionProbe.GetIntersection();
			smokeEmitter.DrawPriority = 900;
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
			BlockTypeEnum blockType = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
			if (BlockType.GetType(blockType).CanBeDug && this.IsValidDigTarget(blockType, this.ConstructionProbe._worldIndex))
			{
				DigMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, false, this.ConstructionProbe.GetIntersection(), this.ConstructionProbe._inNormal, blockType);
				if (effective)
				{
					if (BlockType.IsContainer(blockType))
					{
						DestroyCrateMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex);
						Crate crate;
						if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(this.ConstructionProbe._worldIndex, out crate))
						{
							crate.EjectContents();
						}
					}
					if (blockType == BlockTypeEnum.TeleportStation)
					{
						DestroyCustomBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, blockType);
						this.LocalPlayer.RemoveTeleportStationObject(this.ConstructionProbe._worldIndex + Vector3.Zero);
					}
					CastleMinerZGame.Instance.PlayerStats.DugBlock(blockType);
					GameMessageManager.Instance.Send(GameMessageType.LocalPlayerMinedBlock, blockType, tool);
					if (BlockType.ShouldDropLoot(blockType))
					{
						PossibleLootType.ProcessLootBlockOutput(blockType, this.ConstructionProbe._worldIndex);
					}
					else
					{
						IntVector3 pickupPosition = this.ConstructionProbe._worldIndex;
						if (BlockType.IsUpperDoor(blockType))
						{
							pickupPosition += new IntVector3(0, -1, 0);
						}
						InventoryItem item = tool.CreatesWhenDug(blockType, pickupPosition);
						if (item != null)
						{
							PickupManager.Instance.CreatePickup(item, pickupPosition + Vector3.Zero, false, false);
						}
					}
					AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.Empty);
					for (BlockFace bf = BlockFace.POSX; bf < BlockFace.NUM_FACES; bf++)
					{
						IntVector3 nextBlockLocation = BlockTerrain.Instance.GetNeighborIndex(this.ConstructionProbe._worldIndex, bf);
						BlockTypeEnum bbt = BlockTerrain.Instance.GetBlockWithChanges(nextBlockLocation);
						if (BlockType.GetType(bbt).Facing == bf)
						{
							InventoryItem newBlockItem = BlockInventoryItemClass.CreateBlockItem(BlockType.GetType(bbt).ParentBlockType, 1, nextBlockLocation);
							PickupManager.Instance.CreatePickup(newBlockItem, IntVector3.ToVector3(nextBlockLocation) + new Vector3(0.5f, 0.5f, 0.5f), false, false);
							AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, nextBlockLocation, BlockTypeEnum.Empty);
							this.CheckToRemoveDoorConnections(BlockType.GetType(bbt).ParentBlockType, nextBlockLocation);
						}
					}
					this.CheckToRemoveDoorConnections(blockType, this.ConstructionProbe._worldIndex);
				}
				else
				{
					GameMessageManager.Instance.Send(GameMessageType.LocalPlayerPickedAtBlock, blockType, tool);
				}
				if (tool.InflictDamage())
				{
					this.PlayerInventory.Remove(tool);
				}
			}
		}

		private void CheckToRemoveDoorConnections(BlockTypeEnum removedBlockType, IntVector3 location)
		{
			IntVector3 lowerDoorPosition = IntVector3.Zero;
			if (BlockType.IsLowerDoor(removedBlockType))
			{
				IntVector3 upperDoorPosition = location + new IntVector3(0, 1, 0);
				lowerDoorPosition = location;
				AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, upperDoorPosition, BlockTypeEnum.Empty);
			}
			if (BlockType.IsUpperDoor(removedBlockType))
			{
				lowerDoorPosition = location + new IntVector3(0, -1, 0);
				AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, lowerDoorPosition, BlockTypeEnum.Empty);
			}
			if (lowerDoorPosition != IntVector3.Zero)
			{
				DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, lowerDoorPosition, removedBlockType);
			}
		}

		public IntVector3 Build(BlockInventoryItem blockItem, bool validateOnly = false)
		{
			IntVector3 failLocation = IntVector3.Zero;
			if (!this.ConstructionProbe._collides)
			{
				return failLocation;
			}
			IntVector3 addSpot = BlockTerrain.Instance.GetNeighborIndex(this.ConstructionProbe._worldIndex, this.ConstructionProbe._inFace);
			BlockTypeEnum blockType = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
			if (!BlockTerrain.Instance.OkayToBuildHere(addSpot))
			{
				return failLocation;
			}
			BlockType btype = BlockType.GetType(blockType);
			if (!btype.CanBuildOn)
			{
				return failLocation;
			}
			if (!blockItem.CanPlaceHere(addSpot, this.ConstructionProbe._inFace))
			{
				return failLocation;
			}
			bool buildAway = true;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer nwg = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (nwg != null)
					{
						Player p = (Player)nwg.Tag;
						if (p != null)
						{
							p.MovementProbe.SkipEmbedded = false;
							Vector3 pos = p.WorldPosition;
							pos.Y += 0.05f;
							p.MovementProbe.Init(pos, pos, p.PlayerAABB);
							if (BlockTerrain.Instance.ProbeTouchesBlock(p.MovementProbe, addSpot))
							{
								buildAway = false;
								break;
							}
						}
					}
				}
				if (buildAway)
				{
					if (validateOnly)
					{
						return addSpot;
					}
					BoundingBox bb = default(BoundingBox);
					bb.Min = IntVector3.ToVector3(addSpot) + new Vector3(0.01f, 0.01f, 0.01f);
					bb.Max = bb.Min + new Vector3(0.98f, 0.98f, 0.98f);
					if (!EnemyManager.Instance.TouchesZombies(bb))
					{
						BlockTypeEnum blockType2 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
						DigMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, true, this.ConstructionProbe.GetIntersection(), this.ConstructionProbe._inNormal, blockType2);
						blockItem.AlterBlock(this.LocalPlayer, addSpot, this.ConstructionProbe._inFace);
						return addSpot;
					}
				}
			}
			return failLocation;
		}

		public override void OnLostFocus()
		{
			new GameTime(TimeSpan.FromSeconds(0.001), TimeSpan.FromSeconds(0.001));
			CastleMinerZGame.Instance._controllerMapping.ClearAllControls();
			base.OnLostFocus();
		}

		protected void DrawPlayerList(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont font = this._game._medFont;
			Rectangle safeArea = Screen.Adjuster.ScreenRect;
			spriteBatch.Begin();
			int playerCount = this._game.CurrentNetworkSession.AllGamers.Count;
			int maxPlayers = this._game.CurrentNetworkSession.MaxGamers;
			this._builder.Length = 0;
			this._builder.Append(Strings.Players + " ").Concat(playerCount).Append("/")
				.Concat(maxPlayers);
			Vector2 size = font.MeasureString(this._builder);
			spriteBatch.DrawOutlinedText(font, this._builder, new Vector2((float)safeArea.Right - size.X, (float)safeArea.Bottom - size.Y), Color.White, Color.Black, 2);
			float[] columnWidths = new float[1];
			float totalRowWidth = 0f;
			totalRowWidth += (columnWidths[0] = font.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float offset = ((float)Screen.Adjuster.ScreenRect.Width - totalRowWidth) / 2f;
			float yloc = (float)safeArea.Top;
			spriteBatch.DrawOutlinedText(font, Strings.Player, new Vector2(offset, yloc), Color.Orange, Color.Black, 2);
			yloc += (float)font.LineSpacing;
			for (int i = 0; i < this._game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer gamer = this._game.CurrentNetworkSession.AllGamers[i];
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					spriteBatch.DrawOutlinedText(font, player.Gamer.Gamertag, new Vector2(offset, yloc), player.Gamer.IsLocal ? Color.Red : Color.White, Color.Black, 2);
					if (player.Profile != null)
					{
						float profieSize = (float)font.LineSpacing * 0.9f;
						float buff = (float)font.LineSpacing - profieSize;
						if (player.GamerPicture != null)
						{
							spriteBatch.Draw(player.GamerPicture, new Rectangle((int)(offset - (float)font.LineSpacing), (int)(yloc + buff), (int)profieSize, (int)profieSize), Color.White);
						}
					}
					yloc += (float)font.LineSpacing;
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
			float speed = 5f;
			Vector2 MaxValues = new Vector2(this.maxGunCameraShift, this.maxGunCameraShift);
			if (this.LocalPlayer.Shouldering)
			{
				MaxValues /= 2f;
			}
			Vector2 IntendedValues = controllerMapping.Aiming * MaxValues;
			if (!this.LocalPlayer.Dead)
			{
				this.GunEyePointCameraLocation += (IntendedValues - this.GunEyePointCameraLocation) * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
				this.GunEyePointCameraLocation.Y = MathHelper.Clamp(this.GunEyePointCameraLocation.Y, 0f, 100f);
			}
			GunInventoryItem ActiveGun = this.ActiveInventoryItem as GunInventoryItem;
			if (ActiveGun != null)
			{
				if (!this.LocalPlayer.InContact)
				{
					if (this.InnaccuracyMultiplier < 1f)
					{
						this.InnaccuracyMultiplier += ActiveGun.GunClass.InnaccuracySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
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
						float innaccuracySpeed = MathHelper.Max(Math.Abs(controllerMapping.Movement.X), Math.Abs(controllerMapping.Movement.Y)) * ActiveGun.GunClass.InnaccuracySpeed;
						this.InnaccuracyMultiplier += innaccuracySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
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
						this.InnaccuracyMultiplier -= ActiveGun.GunClass.InnaccuracySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
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
				if (!this.LocalPlayer.Dead && controllerMapping.Activate.Pressed && this.ConstructionProbe._collides)
				{
					BlockTypeEnum blockType = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex);
					if (BlockType.IsContainer(blockType))
					{
						Crate crate = CastleMinerZGame.Instance.CurrentWorld.GetCrate(this.ConstructionProbe._worldIndex, true);
						this._crateScreen.CurrentCrate = crate;
						this._game.GameScreen._uiGroup.PushScreen(this._crateScreen);
						SoundManager.Instance.PlayInstance("Click");
					}
					else if (BlockType.IsSpawnerClickable(blockType))
					{
						Spawner spawner = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(this.ConstructionProbe._worldIndex, true, blockType);
						if (this._game.IsOnlineGame)
						{
							BroadcastTextMessage.Send(this._game.MyNetworkGamer, this.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_triggered_a_monster_spawner);
						}
						spawner.StartSpawner(blockType);
						SoundManager.Instance.PlayInstance("Click");
					}
					else
					{
						BlockTypeEnum blockTypeEnum = blockType;
						switch (blockTypeEnum)
						{
						case BlockTypeEnum.NormalLowerDoorClosedZ:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorOpenZ, BlockTypeEnum.NormalUpperDoorOpen);
							goto IL_0EDE;
						case BlockTypeEnum.NormalLowerDoorClosedX:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorOpenX, BlockTypeEnum.NormalUpperDoorOpen);
							goto IL_0EDE;
						case BlockTypeEnum.NormalLowerDoor:
							break;
						case BlockTypeEnum.NormalUpperDoorClosed:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, true);
							AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.NormalUpperDoorOpen);
							BlockTypeEnum blockType2 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (blockType2 == BlockTypeEnum.NormalLowerDoorClosedX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorOpenX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorOpenZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							goto IL_0EDE;
						}
						case BlockTypeEnum.NormalLowerDoorOpenZ:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorClosedZ, BlockTypeEnum.NormalUpperDoorClosed);
							goto IL_0EDE;
						case BlockTypeEnum.NormalLowerDoorOpenX:
							this.UseDoor(BlockTypeEnum.NormalLowerDoorClosedX, BlockTypeEnum.NormalUpperDoorClosed);
							goto IL_0EDE;
						case BlockTypeEnum.NormalUpperDoorOpen:
						{
							DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, false);
							AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.NormalUpperDoorClosed);
							BlockTypeEnum blockType3 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
							if (blockType3 == BlockTypeEnum.NormalLowerDoorOpenX)
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorClosedX);
							}
							else
							{
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.NormalLowerDoorClosedZ);
							}
							SoundManager.Instance.PlayInstance("Click");
							goto IL_0EDE;
						}
						case BlockTypeEnum.TNT:
							this.SetFuseForExplosive(this.ConstructionProbe._worldIndex, ExplosiveTypes.TNT);
							goto IL_0EDE;
						case BlockTypeEnum.C4:
							this.SetFuseForExplosive(this.ConstructionProbe._worldIndex, ExplosiveTypes.C4);
							goto IL_0EDE;
						default:
							switch (blockTypeEnum)
							{
							case BlockTypeEnum.StrongLowerDoorClosedZ:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorOpenZ, BlockTypeEnum.StrongUpperDoorOpen);
								goto IL_0EDE;
							case BlockTypeEnum.StrongLowerDoorClosedX:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorOpenX, BlockTypeEnum.StrongUpperDoorOpen);
								goto IL_0EDE;
							case BlockTypeEnum.StrongLowerDoor:
								break;
							case BlockTypeEnum.StrongUpperDoorClosed:
							{
								DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, true);
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.StrongUpperDoorOpen);
								BlockTypeEnum blockType4 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
								if (blockType4 == BlockTypeEnum.StrongLowerDoorClosedX)
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorOpenX);
								}
								else
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorOpenZ);
								}
								SoundManager.Instance.PlayInstance("Click");
								goto IL_0EDE;
							}
							case BlockTypeEnum.StrongLowerDoorOpenZ:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorClosedZ, BlockTypeEnum.StrongUpperDoorClosed);
								goto IL_0EDE;
							case BlockTypeEnum.StrongLowerDoorOpenX:
								this.UseDoor(BlockTypeEnum.StrongLowerDoorClosedX, BlockTypeEnum.StrongUpperDoorClosed);
								goto IL_0EDE;
							case BlockTypeEnum.StrongUpperDoorOpen:
							{
								DoorOpenCloseMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, false);
								AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex, BlockTypeEnum.StrongUpperDoorClosed);
								BlockTypeEnum blockType5 = InGameHUD.GetBlock(this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0));
								if (blockType5 == BlockTypeEnum.StrongLowerDoorOpenX)
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorClosedX);
								}
								else
								{
									AlterBlockMessage.Send((LocalNetworkGamer)this.LocalPlayer.Gamer, this.ConstructionProbe._worldIndex + new IntVector3(0, -1, 0), BlockTypeEnum.StrongLowerDoorClosedZ);
								}
								SoundManager.Instance.PlayInstance("Click");
								goto IL_0EDE;
							}
							default:
								switch (blockTypeEnum)
								{
								case BlockTypeEnum.EnemySpawnAltar:
									goto IL_0EDE;
								case BlockTypeEnum.TeleportStation:
									this.PlayerInventory.ShowTeleportStationMenu(this.ConstructionProbe._worldIndex + Vector3.Zero);
									goto IL_0EDE;
								}
								break;
							}
							break;
						}
						SoundManager.Instance.PlayInstance("Error");
					}
				}
				IL_0EDE:
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
			Explosive tnt = new Explosive(location, explosiveType);
			if (!this._tntWaitingToExplode.Contains(tnt))
			{
				this._tntWaitingToExplode.Add(tnt);
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
