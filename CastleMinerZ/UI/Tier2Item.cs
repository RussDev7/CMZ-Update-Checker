using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class Tier2Item : IEquatable<Tier2Item>
	{
		private string Title
		{
			get
			{
				return this._title;
			}
		}

		public InventoryItem SelectedItem
		{
			get
			{
				return this._items[this._selectedIndex].Result;
			}
		}

		public List<InventoryItem> SelectedItemIngredients
		{
			get
			{
				return this._items[this._selectedIndex].Ingredients;
			}
		}

		private PlayerInventory Inventory
		{
			get
			{
				return CastleMinerZGame.Instance.GameScreen.HUD.PlayerInventory;
			}
		}

		public Tier2Item(string title, Recipe.RecipeTypes recipeType, Tier1Item tier1Item, CraftingScreen craftingScreen)
		{
			this._title = title;
			this._items = Recipe.GetRecipes(recipeType);
			this._craftingScreen = craftingScreen;
			this._font = CastleMinerZGame.Instance._smallFont;
			this._tier1Item = tier1Item;
			this._itemLocations = new Point[this._items.Count];
			this._scaledItemLocations = new Rectangle[this._items.Count];
			Point point = new Point(393, 40);
			for (int i = 0; i < this._itemLocations.Length; i++)
			{
				this._itemLocations[i] = new Point(point.X, point.Y);
				if (i > 0)
				{
					for (int j = i - 1; j >= 0; j--)
					{
						if (this._items[i].Result.ItemClass.ID == this._items[j].Result.ItemClass.ID)
						{
							this._itemLocations[i] = new Point(this._itemLocations[j].X + 65, this._itemLocations[j].Y);
							point.Y -= 65;
							break;
						}
					}
				}
				point.Y += 65;
			}
		}

		public bool Equals(Tier2Item other)
		{
			return this.Title == other.Title;
		}

		public void UpdateScaledLocation(Rectangle backgroundRectangle)
		{
			this._scaledLocation = new Rectangle((int)((float)backgroundRectangle.X + this.Location.X * Screen.Adjuster.ScaleFactor.Y), (int)((float)backgroundRectangle.Y + this.Location.Y * Screen.Adjuster.ScaleFactor.Y), (int)(104f * Screen.Adjuster.ScaleFactor.Y), (int)(30f * Screen.Adjuster.ScaleFactor.Y));
			this._backgroundRectangle = backgroundRectangle;
			for (int i = 0; i < this._itemLocations.Length; i++)
			{
				this._scaledItemLocations[i] = new Rectangle((int)((float)backgroundRectangle.X + (float)this._itemLocations[i].X * Screen.Adjuster.ScaleFactor.Y), (int)((float)backgroundRectangle.Y + (float)this._itemLocations[i].Y * Screen.Adjuster.ScaleFactor.Y), (int)(64f * Screen.Adjuster.ScaleFactor.Y), (int)(64f * Screen.Adjuster.ScaleFactor.Y));
			}
		}

		private int _hitTest(MouseInput mouse)
		{
			for (int i = 0; i < this._scaledItemLocations.Length; i++)
			{
				if (this._scaledItemLocations[i].Contains(mouse.Position))
				{
					return i;
				}
			}
			return -1;
		}

		public bool CheckInput(InputManager inputManager)
		{
			if (this._tier1Item.SelectedTier2Item == this)
			{
				int num = this._hitTest(inputManager.Mouse);
				if (num >= 0)
				{
					this._selectedIndex = num;
					if (inputManager.Mouse.LeftButtonPressed)
					{
						if (this.Inventory.CanCraft(this._items[this._selectedIndex]))
						{
							SoundManager.Instance.PlayInstance("craft");
							CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(this._items[this._selectedIndex].Result.ItemClass.ID);
							if (CastleMinerZGame.Instance.GameMode != GameModeTypes.Creative)
							{
								if (!inputManager.Keyboard.IsKeyDown(Keys.LeftShift))
								{
									if (!inputManager.Keyboard.IsKeyDown(Keys.RightShift))
									{
										goto IL_0140;
									}
								}
								while (this.Inventory.CanCraft(this._items[this._selectedIndex]))
								{
									this.Inventory.Craft(this._items[this._selectedIndex]);
									itemStats.Crafted++;
									if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
									{
										CastleMinerZGame.Instance.PlayerStats.TotalItemsCrafted++;
									}
								}
								goto IL_019F;
							}
							IL_0140:
							this.Inventory.Craft(this._items[this._selectedIndex]);
							itemStats.Crafted++;
							if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
							{
								CastleMinerZGame.Instance.PlayerStats.TotalItemsCrafted++;
							}
						}
						else
						{
							SoundManager.Instance.PlayInstance("Error");
						}
					}
				}
			}
			IL_019F:
			return this._scaledLocation.Contains(inputManager.Mouse.Position);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			Color color = CMZColors.MenuAqua;
			bool flag = this._tier1Item.SelectedTier2Item == this;
			spriteBatch.Draw(this._craftingScreen._tier2Back, this._scaledLocation, Color.White);
			if (flag)
			{
				color = Color.White;
			}
			spriteBatch.DrawString(this._font, this._title, new Vector2((float)this._scaledLocation.Left + 10f * Screen.Adjuster.ScaleFactor.Y, (float)this._scaledLocation.Top), color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			if (flag)
			{
				for (int i = 0; i < this._scaledItemLocations.Length; i++)
				{
					spriteBatch.Draw(this._craftingScreen._gridSquare, this._scaledItemLocations[i], this.Inventory.CanCraft(this._items[i]) ? Color.White : new Color(0.25f, 0.25f, 0.25f, 0.5f));
					this._items[i].Result.Draw2D(spriteBatch, new Rectangle(this._scaledItemLocations[i].X, this._scaledItemLocations[i].Y, (int)((float)this._scaledItemLocations[i].Width - 5f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._scaledItemLocations[i].Height - 5f * Screen.Adjuster.ScaleFactor.Y)), this.Inventory.CanCraft(this._items[i]) ? Color.White : new Color(0.25f, 0.25f, 0.25f, 0.5f), true);
					if (this._selectedIndex == i)
					{
						spriteBatch.Draw(this._craftingScreen._gridSelector, this._scaledItemLocations[i], Color.White);
						this._items[i].Result.Draw2D(spriteBatch, new Rectangle(this._backgroundRectangle.X + (int)(32f * Screen.Adjuster.ScaleFactor.Y), this._backgroundRectangle.Y + (int)(235f * Screen.Adjuster.ScaleFactor.Y), (int)(130f * Screen.Adjuster.ScaleFactor.Y), (int)(130f * Screen.Adjuster.ScaleFactor.Y)), Color.White, false);
					}
				}
				spriteBatch.Draw(this._craftingScreen._craftSelector, new Rectangle(this._scaledLocation.X, this._scaledLocation.Y, (int)((float)this._scaledLocation.Width - 8f * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._scaledLocation.Height - 5f * Screen.Adjuster.ScaleFactor.Y)), color);
				Point point = new Point(39, 521);
				for (int j = 0; j < this._items[this._selectedIndex].Ingredients.Count; j++)
				{
					int num = (int)(64f * Screen.Adjuster.ScaleFactor.Y);
					Rectangle rectangle = new Rectangle((int)((float)this._backgroundRectangle.X + (float)point.X * Screen.Adjuster.ScaleFactor.Y), (int)((float)this._backgroundRectangle.Y + (float)point.Y * Screen.Adjuster.ScaleFactor.Y), num, num);
					spriteBatch.Draw(this._craftingScreen._gridSquare, rectangle, Color.White);
					rectangle.Width = (int)((float)rectangle.Width - 5f * Screen.Adjuster.ScaleFactor.Y);
					rectangle.Height = (int)((float)rectangle.Height - 5f * Screen.Adjuster.ScaleFactor.Y);
					this._items[this._selectedIndex].Ingredients[j].Draw2D(spriteBatch, rectangle, this.Inventory.CanConsume(this._items[this._selectedIndex].Ingredients[j].ItemClass, this._items[this._selectedIndex].Ingredients[j].StackCount) ? Color.White : new Color(0.25f, 0.25f, 0.25f, 0.5f), true);
					point.X += 65;
				}
			}
		}

		private string _title;

		public Vector2 Location;

		private Rectangle _scaledLocation;

		private List<Recipe> _items;

		private Point[] _itemLocations;

		private Rectangle[] _scaledItemLocations;

		private int _selectedIndex;

		private CraftingScreen _craftingScreen;

		private SpriteFont _font;

		private Tier1Item _tier1Item;

		private Rectangle _backgroundRectangle;
	}
}
