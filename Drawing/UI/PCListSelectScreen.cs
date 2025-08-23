using System;
using System.Collections.Generic;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class PCListSelectScreen : PCDialogScreen
	{
		public string DefaultText
		{
			set
			{
				this._defaultText = value;
			}
		}

		public string ErrorMessage
		{
			get
			{
				return this._errorMessage;
			}
			set
			{
				this._errorMessage = value;
			}
		}

		public PCListSelectScreen(DNAGame game, string title, string description1, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			base.UseDefaultValues();
			this._game = game;
		}

		public PCListSelectScreen(DNAGame game, string title, string description1, string description2, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			base.UseDefaultValues();
			this._description2 = description2;
			this._game = game;
		}

		public PCListSelectScreen(DNAGame game, string title, string description1, string description2, string description3, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			base.UseDefaultValues();
			this._description2 = description2;
			this._description3 = description3;
			this._game = game;
		}

		public void Init(int sourceIndex, List<string> names)
		{
			this._customNamesList = names;
			this._sourceIndex = sourceIndex;
		}

		private int _hitTest(Point p)
		{
			for (int i = 0; i < this._itemLocations.Length; i++)
			{
				if (this._itemLocations[i].Contains(p))
				{
					this._lastHitTestResult = i;
					return i;
				}
			}
			this._lastHitTestResult = -1;
			return -1;
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			base.Draw(device, spriteBatch, gameTime);
			Rectangle titleSafe = device.Viewport.TitleSafeArea;
			this._drawCursorTimer.Update(gameTime.ElapsedGameTime);
			if (this._drawCursorTimer.Expired)
			{
				this._drawCursorTimer.Reset();
				this._drawCursor = !this._drawCursor;
			}
			spriteBatch.Begin();
			Vector2 location = new Vector2((float)(titleSafe.Center.X - this._bgImage.Width / 2) + this.DescriptionPadding.X, this._endOfDescriptionLoc);
			if (this._errorMessage != null)
			{
				location.Y += 35f;
				spriteBatch.DrawOutlinedText(this._font, this._errorMessage, location, Color.Red, Color.Black, 1);
			}
			if (this._itemLocations.Length != this._customNamesList.Count)
			{
				this._itemLocations = new Rectangle[this._customNamesList.Count];
			}
			Rectangle safeArea = new Rectangle((int)location.X + 10, (int)location.Y, this._bgImage.Width, this._bgImage.Height);
			SpriteFont font = this._font;
			int count = this._customNamesList.Count;
			int maxGamers = this._game.CurrentNetworkSession.MaxGamers;
			this._builder.Length = 0;
			Vector2 size = font.MeasureString(this._builder);
			spriteBatch.DrawOutlinedText(font, this._builder, new Vector2((float)safeArea.Right - size.X, (float)safeArea.Bottom - size.Y), Color.White, Color.Black, 2);
			float[] columnWidths = new float[1];
			float totalRowWidth = 0f;
			totalRowWidth += (columnWidths[0] = font.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float offset = ((float)Screen.Adjuster.ScreenRect.Width - totalRowWidth) / 2f + 2f;
			float yloc = (float)safeArea.Top;
			for (int i = 0; i < this._customNamesList.Count; i++)
			{
				string selectionName = this._customNamesList[i];
				if (this._lastHitTestResult == i)
				{
					Color color = Color.Gray;
					spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle((int)offset - 2, (int)yloc, (int)totalRowWidth + 2, font.LineSpacing + 4), color);
				}
				font.MeasureString(selectionName);
				this._itemLocations[i] = new Rectangle((int)offset - 2, (int)yloc, (int)totalRowWidth + 2, font.LineSpacing + 4);
				spriteBatch.DrawOutlinedText(font, selectionName, new Vector2(offset, yloc + 2f), (this._sourceIndex == i) ? Color.Red : Color.White, Color.Black, 2);
				yloc += (float)(font.LineSpacing + 4);
				if (i == 15)
				{
					yloc = (float)(font.LineSpacing + 10 + safeArea.Top);
					offset += totalRowWidth + 10f;
				}
			}
			spriteBatch.End();
		}

		public override void OnPoped()
		{
			this._defaultText = "";
			base.OnPoped();
		}

		public override void OnPushed()
		{
			base.OnPushed();
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			int hoverItem = this._hitTest(input.Mouse.Position);
			if (hoverItem >= 0 && this._selectedIndex != hoverItem)
			{
				SoundManager.Instance.PlayInstance("Click");
				this._selectedIndex = hoverItem;
				this._optionSelected = this._selectedIndex;
			}
			if (controller.PressedButtons.A || (input.Mouse.LeftButtonPressed && hoverItem >= 0))
			{
				if (this._sourceIndex == hoverItem)
				{
					SoundManager.Instance.PlayInstance("Error");
				}
				else
				{
					base.PopMe();
					if (this.Callback != null)
					{
						this.Callback();
					}
				}
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		private bool SelectDown()
		{
			this._selectedIndex++;
			if (this._selectedIndex >= this._customNamesList.Count)
			{
				this._selectedIndex = this._customNamesList.Count - 1;
				return false;
			}
			return true;
		}

		private bool SelectUp()
		{
			this._selectedIndex--;
			if (this._selectedIndex <= 0)
			{
				this._selectedIndex = 0;
				return false;
			}
			return true;
		}

		private DNAGame _game;

		private string _defaultText = "";

		private OneShotTimer _drawCursorTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private bool _drawCursor = true;

		private int _sourceIndex;

		private List<string> _customNamesList = new List<string>();

		private StringBuilder _builder = new StringBuilder();

		private string _description2;

		private string _description3;

		protected int _cursorLine = 1;

		private string _errorMessage;

		private Rectangle[] _itemLocations = new Rectangle[0];

		private int _selectedIndex;

		private int _lastHitTestResult = -1;
	}
}
