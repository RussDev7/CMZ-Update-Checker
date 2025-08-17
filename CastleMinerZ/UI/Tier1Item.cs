using System;
using System.Collections.Generic;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class Tier1Item : IEquatable<Tier1Item>
	{
		private string Title
		{
			get
			{
				return this._title;
			}
		}

		public Tier1Item(string title, Vector2 location, Sprite icon, CraftingScreen craftingScreen)
		{
			this._title = title;
			this._location = location;
			this._icon = icon;
			this._craftingScreen = craftingScreen;
			this._font = CastleMinerZGame.Instance._medFont;
		}

		public void SetItems(List<Tier2Item> items)
		{
			this._items = items;
			Vector2 vector = new Vector2(263f, 7f);
			for (int i = 0; i < this._items.Count; i++)
			{
				this._items[i].Location = this._location + vector;
				vector.Y += 31f;
			}
			this.SelectedTier2Item = this._items[0];
		}

		public bool Equals(Tier1Item other)
		{
			return this.Title == other.Title;
		}

		public void UpdateScaledLocation(Rectangle backgroundRectangle)
		{
			this._scaledLocation = new Rectangle((int)((float)backgroundRectangle.X + this._location.X * Screen.Adjuster.ScaleFactor.Y), (int)((float)backgroundRectangle.Y + this._location.Y * Screen.Adjuster.ScaleFactor.Y), (int)(243f * Screen.Adjuster.ScaleFactor.Y), (int)(36f * Screen.Adjuster.ScaleFactor.Y));
			for (int i = 0; i < this._items.Count; i++)
			{
				this._items[i].UpdateScaledLocation(backgroundRectangle);
			}
		}

		public bool CheckInput(InputManager inputManager)
		{
			if (this._craftingScreen.SelectedTier1Item == this)
			{
				for (int i = 0; i < this._items.Count; i++)
				{
					if (this._items[i].CheckInput(inputManager))
					{
						this.SelectedTier2Item = this._items[i];
					}
				}
			}
			return this._scaledLocation.Contains(inputManager.Mouse.Position);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			bool flag = this._craftingScreen.SelectedTier1Item == this;
			Color color = CMZColors.MenuAqua;
			if (flag)
			{
				color = Color.White;
			}
			spriteBatch.DrawString(this._font, this._title, new Vector2((float)this._scaledLocation.Right - (50f + this._font.MeasureString(this._title).X) * Screen.Adjuster.ScaleFactor.Y, (float)this._scaledLocation.Top), color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.Y, SpriteEffects.None, 0f);
			int num = (int)(35f * Screen.Adjuster.ScaleFactor.Y);
			spriteBatch.Draw(this._icon, new Rectangle(this._scaledLocation.Right - num, this._scaledLocation.Top, num, num), color);
			if (flag)
			{
				spriteBatch.Draw(this._craftingScreen._craftSelector, this._scaledLocation, color);
			}
			if (flag)
			{
				for (int i = 0; i < this._items.Count; i++)
				{
					this._items[i].Draw(spriteBatch);
				}
			}
		}

		private string _title;

		private Vector2 _location;

		private Rectangle _scaledLocation;

		private Sprite _icon;

		private List<Tier2Item> _items;

		public Tier2Item SelectedTier2Item;

		private CraftingScreen _craftingScreen;

		private SpriteFont _font;
	}
}
