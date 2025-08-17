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
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			this._drawCursorTimer.Update(gameTime.ElapsedGameTime);
			if (this._drawCursorTimer.Expired)
			{
				this._drawCursorTimer.Reset();
				this._drawCursor = !this._drawCursor;
			}
			spriteBatch.Begin();
			Vector2 vector = new Vector2((float)(titleSafeArea.Center.X - this._bgImage.Width / 2) + this.DescriptionPadding.X, this._endOfDescriptionLoc);
			if (this._errorMessage != null)
			{
				vector.Y += 35f;
				spriteBatch.DrawOutlinedText(this._font, this._errorMessage, vector, Color.Red, Color.Black, 1);
			}
			if (this._itemLocations.Length != this._customNamesList.Count)
			{
				this._itemLocations = new Rectangle[this._customNamesList.Count];
			}
			Rectangle rectangle = new Rectangle((int)vector.X + 10, (int)vector.Y, this._bgImage.Width, this._bgImage.Height);
			SpriteFont font = this._font;
			int count = this._customNamesList.Count;
			int maxGamers = this._game.CurrentNetworkSession.MaxGamers;
			this._builder.Length = 0;
			Vector2 vector2 = font.MeasureString(this._builder);
			spriteBatch.DrawOutlinedText(font, this._builder, new Vector2((float)rectangle.Right - vector2.X, (float)rectangle.Bottom - vector2.Y), Color.White, Color.Black, 2);
			float[] array = new float[1];
			float num = 0f;
			num += (array[0] = font.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float num2 = ((float)Screen.Adjuster.ScreenRect.Width - num) / 2f + 2f;
			float num3 = (float)rectangle.Top;
			for (int i = 0; i < this._customNamesList.Count; i++)
			{
				string text = this._customNamesList[i];
				if (this._lastHitTestResult == i)
				{
					Color gray = Color.Gray;
					spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle((int)num2 - 2, (int)num3, (int)num + 2, font.LineSpacing + 4), gray);
				}
				font.MeasureString(text);
				this._itemLocations[i] = new Rectangle((int)num2 - 2, (int)num3, (int)num + 2, font.LineSpacing + 4);
				spriteBatch.DrawOutlinedText(font, text, new Vector2(num2, num3 + 2f), (this._sourceIndex == i) ? Color.Red : Color.White, Color.Black, 2);
				num3 += (float)(font.LineSpacing + 4);
				if (i == 15)
				{
					num3 = (float)(font.LineSpacing + 10 + rectangle.Top);
					num2 += num + 10f;
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
			int num = this._hitTest(input.Mouse.Position);
			if (num >= 0 && this._selectedIndex != num)
			{
				SoundManager.Instance.PlayInstance("Click");
				this._selectedIndex = num;
				this._optionSelected = this._selectedIndex;
			}
			if (controller.PressedButtons.A || (input.Mouse.LeftButtonPressed && num >= 0))
			{
				if (this._sourceIndex == num)
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
