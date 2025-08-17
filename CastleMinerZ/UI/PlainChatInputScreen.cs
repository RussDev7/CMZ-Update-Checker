using System;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class PlainChatInputScreen : UIControlScreen
	{
		public float YLoc
		{
			set
			{
				this._yLoc = value;
			}
		}

		public PlainChatInputScreen(float yLoc)
			: base(true)
		{
			this._yLoc = yLoc;
			this._textEditControl.LocalPosition = new Point(0, (int)this._yLoc);
			this._textEditControl.Size = new Size(this.Width - 20, 200);
			this._textEditControl.Font = this._game._myriadSmall;
			this._textEditControl.Frame = this._game.ButtonFrame;
			this._textEditControl.EnterPressed += this._textEditControl_EnterPressed;
			this._textEditControl.TextColor = Color.White;
			this._textEditControl.FrameColor = new Color(0f, 0f, 0f, 0.5f);
			base.Controls.Add(this._textEditControl);
		}

		private void _textEditControl_EnterPressed(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(this._textEditControl.Text))
			{
				BroadcastTextMessage.Send(this._game.MyNetworkGamer, this._game.MyNetworkGamer.Gamertag + ": " + this._textEditControl.Text);
			}
			base.PopMe();
		}

		public override void OnPushed()
		{
			this._textEditControl.Text = "";
			this._textEditControl.HasFocus = true;
			base.OnPushed();
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				base.PopMe();
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			if (string.IsNullOrWhiteSpace(this._textEditControl.Text))
			{
				spriteBatch.DrawString(this._game._myriadSmall, Strings.Type_here, new Vector2(0f, this._yLoc), Color.White);
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		public TextEditControl _textEditControl = new TextEditControl();

		public int Width = 350;

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private float _yLoc;
	}
}
