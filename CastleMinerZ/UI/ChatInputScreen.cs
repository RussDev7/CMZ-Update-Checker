using System;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class ChatInputScreen : UIControlScreen
	{
		public ChatInputScreen()
			: base(true)
		{
			base.Controls.Add(this._chatInput);
			this._chatInput.Close += this._chatInput_Close;
		}

		public override void OnPushed()
		{
			this._chatInput.OnPushed();
			base.OnPushed();
		}

		private void _chatInput_Close(object sender, EventArgs e)
		{
			base.PopMe();
		}

		protected override bool OnInput(InputManager inputManager, GameTime gameTime)
		{
			if (this._chatInput.UpperBar.DragMenu)
			{
				this._chatPosition += inputManager.Mouse.DeltaPosition;
				float num = (float)(Screen.Adjuster.ScreenRect.Width - this._chatInput.Width);
				float num2 = (float)(Screen.Adjuster.ScreenRect.Height - this._chatInput.Height);
				if (this._chatPosition.X < 0f)
				{
					this._chatPosition.X = 0f;
				}
				else if (this._chatPosition.X > num)
				{
					this._chatPosition.X = num;
				}
				if (this._chatPosition.Y < 0f)
				{
					this._chatPosition.Y = 0f;
				}
				else if (this._chatPosition.Y > num2)
				{
					this._chatPosition.Y = num2;
				}
			}
			return base.OnInput(inputManager, gameTime);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			this._chatInput.LocalPosition = new Point((int)this._chatPosition.X, (int)this._chatPosition.Y);
			this._chatInput._textEditControl.HasFocus = true;
			base.Update(game, gameTime);
		}

		private ChatInputScreen.ChatInputGroup _chatInput = new ChatInputScreen.ChatInputGroup();

		private Vector2 _chatPosition = new Vector2(0f, 0f);

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private class ChatInputGroup : UIControlGroup
		{
			public event EventHandler Close;

			public ChatInputGroup()
			{
				this.UpperBar.LocalPosition = Point.Zero;
				this.UpperBar.Size = new Size(this.Width, 20);
				this.UpperBar.Frame = this._game.ButtonFrame;
				this.UpperBar.Text = Strings.Chat;
				this.UpperBar.Font = this._game._myriadSmall;
				this.UpperBar.ButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
				base.Children.Add(this.UpperBar);
				this._closeWindow.LocalPosition = new Point(this.Width - 19, 2);
				this._closeWindow.Size = new Size(17, 17);
				this._closeWindow.Frame = this._game.ButtonFrame;
				this._closeWindow.Text = "x";
				this._closeWindow.Font = this._game._smallFont;
				this._closeWindow.Pressed += this._closeWindow_Pressed;
				base.Children.Add(this._closeWindow);
				this._textEditControl.LocalPosition = new Point(10, 30);
				this._textEditControl.Size = new Size(this.Width - 20, 200);
				this._textEditControl.Font = this._game._myriadSmall;
				this._textEditControl.Frame = this._game.ButtonFrame;
				this._textEditControl.EnterPressed += this._textEditControl_EnterPressed;
				base.Children.Add(this._textEditControl);
			}

			private void _textEditControl_EnterPressed(object sender, EventArgs e)
			{
				if (!string.IsNullOrWhiteSpace(this._textEditControl.Text))
				{
					BroadcastTextMessage.Send(this._game.MyNetworkGamer, this._game.MyNetworkGamer.Gamertag + ": " + this._textEditControl.Text);
				}
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			private void _sendButton_Pressed(object sender, EventArgs e)
			{
				if (!string.IsNullOrWhiteSpace(this._textEditControl.Text))
				{
					BroadcastTextMessage.Send(this._game.MyNetworkGamer, this._game.MyNetworkGamer.Gamertag + ": " + this._textEditControl.Text);
				}
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			public void OnPushed()
			{
				this._textEditControl.Text = "";
				this._textEditControl.HasFocus = true;
			}

			private void _cancelButton_Pressed(object sender, EventArgs e)
			{
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			private void _closeWindow_Pressed(object sender, EventArgs e)
			{
				if (this.Close != null)
				{
					this.Close(this, new EventArgs());
				}
			}

			protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
			{
				this._game.ButtonFrame.Draw(spriteBatch, new Rectangle(base.ScreenPosition.X, base.ScreenPosition.Y + 20, this.Width, this.Height - 20), new Color(0.75f, 0.75f, 0.75f, 0.75f));
				base.OnDraw(device, spriteBatch, gameTime);
			}

			public TextEditControl _textEditControl = new TextEditControl();

			public MenuBarControl UpperBar = new MenuBarControl();

			private CastleMinerZGame _game = CastleMinerZGame.Instance;

			private FrameButtonControl _closeWindow = new FrameButtonControl();

			public int Width = 400;

			public int Height = 60;
		}
	}
}
