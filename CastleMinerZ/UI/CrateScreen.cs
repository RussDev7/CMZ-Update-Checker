using System;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Net.GamerServices;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class CrateScreen : Screen
	{
		private bool _selectorInCrateGrid
		{
			get
			{
				return this._setSelectorToCrate && this._selectedLocation.Y < 8;
			}
			set
			{
				this._setSelectorToCrate = value;
			}
		}

		private int SelectorIndex
		{
			get
			{
				return this._selectedLocation.X + this._selectedLocation.Y * 4;
			}
		}

		private int GetTrayIndexFromRow(int row)
		{
			return (row - 8) / 2;
		}

		public bool IsSelectedSlotLocked()
		{
			if (this._selectorInCrateGrid && this._game.CurrentNetworkSession != null)
			{
				foreach (NetworkGamer gamer in this._game.CurrentNetworkSession.RemoteGamers)
				{
					if (gamer.Tag != null)
					{
						Player player = (Player)gamer.Tag;
						if (player.FocusCrate == this.CurrentCrate.Location && player.FocusCrateItem == this._selectedLocation)
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		public bool IsSlotLocked(int index)
		{
			foreach (NetworkGamer gamer in this._game.CurrentNetworkSession.RemoteGamers)
			{
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					int playerLockedIndex = player.FocusCrateItem.X + player.FocusCrateItem.Y * 4;
					if (player.FocusCrate == this.CurrentCrate.Location && playerLockedIndex == index)
					{
						return true;
					}
				}
			}
			return false;
		}

		public InventoryItem SelectedItem
		{
			get
			{
				int index = this._selectedLocation.X + this._selectedLocation.Y * 4;
				if (this._selectorInCrateGrid)
				{
					if (this.CurrentCrate != null && this.CurrentCrate.Inventory != null && index >= 0 && index < this.CurrentCrate.Inventory.Length)
					{
						return this.CurrentCrate.Inventory[index];
					}
					return null;
				}
				else if (this._selectedLocation.Y < 8)
				{
					if (this._hud != null && this._hud.PlayerInventory != null && this._hud.PlayerInventory.Inventory != null && index >= 0 && index < this._hud.PlayerInventory.Inventory.Length)
					{
						return this._hud.PlayerInventory.Inventory[index];
					}
					return null;
				}
				else
				{
					int trayIndex = this.GetTrayIndexFromRow(this._selectedLocation.Y);
					if (this._hud == null || this._hud.PlayerInventory == null)
					{
						return null;
					}
					return this._hud.PlayerInventory.TrayManager.GetTrayItem(trayIndex, this._selectedLocation.X);
				}
			}
			set
			{
				if (this._selectedLocation.Y < 8)
				{
					int index = this._selectedLocation.X + this._selectedLocation.Y * 4;
					if (this._selectorInCrateGrid)
					{
						if (this.CurrentCrate != null && this.CurrentCrate.Inventory != null && index >= 0 && index < this.CurrentCrate.Inventory.Length)
						{
							ItemCrateMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, value, this.CurrentCrate, index);
							return;
						}
					}
					else if (this._hud != null && this._hud.PlayerInventory != null && this._hud.PlayerInventory.Inventory != null && index >= 0 && index < this._hud.PlayerInventory.Inventory.Length)
					{
						this._hud.PlayerInventory.Inventory[index] = value;
						return;
					}
				}
				else
				{
					this._hud.PlayerInventory.TrayManager.SetTrayItem(this.GetTrayIndexFromRow(this._selectedLocation.Y), this._selectedLocation.X, value);
				}
			}
		}

		public override void OnPushed()
		{
			this._selectedLocation = new Point(this._game.GameScreen.HUD.PlayerInventory.SelectedInventoryIndex, 8);
			this._selectorInCrateGrid = false;
			CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this._selectorInCrateGrid ? this.CurrentCrate.Location : IntVector3.Zero, this._selectedLocation);
			this._nextPopUpItem = null;
			this._popUpFadeInTimer.Reset();
			this._popUpFadeOutTimer.Update(TimeSpan.FromSeconds(2.0));
			this._hitTestTrue = false;
			base.OnPushed();
		}

		public int InventoryHitTest(Point p)
		{
			for (int i = 0; i < this._inventoryItemLocations.Length; i++)
			{
				if (this._inventoryItemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public int CrateHitTest(Point p)
		{
			for (int i = 0; i < this._crateItemLocations.Length; i++)
			{
				if (this._crateItemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public void ForceClose()
		{
			if (this._holdingItem != null)
			{
				this._hud.PlayerInventory.AddInventoryItem(this._holdingItem, false);
				this._holdingItem = null;
			}
			if (this.SelectedItem != null && this._selectorInCrateGrid && !this.IsSelectedSlotLocked())
			{
				Vector3 v = this._game.LocalPlayer.LocalPosition;
				v.Y += 1f;
				PickupManager.Instance.CreatePickup(this.SelectedItem, v, true, false);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			base.PopMe();
			CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			this._game.GameScreen.ShowBlockPicker();
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (this._hud.LocalPlayer.Dead)
			{
				base.PopMe();
			}
			else if (this.CurrentCrate == null)
			{
				base.PopMe();
				this._game.GameScreen.ShowBlockPicker();
			}
			else if (this.SelectedItem != null && this.SelectedItem.StackCount < 1)
			{
				this.SelectedItem = null;
			}
			base.Update(game, gameTime);
		}

		public CrateScreen(CastleMinerZGame game, InGameHUD hud)
			: base(true, false)
		{
			this._hud = hud;
			this._game = game;
			this._bigFont = this._game._medFont;
			this._smallFont = this._game._smallFont;
			this._gridSelector = this._game._uiSprites["Selector"];
			this._grid = this._game._uiSprites["InventoryGrid"];
			this._gridSprite = this._game._uiSprites["HudGrid"];
			Rectangle screenArea = Screen.Adjuster.ScreenRect;
			Vector2 gridSize = new Vector2((float)(this._grid.Width * 2 + 16), (float)this._grid.Height) * Screen.Adjuster.ScaleFactor.Y;
			this._backgroundRect = new Rectangle((int)((float)screenArea.Center.X - gridSize.X / 2f), (int)((float)screenArea.Center.Y - gridSize.Y / 2f), (int)gridSize.X, (int)gridSize.Y);
			this._popUpFadeOutTimer.Update(TimeSpan.FromSeconds(2.0));
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			this._game.GameScreen.HUD.console.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.Begin();
			this._game.GameScreen.HUD.DrawPlayerStats(spriteBatch);
			this._game.GameScreen.HUD.DrawDistanceStr(spriteBatch);
			SpriteFont smallFont = CastleMinerZGame.Instance._smallFont;
			Vector2 gridSize = new Vector2((float)(this._grid.Width * 2 + 16), (float)this._grid.Height) * Screen.Adjuster.ScaleFactor.Y;
			this._backgroundRect = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Center.X - gridSize.X / 2f), (int)((float)Screen.Adjuster.ScreenRect.Center.Y - gridSize.Y / 2f), (int)gridSize.X, (int)gridSize.Y);
			this._grid.Draw(spriteBatch, new Rectangle(this._backgroundRect.X, this._backgroundRect.Y, (int)((float)this._grid.Width * Screen.Adjuster.ScaleFactor.Y), (int)gridSize.Y), Color.White);
			this._grid.Draw(spriteBatch, new Rectangle(this._backgroundRect.Right - (int)((float)this._grid.Width * Screen.Adjuster.ScaleFactor.Y), this._backgroundRect.Y, (int)((float)this._grid.Width * Screen.Adjuster.ScaleFactor.Y), (int)gridSize.Y), Color.White);
			PlayerInventory inventory = this._hud.PlayerInventory;
			Vector2 inventoryOrigin = new Vector2((float)this._backgroundRect.X + 299f * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRect.Y + 33f * Screen.Adjuster.ScaleFactor.Y);
			float itemSpacing = 59f * Screen.Adjuster.ScaleFactor.Y;
			int size = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					Vector2 itemLocation = inventoryOrigin + itemSpacing * new Vector2((float)x, (float)y);
					this._inventoryItemLocations[x + y * 4] = new Rectangle((int)itemLocation.X, (int)itemLocation.Y, (int)itemSpacing, (int)itemSpacing);
					InventoryItem item = inventory.Inventory[y * 4 + x];
					if (item != null && item != this._holdingItem && (this._holdingItem == null || this.SelectedItem != item || this._mousePointerActive))
					{
						item.Draw2D(spriteBatch, new Rectangle((int)itemLocation.X, (int)itemLocation.Y, size, size));
					}
				}
			}
			int trayIndex = 0;
			int maxTrayCount = 2;
			Rectangle[] gridRects = new Rectangle[maxTrayCount];
			Size traySize = new Size((int)((float)this._gridSprite.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._gridSprite.Height * Screen.Adjuster.ScaleFactor.Y));
			gridRects[trayIndex] = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - traySize.Width, Screen.Adjuster.ScreenRect.Bottom - traySize.Height - (int)(5f * Screen.Adjuster.ScaleFactor.Y), traySize.Width, traySize.Height);
			Rectangle gridRect = gridRects[trayIndex];
			this._gridSprite.Draw(spriteBatch, gridRect, Color.White);
			this.DrawItemTray(trayIndex, 32, gridRect, size, itemSpacing, inventory, spriteBatch);
			trayIndex = 1;
			gridRects[trayIndex] = new Rectangle(Screen.Adjuster.ScreenRect.Center.X, Screen.Adjuster.ScreenRect.Bottom - traySize.Height - (int)(5f * Screen.Adjuster.ScaleFactor.Y), traySize.Width, traySize.Height);
			gridRect = gridRects[trayIndex];
			this._gridSprite.Draw(spriteBatch, gridRect, Color.White);
			this.DrawItemTray(trayIndex, 40, gridRect, size, itemSpacing, inventory, spriteBatch);
			InventoryItem[] crateInventory = this.CurrentCrate.Inventory;
			Vector2 crateInventoryOrigin = new Vector2((float)this._backgroundRect.X + 17f * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRect.Y + 33f * Screen.Adjuster.ScaleFactor.Y);
			spriteBatch.DrawOutlinedText(this._smallFont, this.CRATE, new Vector2((float)this._backgroundRect.X + (85f - this._smallFont.MeasureString(this.CRATE).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRect.Y + 5f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			spriteBatch.DrawOutlinedText(this._smallFont, this.INVENTORY, new Vector2((float)this._backgroundRect.X + (369f - this._smallFont.MeasureString(this.INVENTORY).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRect.Y + 5f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			for (int y2 = 0; y2 < 8; y2++)
			{
				for (int x2 = 0; x2 < 4; x2++)
				{
					Vector2 itemLocation2 = crateInventoryOrigin + itemSpacing * new Vector2((float)x2, (float)y2);
					this._crateItemLocations[x2 + y2 * 4] = new Rectangle((int)itemLocation2.X, (int)itemLocation2.Y, (int)itemSpacing, (int)itemSpacing);
					InventoryItem item2 = crateInventory[y2 * 4 + x2];
					if (item2 != null && item2 != this._holdingItem && (this._holdingItem == null || this.SelectedItem != item2 || this._mousePointerActive))
					{
						item2.Draw2D(spriteBatch, new Rectangle((int)itemLocation2.X, (int)itemLocation2.Y, size, size));
					}
				}
			}
			Vector2 selectorOrigin;
			if (this._selectorInCrateGrid)
			{
				selectorOrigin = crateInventoryOrigin;
			}
			else
			{
				selectorOrigin = inventoryOrigin;
			}
			Rectangle selectorLocation;
			if (this._selectedLocation.Y < 8)
			{
				Vector2 loc = selectorOrigin + itemSpacing * new Vector2((float)this._selectedLocation.X, (float)this._selectedLocation.Y);
				selectorLocation = new Rectangle((int)loc.X, (int)loc.Y, size, size);
			}
			else
			{
				Rectangle targetTrayGridRect = gridRects[this.GetTrayIndexFromRow(this._selectedLocation.Y)];
				selectorLocation = new Rectangle(targetTrayGridRect.Left + (int)(7f * Screen.Adjuster.ScaleFactor.Y + itemSpacing * (float)this._selectedLocation.X), (int)((float)targetTrayGridRect.Top + 7f * Screen.Adjuster.ScaleFactor.Y), size, size);
			}
			if (!this._mousePointerActive || this._hitTestTrue)
			{
				this._gridSelector.Draw(spriteBatch, selectorLocation, (this._holdingItem == null) ? Color.White : Color.Red);
			}
			if (this._holdingItem != null)
			{
				if (this._mousePointerActive)
				{
					this._holdingItem.Draw2D(spriteBatch, new Rectangle((int)((float)this._mousePointerLocation.X - itemSpacing / 2f), (int)((float)this._mousePointerLocation.Y - itemSpacing / 2f), (int)itemSpacing, (int)itemSpacing));
				}
				else
				{
					this._holdingItem.Draw2D(spriteBatch, new Rectangle(selectorLocation.X, selectorLocation.Y, (int)itemSpacing, (int)itemSpacing));
				}
			}
			InventoryItem selectedItem = this.SelectedItem;
			InventoryItem holdingItem = this._holdingItem;
			if (this.SelectedItem != null && this._holdingItem == null && this._hitTestTrue && this._popUpTimer.Expired)
			{
				Color color = new Color(this._popUpFadeInTimer.PercentComplete, this._popUpFadeInTimer.PercentComplete, this._popUpFadeInTimer.PercentComplete, this._popUpFadeInTimer.PercentComplete);
				Color blackColor = new Color(0f, 0f, 0f, this._popUpFadeInTimer.PercentComplete);
				spriteBatch.DrawOutlinedText(this._smallFont, this._popUpItem.Name, this._popUpLocation, color, blackColor, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			}
			else if (!this._popUpFadeOutTimer.Expired)
			{
				Color color2 = new Color(1f - this._popUpFadeOutTimer.PercentComplete, 1f - this._popUpFadeOutTimer.PercentComplete, 1f - this._popUpFadeOutTimer.PercentComplete, 1f - this._popUpFadeOutTimer.PercentComplete);
				Color blackColor2 = new Color(0f, 0f, 0f, 1f - this._popUpFadeOutTimer.PercentComplete);
				spriteBatch.DrawOutlinedText(this._smallFont, this._popUpItem.Name, this._popUpLocation, color2, blackColor2, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		private void DrawItemTray(int trayIndex, int columnOffset, Rectangle gridRect, int size, float itemSpacing, PlayerInventory inventory, SpriteBatch spriteBatch)
		{
			for (int x = 0; x < 8; x++)
			{
				Vector2 itemLocation = new Vector2(itemSpacing * (float)x + (float)gridRect.Left + 2f * Screen.Adjuster.ScaleFactor.Y, (float)gridRect.Top + 2f * Screen.Adjuster.ScaleFactor.Y);
				this._inventoryItemLocations[x + columnOffset] = new Rectangle((int)itemLocation.X, (int)itemLocation.Y, size, size);
				InventoryItem item = inventory.TrayManager.GetTrayItem(trayIndex, x);
				if (item != null && item != this._holdingItem && (this._holdingItem == null || this.SelectedItem != item || this._mousePointerActive))
				{
					item.Draw2D(spriteBatch, new Rectangle((int)itemLocation.X, (int)itemLocation.Y, size, size));
				}
			}
		}

		private bool SwapSelectedItemLocation()
		{
			if (!this._selectorInCrateGrid)
			{
				for (int i = 0; i < this.CurrentCrate.Inventory.Length; i++)
				{
					if (this.CurrentCrate.Inventory[i] != null && !this.IsSlotLocked(i))
					{
						int stackcount = this.SelectedItem.StackCount;
						InventoryItem selectedItem = this.SelectedItem;
						this.CurrentCrate.Inventory[i].Stack(selectedItem);
						this.SelectedItem = selectedItem;
						if (selectedItem.StackCount != stackcount)
						{
							ItemCrateMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this.CurrentCrate.Inventory[i], this.CurrentCrate, i);
						}
					}
				}
				if (this.SelectedItem.StackCount <= 0)
				{
					this.SelectedItem = null;
					return true;
				}
				for (int j = 0; j < this.CurrentCrate.Inventory.Length; j++)
				{
					if (this.CurrentCrate.Inventory[j] == null && !this.IsSlotLocked(j))
					{
						ItemCrateMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this.SelectedItem, this.CurrentCrate, j);
						this.SelectedItem = null;
						return true;
					}
				}
			}
			else
			{
				this.SelectedItem = this._hud.PlayerInventory.TrayManager.Stack(this.SelectedItem);
				for (int k = 0; k < this._hud.PlayerInventory.Inventory.Length; k++)
				{
					if (this._hud.PlayerInventory.Inventory[k] != null)
					{
						InventoryItem selectedItem2 = this.SelectedItem;
						this._hud.PlayerInventory.Inventory[k].Stack(selectedItem2);
						this.SelectedItem = selectedItem2;
					}
				}
				if (this.SelectedItem.StackCount <= 0)
				{
					this.SelectedItem = null;
					this._hud.PlayerInventory.DiscoverRecipies();
					return true;
				}
				if (this._hud.PlayerInventory.TrayManager.PlaceInEmptySlot(this.SelectedItem))
				{
					this._hud.PlayerInventory.DiscoverRecipies();
					this.SelectedItem = null;
					return true;
				}
				for (int l = 0; l < this._hud.PlayerInventory.Inventory.Length; l++)
				{
					if (this._hud.PlayerInventory.Inventory[l] == null)
					{
						this._hud.PlayerInventory.Inventory[l] = this.SelectedItem;
						this._hud.PlayerInventory.DiscoverRecipies();
						this.SelectedItem = null;
						return true;
					}
				}
			}
			return false;
		}

		private bool SwapHoldingItemLocation()
		{
			if (!this._selectorInCrateGrid)
			{
				for (int i = 0; i < this.CurrentCrate.Inventory.Length; i++)
				{
					if (this.CurrentCrate.Inventory[i] != null && !this.IsSlotLocked(i))
					{
						int stackcount = this._holdingItem.StackCount;
						this.CurrentCrate.Inventory[i].Stack(this._holdingItem);
						if (this._holdingItem.StackCount != stackcount)
						{
							ItemCrateMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this.CurrentCrate.Inventory[i], this.CurrentCrate, i);
						}
					}
				}
				if (this._holdingItem.StackCount <= 0)
				{
					this._holdingItem = null;
					return true;
				}
				for (int j = 0; j < this.CurrentCrate.Inventory.Length; j++)
				{
					if (this.CurrentCrate.Inventory[j] == null && !this.IsSlotLocked(j))
					{
						ItemCrateMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this._holdingItem, this.CurrentCrate, j);
						this._holdingItem = null;
						return true;
					}
				}
			}
			else
			{
				this._holdingItem = this._hud.PlayerInventory.TrayManager.Stack(this._holdingItem);
				for (int k = 0; k < this._hud.PlayerInventory.Inventory.Length; k++)
				{
					if (this._hud.PlayerInventory.Inventory[k] != null)
					{
						this._hud.PlayerInventory.Inventory[k].Stack(this._holdingItem);
					}
				}
				if (this._holdingItem.StackCount <= 0)
				{
					this._holdingItem = null;
					this._hud.PlayerInventory.DiscoverRecipies();
					return true;
				}
				if (this._hud.PlayerInventory.TrayManager.PlaceInEmptySlot(this._holdingItem))
				{
					this._hud.PlayerInventory.DiscoverRecipies();
					this._holdingItem = null;
					return true;
				}
				for (int l = 0; l < this._hud.PlayerInventory.Inventory.Length; l++)
				{
					if (this._hud.PlayerInventory.Inventory[l] == null)
					{
						this._hud.PlayerInventory.Inventory[l] = this._holdingItem;
						this._hud.PlayerInventory.DiscoverRecipies();
						this._holdingItem = null;
						return true;
					}
				}
			}
			return false;
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (this.CurrentCrate.Destroyed)
			{
				this.ForceClose();
				return false;
			}
			if (this._holdingItem == null && this.SelectedItem != null && this._hitTestTrue)
			{
				if (this.SelectedItem == this._nextPopUpItem)
				{
					if (this._popUpTimer.Expired)
					{
						if (!this._popUpFadeInTimer.Expired)
						{
							this._popUpFadeInTimer.Update(gameTime.ElapsedGameTime);
							this._popUpFadeOutTimer = new OneShotTimer(this._popUpFadeInTimer.ElaspedTime);
						}
					}
					else
					{
						this._popUpFadeOutTimer.Update(gameTime.ElapsedGameTime);
						this._popUpTimer.Update(gameTime.ElapsedGameTime);
						if (this._popUpTimer.Expired)
						{
							this._popUpItem = this.SelectedItem;
							Vector2 size = this._smallFont.MeasureString(this._popUpItem.Name) * Screen.Adjuster.ScaleFactor.Y;
							this._popUpLocation = new Point(inputManager.Mouse.Position.X, inputManager.Mouse.Position.Y + (int)size.Y);
							if ((float)this._popUpLocation.Y + size.Y > (float)Screen.Adjuster.ScreenRect.Bottom)
							{
								this._popUpLocation.Y = this._popUpLocation.Y - (int)(size.Y * 2f);
							}
							if ((float)this._popUpLocation.X + size.X > (float)Screen.Adjuster.ScreenRect.Right)
							{
								this._popUpLocation.X = (int)((float)Screen.Adjuster.ScreenRect.Right - size.X);
							}
						}
					}
				}
				else
				{
					this._nextPopUpItem = this.SelectedItem;
					this._popUpTimer.Reset();
					this._popUpFadeInTimer.Reset();
					this._popUpFadeOutTimer.Update(gameTime.ElapsedGameTime);
				}
			}
			else
			{
				this._popUpFadeOutTimer.Update(gameTime.ElapsedGameTime);
				this._nextPopUpItem = null;
				this._popUpFadeInTimer.Reset();
			}
			if (controller.PressedButtons.Y || ((inputManager.Keyboard.IsKeyDown(Keys.LeftShift) || inputManager.Keyboard.IsKeyDown(Keys.RightShift)) && inputManager.Mouse.LeftButtonPressed && this._hitTestTrue))
			{
				if (this._holdingItem != null)
				{
					if (this.SwapHoldingItemLocation())
					{
						SoundManager.Instance.PlayInstance("Click");
					}
				}
				else if (this.SelectedItem != null && !this.IsSelectedSlotLocked() && this.SwapSelectedItemLocation())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (controller.PressedButtons.A || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || (inputManager.Mouse.LeftButtonPressed && this._hitTestTrue))
			{
				if (!this.IsSelectedSlotLocked())
				{
					if (this._holdingItem == null)
					{
						if (this.SelectedItem != null)
						{
							this._holdingItem = this.SelectedItem;
							if (this._selectorInCrateGrid)
							{
								this.SelectedItem = null;
							}
							else
							{
								this._hud.PlayerInventory.Remove(this._holdingItem);
							}
						}
						SoundManager.Instance.PlayInstance("Click");
					}
					else
					{
						InventoryItem selectedItem = this.SelectedItem;
						if (selectedItem != null && selectedItem.CanStack(this._holdingItem))
						{
							selectedItem.Stack(this._holdingItem);
							this.SelectedItem = selectedItem;
							if (this._holdingItem.StackCount == 0)
							{
								this._holdingItem = null;
							}
						}
						else
						{
							this.SelectedItem = this._holdingItem;
							this._holdingItem = selectedItem;
						}
						SoundManager.Instance.PlayInstance("Click");
					}
				}
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.PressedButtons.RightStick || (inputManager.Mouse.RightButtonPressed && this._hitTestTrue))
			{
				if (!this.IsSelectedSlotLocked())
				{
					if (this._holdingItem == null)
					{
						if (this.SelectedItem != null)
						{
							SoundManager.Instance.PlayInstance("Click");
							if (this.SelectedItem.StackCount == 1)
							{
								this._holdingItem = this.SelectedItem;
								if (this._selectorInCrateGrid)
								{
									this.SelectedItem = null;
								}
								else
								{
									this._hud.PlayerInventory.Remove(this._holdingItem);
								}
							}
							else
							{
								InventoryItem selectedItem2 = this.SelectedItem;
								this._holdingItem = selectedItem2.Split();
								this.SelectedItem = selectedItem2;
							}
						}
					}
					else if (this._holdingItem != null)
					{
						SoundManager.Instance.PlayInstance("Click");
						if (this.SelectedItem != null)
						{
							if (this._holdingItem.ItemClass == this.SelectedItem.ItemClass)
							{
								if (inputManager.Mouse.RightButtonPressed)
								{
									if (this.SelectedItem.StackCount < this.SelectedItem.MaxStackCount)
									{
										if (this._holdingItem.StackCount > 1)
										{
											InventoryItem selectedItem3 = this.SelectedItem;
											selectedItem3.Stack(this._holdingItem.PopOneItem());
											this.SelectedItem = selectedItem3;
										}
										else
										{
											this.SelectedItem.Stack(this._holdingItem);
											this._holdingItem = null;
										}
									}
								}
								else if (this.SelectedItem.StackCount > 1)
								{
									InventoryItem selectedItem4 = this.SelectedItem;
									InventoryItem item = selectedItem4.Split();
									this._holdingItem.Stack(item);
									selectedItem4.Stack(item);
									this.SelectedItem = selectedItem4;
								}
								else if (this._holdingItem.StackCount < this._holdingItem.MaxStackCount)
								{
									this._holdingItem.Stack(this.SelectedItem);
									this.SelectedItem = null;
								}
							}
							else
							{
								InventoryItem item2 = this.SelectedItem;
								this.SelectedItem = this._holdingItem;
								this._holdingItem = item2;
							}
						}
						else if (inputManager.Mouse.RightButtonPressed)
						{
							if (this._holdingItem.StackCount > 1)
							{
								this.SelectedItem = this._holdingItem.PopOneItem();
							}
							else
							{
								this.SelectedItem = this._holdingItem;
								this._holdingItem = null;
							}
						}
						else if (this._holdingItem.StackCount > 1)
						{
							this.SelectedItem = this._holdingItem.Split();
						}
						else
						{
							this.SelectedItem = this._holdingItem;
							this._holdingItem = null;
						}
					}
				}
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.PressedButtons.Start)
			{
				this._game.GameScreen.ShowInGameMenu();
				SoundManager.Instance.PlayInstance("Click");
			}
			else if (controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				SoundManager.Instance.PlayInstance("Click");
				if (this._holdingItem != null)
				{
					this._hud.PlayerInventory.AddInventoryItem(this._holdingItem, false);
					this._holdingItem = null;
				}
				base.PopMe();
				CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			}
			else if (controller.PressedButtons.B)
			{
				SoundManager.Instance.PlayInstance("Click");
				if (this._holdingItem != null)
				{
					this._hud.PlayerInventory.AddInventoryItem(this._holdingItem, false);
					this._holdingItem = null;
				}
				else
				{
					base.PopMe();
					CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
				}
			}
			else if (inputManager.Mouse.LeftButtonPressed && !this._backgroundRect.Contains(inputManager.Mouse.Position) && this._holdingItem != null)
			{
				this._mousePointerActive = true;
				SoundManager.Instance.PlayInstance("Click");
				Vector3 v = this._game.LocalPlayer.LocalPosition;
				v.Y += 1f;
				PickupManager.Instance.CreatePickup(this._holdingItem, v, true, false);
				SoundManager.Instance.PlayInstance("dropitem");
				this._holdingItem = null;
			}
			else if (inputManager.Mouse.RightButtonPressed && !this._backgroundRect.Contains(inputManager.Mouse.Position) && this._holdingItem != null)
			{
				InventoryItem dropItem;
				if (this._holdingItem.StackCount > 1)
				{
					dropItem = this._holdingItem.PopOneItem();
				}
				else
				{
					dropItem = this._holdingItem;
					this._holdingItem = null;
				}
				this._mousePointerActive = true;
				SoundManager.Instance.PlayInstance("Click");
				Vector3 v2 = this._game.LocalPlayer.LocalPosition;
				v2.Y += 1f;
				PickupManager.Instance.CreatePickup(dropItem, v2, true, false);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			else if (controller.PressedButtons.X || inputManager.Keyboard.WasKeyPressed(Keys.Q))
			{
				if (!this.IsSelectedSlotLocked())
				{
					SoundManager.Instance.PlayInstance("Click");
					if (this._holdingItem != null)
					{
						Vector3 v3 = this._game.LocalPlayer.LocalPosition;
						v3.Y += 1f;
						PickupManager.Instance.CreatePickup(this._holdingItem, v3, true, false);
						SoundManager.Instance.PlayInstance("dropitem");
						this._holdingItem = null;
					}
					else if (this.SelectedItem != null)
					{
						if (this._selectorInCrateGrid)
						{
							Vector3 v4 = this._game.LocalPlayer.LocalPosition;
							v4.Y += 1f;
							PickupManager.Instance.CreatePickup(this.SelectedItem, v4, true, false);
							SoundManager.Instance.PlayInstance("dropitem");
							this.SelectedItem = null;
						}
						else
						{
							this._hud.PlayerInventory.DropItem(this.SelectedItem);
						}
					}
				}
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.CurrentState.ThumbSticks.Right.Y < -0.2f && controller.LastState.ThumbSticks.Right.Y >= -0.2f)
			{
				this.itemCountWaitScrollTimer.Reset();
				this.itemCountAutoScrollTimer.Reset();
				if (this.ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel < 0 && this._hitTestTrue)
			{
				this._mousePointerActive = true;
				this.itemCountWaitScrollTimer.Reset();
				this.itemCountAutoScrollTimer.Reset();
				if (this.ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel > 0 && this._hitTestTrue)
			{
				this._mousePointerActive = true;
				this.itemCountWaitScrollTimer.Reset();
				this.itemCountAutoScrollTimer.Reset();
				if (this.ItemCountUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (controller.CurrentState.ThumbSticks.Right.Y > 0.2f && controller.LastState.ThumbSticks.Right.Y <= 0.2f)
			{
				this.itemCountWaitScrollTimer.Reset();
				this.itemCountAutoScrollTimer.Reset();
				if (this.ItemCountUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			if (controller.PressedDPad.Down || (controller.CurrentState.ThumbSticks.Left.Y < -0.2f && controller.LastState.ThumbSticks.Left.Y >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Up))
			{
				this._mousePointerActive = false;
				this.waitScrollTimer.Reset();
				this.autoScrollTimer.Reset();
				if (this.SelectDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this._selectorInCrateGrid ? this.CurrentCrate.Location : IntVector3.Zero, this._selectedLocation);
			}
			if (controller.PressedDPad.Up || (controller.CurrentState.ThumbSticks.Left.Y > 0.2f && controller.LastState.ThumbSticks.Left.Y <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Down))
			{
				this._mousePointerActive = false;
				this.waitScrollTimer.Reset();
				this.autoScrollTimer.Reset();
				if (this.SelectUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this._selectorInCrateGrid ? this.CurrentCrate.Location : IntVector3.Zero, this._selectedLocation);
			}
			if (controller.PressedButtons.LeftShoulder || controller.PressedDPad.Left || (controller.CurrentState.ThumbSticks.Left.X < -0.2f && controller.LastState.ThumbSticks.Left.X >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Left))
			{
				this._mousePointerActive = false;
				this.waitScrollTimer.Reset();
				this.autoScrollTimer.Reset();
				if (this.SelectLeft())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this._selectorInCrateGrid ? this.CurrentCrate.Location : IntVector3.Zero, this._selectedLocation);
			}
			if (controller.PressedButtons.RightShoulder || controller.PressedDPad.Right || (controller.CurrentState.ThumbSticks.Left.X > 0.2f && controller.LastState.ThumbSticks.Left.X <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Right))
			{
				this._mousePointerActive = false;
				this.waitScrollTimer.Reset();
				this.autoScrollTimer.Reset();
				if (this.SelectRight())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this._selectorInCrateGrid ? this.CurrentCrate.Location : IntVector3.Zero, this._selectedLocation);
			}
			this.itemCountWaitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (this.itemCountWaitScrollTimer.Expired && !controller.PressedButtons.A)
			{
				if (controller.CurrentState.ThumbSticks.Right.Y < -0.2f)
				{
					this.itemCountAutoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.itemCountAutoScrollTimer.Expired)
					{
						this.itemCountAutoScrollTimer.Reset();
						if (this.ItemCountDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Right.Y > 0.2f)
				{
					this.itemCountAutoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.itemCountAutoScrollTimer.Expired)
					{
						this.itemCountAutoScrollTimer.Reset();
						if (this.ItemCountUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			this.waitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (this.waitScrollTimer.Expired)
			{
				if (controller.CurrentState.ThumbSticks.Left.Y < -0.2f || inputManager.Keyboard.IsKeyDown(Keys.Up))
				{
					this.autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.autoScrollTimer.Expired)
					{
						this.autoScrollTimer.Reset();
						if (this.SelectDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.Y > 0.2f || inputManager.Keyboard.IsKeyDown(Keys.Down))
				{
					this.autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.autoScrollTimer.Expired)
					{
						this.autoScrollTimer.Reset();
						if (this.SelectUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.X < -0.2f || inputManager.Keyboard.IsKeyDown(Keys.Left))
				{
					this.autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.autoScrollTimer.Expired)
					{
						this.autoScrollTimer.Reset();
						if (this.SelectLeft())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.X > 0.2f || inputManager.Keyboard.IsKeyDown(Keys.Right))
				{
					this.autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.autoScrollTimer.Expired)
					{
						this.autoScrollTimer.Reset();
						if (this.SelectRight())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			if (inputManager.Mouse.Position != inputManager.Mouse.LastPosition)
			{
				this._mousePointerActive = true;
				this._mousePointerLocation = inputManager.Mouse.Position;
				this.hoverItem = this.InventoryHitTest(inputManager.Mouse.Position);
				if (this.hoverItem >= 0)
				{
					this._hitTestTrue = true;
					this._selectedLocation.Y = this.hoverItem / 4;
					if (this._selectedLocation.Y > 7)
					{
						this._selectedLocation.X = this.hoverItem % 8;
					}
					else
					{
						this._selectedLocation.X = this.hoverItem % 4;
					}
					this._selectorInCrateGrid = false;
				}
				else
				{
					this.hoverItem = this.CrateHitTest(inputManager.Mouse.Position);
					if (this.hoverItem >= 0)
					{
						this._hitTestTrue = true;
						this._selectedLocation.Y = this.hoverItem / 4;
						if (this._selectedLocation.Y > 7)
						{
							this._selectedLocation.Y = 8;
							this._selectedLocation.X = this.hoverItem % 8;
						}
						else
						{
							this._selectedLocation.X = this.hoverItem % 4;
						}
						this._selectorInCrateGrid = true;
						if (this._selectedLocation != this._lastSelectedLocation)
						{
							this._lastSelectedLocation = this._selectedLocation;
							CrateFocusMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer, this.CurrentCrate.Location, this._selectedLocation);
						}
					}
					else
					{
						this._hitTestTrue = false;
					}
				}
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		public bool ItemCountDown()
		{
			if (this.IsSelectedSlotLocked())
			{
				return false;
			}
			if (this._holdingItem == null)
			{
				return false;
			}
			if (this.SelectedItem == null)
			{
				if (this._holdingItem.StackCount == 1)
				{
					this.SelectedItem = this._holdingItem;
					this._holdingItem = null;
				}
				else if (this._holdingItem.StackCount > 1)
				{
					this.SelectedItem = this._holdingItem.PopOneItem();
				}
			}
			else
			{
				if (this._holdingItem.ItemClass != this.SelectedItem.ItemClass || this.SelectedItem.StackCount >= this.SelectedItem.MaxStackCount)
				{
					return false;
				}
				InventoryItem selectedItem = this.SelectedItem;
				if (this._holdingItem.StackCount == 1)
				{
					selectedItem.Stack(this._holdingItem);
					this._holdingItem = null;
				}
				else if (this._holdingItem.StackCount > 1)
				{
					selectedItem.Stack(this._holdingItem.PopOneItem());
				}
				this.SelectedItem = selectedItem;
			}
			return true;
		}

		public bool ItemCountUp()
		{
			if (this.IsSelectedSlotLocked())
			{
				return false;
			}
			if (this._holdingItem == null)
			{
				if (this.SelectedItem == null)
				{
					return false;
				}
				if (this.SelectedItem.StackCount == 1)
				{
					this._holdingItem = this.SelectedItem;
					if (this._selectorInCrateGrid)
					{
						this.SelectedItem = null;
					}
					else
					{
						this._hud.PlayerInventory.Remove(this._holdingItem);
					}
				}
				else if (this.SelectedItem.StackCount > 1)
				{
					InventoryItem selectedItem = this.SelectedItem;
					this._holdingItem = selectedItem.PopOneItem();
					this.SelectedItem = selectedItem;
				}
			}
			else
			{
				if (this.SelectedItem == null)
				{
					return false;
				}
				if (this._holdingItem.ItemClass != this.SelectedItem.ItemClass || this._holdingItem.StackCount >= this._holdingItem.MaxStackCount)
				{
					return false;
				}
				InventoryItem selectedItem2 = this.SelectedItem;
				if (this.SelectedItem.StackCount == 1)
				{
					this._holdingItem.Stack(selectedItem2);
					this.SelectedItem = null;
				}
				else if (this.SelectedItem.StackCount > 1)
				{
					this._holdingItem.Stack(selectedItem2.PopOneItem());
					this.SelectedItem = selectedItem2;
				}
			}
			return true;
		}

		public bool SelectDown()
		{
			this._selectedLocation.Y = this._selectedLocation.Y + 1;
			if (this._selectedLocation.Y > 8)
			{
				this._selectedLocation.Y = 0;
			}
			return true;
		}

		public bool SelectUp()
		{
			this._selectedLocation.Y = this._selectedLocation.Y - 1;
			if (this._selectedLocation.Y < 0)
			{
				this._selectedLocation.Y = 8;
			}
			return true;
		}

		public bool SelectLeft()
		{
			this._selectedLocation.X = this._selectedLocation.X - 1;
			if (this._selectedLocation.X < 0)
			{
				this._selectorInCrateGrid = !this._selectorInCrateGrid;
				this._selectedLocation.X = 3;
			}
			return true;
		}

		public bool SelectRight()
		{
			this._selectedLocation.X = this._selectedLocation.X + 1;
			if (this._selectedLocation.X > 3)
			{
				this._selectorInCrateGrid = !this._selectorInCrateGrid;
				this._selectedLocation.X = 0;
			}
			return true;
		}

		private const int Columns = 4;

		private const int Rows = 8;

		private const int ItemSize = 64;

		public Crate CurrentCrate;

		private int hoverItem;

		private bool _setSelectorToCrate;

		private Sprite _grid;

		private Sprite _gridSelector;

		private Sprite _gridSprite;

		private CastleMinerZGame _game;

		private SpriteFont _bigFont;

		private SpriteFont _smallFont;

		private InGameHUD _hud;

		private Point _selectedLocation = new Point(0, 0);

		private Point _lastSelectedLocation = new Point(0, 0);

		private InventoryItem _holdingItem;

		private Rectangle[] _crateItemLocations = new Rectangle[32];

		private Rectangle[] _inventoryItemLocations = new Rectangle[48];

		private Rectangle _backgroundRect;

		private bool _mousePointerActive;

		private Point _mousePointerLocation = default(Point);

		private bool _hitTestTrue;

		private OneShotTimer _popUpTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		private OneShotTimer _popUpFadeInTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer _popUpFadeOutTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private Point _popUpLocation;

		private InventoryItem _popUpItem;

		private InventoryItem _nextPopUpItem;

		private StringBuilder stringBuilder = new StringBuilder();

		private string CRATE = Strings.Crate.ToUpper();

		private string INVENTORY = Strings.Inventory.ToUpper();

		private OneShotTimer waitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer autoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private OneShotTimer itemCountWaitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer itemCountAutoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));
	}
}
