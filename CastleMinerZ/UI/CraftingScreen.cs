using System;
using System.Collections.Generic;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class CraftingScreen : UIControlScreen
	{
		private int GetTrayIndexFromRow(int row)
		{
			return (row - 8) / 2;
		}

		public InventoryItem SelectedItem
		{
			get
			{
				if (this._selectedLocation.Y < 8)
				{
					return this._hud.PlayerInventory.Inventory[this._selectedLocation.X + this._selectedLocation.Y * 4];
				}
				if (this._selectedLocation.Y >= 8)
				{
					return this._hud.PlayerInventory.TrayManager.GetTrayItem(this.GetTrayIndexFromRow(this._selectedLocation.Y), this._selectedLocation.X);
				}
				return null;
			}
			set
			{
				if (this._selectedLocation.Y < 8)
				{
					this._hud.PlayerInventory.Inventory[this._selectedLocation.X + this._selectedLocation.Y * 4] = value;
				}
				if (this._selectedLocation.Y >= 8)
				{
					this._hud.PlayerInventory.TrayManager.SetTrayItem(this.GetTrayIndexFromRow(this._selectedLocation.Y), this._selectedLocation.X, value);
				}
			}
		}

		public int SelectedRecipeIndex
		{
			get
			{
				return this._hud.PlayerInventory.DiscoveredRecipies.IndexOf(this._selectedRecipe);
			}
			set
			{
				if (this._hud.PlayerInventory.DiscoveredRecipies.Count > 0 && value >= 0 && value < this._hud.PlayerInventory.DiscoveredRecipies.Count)
				{
					this._selectedRecipe = this._hud.PlayerInventory.DiscoveredRecipies[value];
					return;
				}
				this._selectedRecipe = null;
			}
		}

		public CraftingScreen(CastleMinerZGame game, InGameHUD hud)
			: base(false)
		{
			this._game = game;
			this._hud = hud;
			this._bigFont = this._game._medFont;
			this._smallFont = this._game._smallFont;
			this._background = this._game._uiSprites["BlockUIBack"];
			this._gridSelector = this._game._uiSprites["Selector"];
			this._gridSquare = this._game._uiSprites["SingleGrid"];
			this._tier2Back = this._game._uiSprites["Tier2Back"];
			this._gridSprite = this._game._uiSprites["HudGrid"];
			this._craftSelector = CastleMinerZGame.Instance._uiSprites["CraftSelector"];
			this._buyToCraftDialog = new PCDialogScreen(Strings.Purchase_Game, Strings.You_must_purchase_the_game_to_craft_this_item, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._buyToCraftDialog.UseDefaultValues();
			this._selectedItemNameText = new TextRegionElement(this._game._medFont);
			this._selectedItemNameText.Color = CMZColors.MenuAqua;
			this._selectedItemDescriptionText = new TextRegionElement(this._game._smallFont);
			this._selectedItemDescriptionText.Color = CMZColors.MenuBlue;
			this._selectedItemIngredientsText = new TextRegionElement(this._game._smallFont);
			this._selectedItemIngredientsText.Color = CMZColors.MenuBlue;
			this._selectedItemNameText.OutlineWidth = (this._selectedItemDescriptionText.OutlineWidth = 0);
			this._selectedItemIngredientsText.OutlineWidth = 0;
			Tier1Item Materials = new Tier1Item(Strings.Materials, new Vector2(14f, 29f), this._game._uiSprites["MaterialsIcon"], this);
			Materials.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Ores, Recipe.RecipeTypes.Ores, Materials, this),
				new Tier2Item(Strings.Components, Recipe.RecipeTypes.Components, Materials, this)
			});
			this._tier1Items.Add(Materials);
			this.SelectedTier1Item = Materials;
			Tier1Item Tools = new Tier1Item(Strings.Tools, new Vector2(14f, 77f), this._game._uiSprites["ToolsIcon"], this);
			Tools.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Pickaxes, Recipe.RecipeTypes.Pickaxes, Tools, this),
				new Tier2Item(Strings.Spades, Recipe.RecipeTypes.Spades, Tools, this),
				new Tier2Item(Strings.Axes, Recipe.RecipeTypes.Axes, Tools, this),
				new Tier2Item(Strings.Special, Recipe.RecipeTypes.SpecialTools, Tools, this)
			});
			this._tier1Items.Add(Tools);
			Tier1Item Weapons = new Tier1Item(Strings.Weapons, new Vector2(14f, 126f), this._game._uiSprites["WeaponsIcon"], this);
			Weapons.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Ammo, Recipe.RecipeTypes.Ammo, Weapons, this),
				new Tier2Item(Strings.Knives, Recipe.RecipeTypes.Knives, Weapons, this),
				new Tier2Item(Strings.Pistols, Recipe.RecipeTypes.Pistols, Weapons, this),
				new Tier2Item(Strings.Shotguns, Recipe.RecipeTypes.Shotguns, Weapons, this),
				new Tier2Item(Strings.Rifles, Recipe.RecipeTypes.Rifles, Weapons, this),
				new Tier2Item(Strings.Assault_Rifles, Recipe.RecipeTypes.AssaultRifles, Weapons, this),
				new Tier2Item(Strings.SMG_s, Recipe.RecipeTypes.SMGs, Weapons, this),
				new Tier2Item(Strings.LMG_s, Recipe.RecipeTypes.LMGs, Weapons, this),
				new Tier2Item(Strings.RPG, Recipe.RecipeTypes.RPG, Weapons, this),
				new Tier2Item(Strings.Explosives, Recipe.RecipeTypes.Explosives, Weapons, this),
				new Tier2Item(Strings.Laser_Swords, Recipe.RecipeTypes.LaserSwords, Weapons, this)
			});
			this._tier1Items.Add(Weapons);
			Tier1Item Structures = new Tier1Item(Strings.Structures, new Vector2(14f, 172f), this._game._uiSprites["StructuresIcon"], this);
			Structures.SetItems(new List<Tier2Item>
			{
				new Tier2Item(Strings.Walls, Recipe.RecipeTypes.Walls, Structures, this),
				new Tier2Item(Strings.Containers, Recipe.RecipeTypes.Containers, Structures, this),
				new Tier2Item(Strings.Spawn_Points, Recipe.RecipeTypes.SpawnPoints, Structures, this),
				new Tier2Item(Strings.Other, Recipe.RecipeTypes.OtherStructure, Structures, this),
				new Tier2Item(Strings.Doors, Recipe.RecipeTypes.Doors, Structures, this)
			});
			this._tier1Items.Add(Structures);
			this._popUpFadeOutTimer.Update(TimeSpan.FromSeconds(2.0));
		}

		private PlayerInventory Inventory
		{
			get
			{
				return this._hud.PlayerInventory;
			}
		}

		private List<Recipe> DiscoveredReceipes
		{
			get
			{
				return this._hud.PlayerInventory.DiscoveredRecipies;
			}
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(this._background, this._backgroundRectangle, Color.White);
			this._game.GameScreen.HUD.DrawPlayerStats(spriteBatch);
			this._game.GameScreen.HUD.DrawDistanceStr(spriteBatch);
			for (int i = 0; i < this._tier1Items.Count; i++)
			{
				this._tier1Items[i].Draw(spriteBatch);
			}
			this._selectedItemNameText.Size = new Vector2(150f * Screen.Adjuster.ScaleFactor.Y, 10f * Screen.Adjuster.ScaleFactor.Y);
			this._selectedItemDescriptionText.Size = new Vector2(150f * Screen.Adjuster.ScaleFactor.Y, 60f * Screen.Adjuster.ScaleFactor.Y);
			this._selectedItemIngredientsText.Size = new Vector2(72f * Screen.Adjuster.ScaleFactor.Y, 130f * Screen.Adjuster.ScaleFactor.Y);
			this._selectedItemNameText.Draw(device, spriteBatch, gameTime, false);
			this._selectedItemDescriptionText.Draw(device, spriteBatch, gameTime, false);
			this._selectedItemIngredientsText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.DrawOutlinedText(this._smallFont, this.CRAFTING, new Vector2((float)this._backgroundRectangle.X + (84f - this._smallFont.MeasureString(this.CRAFTING).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRectangle.Y + 5f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			spriteBatch.DrawOutlinedText(this._smallFont, this.INVENTORY, new Vector2((float)this._backgroundRectangle.X + (717f - this._smallFont.MeasureString(this.INVENTORY).X / 2f) * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRectangle.Y + 70f * Screen.Adjuster.ScaleFactor.Y), CMZColors.MenuOrange, Color.Black, 1, Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
			PlayerInventory inventory = this._hud.PlayerInventory;
			Vector2 inventoryOrigin = new Vector2((float)this._backgroundRectangle.X + 646f * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRectangle.Y + 96f * Screen.Adjuster.ScaleFactor.Y);
			float itemSpacing = 59f * Screen.Adjuster.ScaleFactor.Y;
			int size = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					Vector2 itemLocation = inventoryOrigin + itemSpacing * new Vector2((float)x, (float)y);
					this._inventoryItemLocations[x + y * 4] = new Rectangle((int)itemLocation.X, (int)itemLocation.Y, (int)itemSpacing, (int)itemSpacing);
					InventoryItem item = inventory.Inventory[y * 4 + x];
					if (item != null && item != this._holdingItem)
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
			Vector2 selectorOrigin = inventoryOrigin;
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
			if (this._hitTestTrue)
			{
				this._gridSelector.Draw(spriteBatch, selectorLocation, (this._holdingItem == null) ? Color.White : Color.Red);
			}
			if (this._holdingItem != null)
			{
				this._holdingItem.Draw2D(spriteBatch, new Rectangle((int)((float)this._mousePointerLocation.X - itemSpacing / 2f), (int)((float)this._mousePointerLocation.Y - itemSpacing / 2f), (int)itemSpacing, (int)itemSpacing));
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
				if (item != null && item != this._holdingItem)
				{
					item.Draw2D(spriteBatch, new Rectangle((int)itemLocation.X, (int)itemLocation.Y, size, size));
				}
			}
		}

		public override void OnPushed()
		{
			this._nextPopUpItem = null;
			this._popUpFadeInTimer.Reset();
			this._popUpFadeOutTimer.Update(TimeSpan.FromSeconds(0.2));
			this._hitTestTrue = false;
			base.OnPushed();
		}

		public int HitTest(Point p)
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

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				if (this._holdingItem != null)
				{
					this._hud.PlayerInventory.AddInventoryItem(this._holdingItem, false);
					this._holdingItem = null;
				}
				else
				{
					base.PopMe();
				}
				SoundManager.Instance.PlayInstance("Click");
			}
			else if (inputManager.Keyboard.WasKeyPressed(Keys.E))
			{
				if (this._holdingItem != null)
				{
					this._hud.PlayerInventory.AddInventoryItem(this._holdingItem, false);
					this._holdingItem = null;
				}
				SoundManager.Instance.PlayInstance("Click");
				base.PopMe();
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
			if (this._holdingItem == null)
			{
				for (int i = 0; i < this._tier1Items.Count; i++)
				{
					if (this._tier1Items[i].CheckInput(inputManager))
					{
						this.SelectedTier1Item = this._tier1Items[i];
					}
				}
			}
			if ((inputManager.Keyboard.IsKeyDown(Keys.LeftShift) || inputManager.Keyboard.IsKeyDown(Keys.RightShift)) && inputManager.Mouse.LeftButtonPressed && this.HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (this._holdingItem != null)
				{
					if (this._selectedLocation.Y < 8)
					{
						int leftoverStackCount = this._hud.PlayerInventory.AddItemToTray(this._holdingItem);
						if (leftoverStackCount == 0)
						{
							this._holdingItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (this._holdingItem.StackCount != leftoverStackCount)
						{
							this._holdingItem.StackCount = leftoverStackCount;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
					else
					{
						int leftoverStackCount2 = this._hud.PlayerInventory.AddItemToInventory(this._holdingItem);
						if (leftoverStackCount2 == 0)
						{
							this._holdingItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (this._holdingItem.StackCount != leftoverStackCount2)
						{
							this._holdingItem.StackCount = leftoverStackCount2;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (this.SelectedItem != null)
				{
					if (this._selectedLocation.Y < 8)
					{
						int leftoverStackCount3 = this._hud.PlayerInventory.AddItemToTray(this.SelectedItem);
						if (leftoverStackCount3 == 0)
						{
							this.SelectedItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (this.SelectedItem.StackCount != leftoverStackCount3)
						{
							this.SelectedItem.StackCount = leftoverStackCount3;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
					else
					{
						int leftoverStackCount4 = this._hud.PlayerInventory.AddItemToInventory(this.SelectedItem);
						if (leftoverStackCount4 == 0)
						{
							this.SelectedItem = null;
							SoundManager.Instance.PlayInstance("Click");
						}
						else if (this.SelectedItem.StackCount != leftoverStackCount4)
						{
							this.SelectedItem.StackCount = leftoverStackCount4;
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			else if (inputManager.Mouse.LeftButtonPressed && this.HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (this._holdingItem == null)
				{
					if (this.SelectedItem != null)
					{
						this._holdingItem = this.SelectedItem;
						this._hud.PlayerInventory.Remove(this._holdingItem);
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
			else if (inputManager.Mouse.RightButtonPressed && this.HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (this._holdingItem == null)
				{
					if (this.SelectedItem != null)
					{
						SoundManager.Instance.PlayInstance("Click");
						if (this.SelectedItem.StackCount == 1)
						{
							this._holdingItem = this.SelectedItem;
							this._hud.PlayerInventory.Remove(this._holdingItem);
						}
						else
						{
							this._holdingItem = this.SelectedItem.Split();
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
										this.SelectedItem.Stack(this._holdingItem.PopOneItem());
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
								InventoryItem item = this.SelectedItem.Split();
								this._holdingItem.Stack(item);
								this.SelectedItem.Stack(item);
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
			else if (inputManager.Keyboard.WasKeyPressed(Keys.Q))
			{
				SoundManager.Instance.PlayInstance("Click");
				if (this._holdingItem != null)
				{
					Vector3 v = this._game.LocalPlayer.LocalPosition;
					v.Y += 1f;
					PickupManager.Instance.CreatePickup(this._holdingItem, v, true, false);
					SoundManager.Instance.PlayInstance("dropitem");
					this._holdingItem = null;
				}
				else if (this.SelectedItem != null)
				{
					this._hud.PlayerInventory.DropItem(this.SelectedItem);
				}
			}
			else if (inputManager.Mouse.LeftButtonPressed && !this._backgroundRectangle.Contains(inputManager.Mouse.Position) && this._holdingItem != null)
			{
				SoundManager.Instance.PlayInstance("Click");
				Vector3 v2 = this._game.LocalPlayer.LocalPosition;
				v2.Y += 1f;
				PickupManager.Instance.CreatePickup(this._holdingItem, v2, true, false);
				SoundManager.Instance.PlayInstance("dropitem");
				this._holdingItem = null;
			}
			else if (inputManager.Mouse.RightButtonPressed && !this._backgroundRectangle.Contains(inputManager.Mouse.Position) && this._holdingItem != null)
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
				SoundManager.Instance.PlayInstance("Click");
				Vector3 v3 = this._game.LocalPlayer.LocalPosition;
				v3.Y += 1f;
				PickupManager.Instance.CreatePickup(dropItem, v3, true, false);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			else if (inputManager.Mouse.DeltaWheel < 0 && this.HitTest(inputManager.Mouse.Position) >= 0)
			{
				if (this.ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel > 0 && this.HitTest(inputManager.Mouse.Position) >= 0 && this.ItemCountUp())
			{
				SoundManager.Instance.PlayInstance("Click");
			}
			if (inputManager.Mouse.Position != inputManager.Mouse.LastPosition)
			{
				this._mousePointerLocation = inputManager.Mouse.Position;
				int hoverItem = this.HitTest(inputManager.Mouse.Position);
				if (hoverItem >= 0)
				{
					this._hitTestTrue = true;
					this._selectedLocation.Y = hoverItem / 4;
					if (this._selectedLocation.Y < 8)
					{
						this._selectedLocation.X = hoverItem % 4;
					}
					else
					{
						this._selectedLocation.X = hoverItem % 8;
					}
				}
				else
				{
					this._hitTestTrue = false;
				}
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		public bool ItemCountDown()
		{
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
				else
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
				if (this._holdingItem.StackCount == 1)
				{
					this.SelectedItem.Stack(this._holdingItem);
					this._holdingItem = null;
				}
				else
				{
					this.SelectedItem.Stack(this._holdingItem.PopOneItem());
				}
			}
			return true;
		}

		public bool ItemCountUp()
		{
			if (this._holdingItem == null)
			{
				if (this.SelectedItem == null)
				{
					return false;
				}
				if (this.SelectedItem.StackCount == 1)
				{
					this._holdingItem = this.SelectedItem;
					this._hud.PlayerInventory.Remove(this._holdingItem);
				}
				else
				{
					this._holdingItem = this.SelectedItem.PopOneItem();
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
				if (this.SelectedItem.StackCount == 1)
				{
					this._holdingItem.Stack(this.SelectedItem);
					this.SelectedItem = null;
				}
				else
				{
					this._holdingItem.Stack(this.SelectedItem.PopOneItem());
				}
			}
			return true;
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			InGameHUD.Instance.ExternalUpdate(game, gameTime);
			if (this._hud.LocalPlayer.Dead)
			{
				if (this._holdingItem != null)
				{
					this._hud.PlayerInventory.AddInventoryItem(this._holdingItem, false);
					this._holdingItem = null;
				}
				base.PopMe();
			}
			Size bgRectSize = new Size((int)((float)this._background.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._background.Height * Screen.Adjuster.ScaleFactor.Y));
			this._backgroundRectangle = new Rectangle(Screen.Adjuster.ScreenRect.Center.X - bgRectSize.Width / 2, (int)(55f * Screen.Adjuster.ScaleFactor.Y), bgRectSize.Width, bgRectSize.Height);
			this._selectedItemNameText.Text = this.SelectedTier1Item.SelectedTier2Item.SelectedItem.Name;
			this._selectedItemDescriptionText.Location = new Vector2((float)this._backgroundRectangle.X + 32f * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRectangle.Y + (float)(367 + this._game._medFont.LineSpacing * this._selectedItemNameText.NumberOfLines) * Screen.Adjuster.ScaleFactor.Y);
			this._selectedItemDescriptionText.Text = this.SelectedTier1Item.SelectedTier2Item.SelectedItem.Description;
			this.sbuilder.Clear();
			this.sbuilder.Append(Strings.Components);
			this.sbuilder.Append(":\n");
			this.sbuilder.Append(this.SelectedTier1Item.SelectedTier2Item.SelectedItemIngredients[0].Name);
			for (int i = 1; i < this.SelectedTier1Item.SelectedTier2Item.SelectedItemIngredients.Count; i++)
			{
				this.sbuilder.Append(", ");
				this.sbuilder.Append(this.SelectedTier1Item.SelectedTier2Item.SelectedItemIngredients[i].Name);
			}
			this._selectedItemIngredientsText.Location = new Vector2((float)this._backgroundRectangle.X + 170f * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRectangle.Y + 235f * Screen.Adjuster.ScaleFactor.Y);
			this._selectedItemIngredientsText.Text = this.sbuilder.ToString();
			if (this._prevScreenRect != Screen.Adjuster.ScreenRect)
			{
				for (int j = 0; j < this._tier1Items.Count; j++)
				{
					this._tier1Items[j].UpdateScaledLocation(this._backgroundRectangle);
				}
				this._selectedItemNameText.Location = new Vector2((float)this._backgroundRectangle.X + 32f * Screen.Adjuster.ScaleFactor.Y, (float)this._backgroundRectangle.Y + 367f * Screen.Adjuster.ScaleFactor.Y);
			}
			this._prevScreenRect = Screen.Adjuster.ScreenRect;
			base.OnUpdate(game, gameTime);
		}

		private const int Columns = 4;

		private const int Rows = 8;

		private const int ItemSize = 64;

		private Sprite _background;

		public Sprite _gridSelector;

		public Sprite _craftSelector;

		public Sprite _gridSquare;

		private Sprite _gridSprite;

		public Sprite _tier2Back;

		private CastleMinerZGame _game;

		private SpriteFont _bigFont;

		private SpriteFont _smallFont;

		private InGameHUD _hud;

		private Recipe _selectedRecipe;

		private int _selectedIngredientIndex;

		private PCDialogScreen _buyToCraftDialog;

		private List<Tier1Item> _tier1Items = new List<Tier1Item>();

		public Tier1Item SelectedTier1Item;

		private Rectangle _backgroundRectangle;

		private TextRegionElement _selectedItemNameText;

		private TextRegionElement _selectedItemDescriptionText;

		private TextRegionElement _selectedItemIngredientsText;

		private Rectangle[] _inventoryItemLocations = new Rectangle[48];

		private InventoryItem _holdingItem;

		private Point _selectedLocation = new Point(0, 0);

		private bool _hitTestTrue;

		private Point _mousePointerLocation = default(Point);

		private OneShotTimer _popUpTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer _popUpFadeInTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer _popUpFadeOutTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private Point _popUpLocation;

		private InventoryItem _popUpItem;

		private InventoryItem _nextPopUpItem;

		private string CRAFTING = Strings.Crafting.ToUpper();

		private string INVENTORY = Strings.Inventory.ToUpper();

		private Rectangle _prevScreenRect;

		private StringBuilder sbuilder = new StringBuilder();
	}
}
