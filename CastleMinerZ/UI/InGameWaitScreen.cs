using System;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class InGameWaitScreen : Screen
	{
		public static void ShowScreen(CastleMinerZGame game, ScreenGroup group, string text, bool spawnontop, ProgressCallback callback)
		{
			if (!callback())
			{
				group.PushScreen(new InGameWaitScreen(game, text, callback, spawnontop));
				return;
			}
			game.MakeAboveGround(spawnontop);
		}

		public InGameWaitScreen(CastleMinerZGame game, string text, ProgressCallback callback, bool spawnontop)
			: base(true, false)
		{
			this._largeFont = game._largeFont;
			this._text = text;
			this._callback = callback;
			this._game = game;
			this._spawnOnTop = spawnontop;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafe = Screen.Adjuster.ScreenRect;
			float progress = this._game.LoadProgress;
			string msg = this._text;
			float barWidth = (float)titleSafe.Width * 0.8f;
			float leftStart = (float)titleSafe.Left + ((float)titleSafe.Width - barWidth) / 2f;
			Sprite bar = this._game._uiSprites["Bar"];
			Vector2 size = this._largeFont.MeasureString(msg);
			Vector2 position = new Vector2(leftStart, (float)(titleSafe.Height / 2) + size.Y / 2f);
			float ypos = position.Y + (float)this._largeFont.LineSpacing + 6.6666665f;
			Rectangle location = new Rectangle((int)leftStart, (int)ypos, (int)barWidth, this._largeFont.LineSpacing);
			int xloc = location.Left;
			int yloc = location.Top;
			float num = (float)location.Width / (float)bar.Width;
			spriteBatch.Begin();
			Rectangle Destination = new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height);
			spriteBatch.Draw(this._game.MenuBackdrop, Destination, Color.White);
			this._game.Logo.Draw(spriteBatch, new Vector2((float)(titleSafe.Center.X - this._game.Logo.Width / 2), 0f), Color.White);
			spriteBatch.DrawOutlinedText(this._largeFont, msg, position, Color.White, Color.Black, 1);
			spriteBatch.Draw(this._game.DummyTexture, new Rectangle(xloc - 2, yloc - 2, location.Width + 4, location.Height + 4), Color.White);
			spriteBatch.Draw(this._game.DummyTexture, new Rectangle(xloc, yloc, location.Width, location.Height), Color.Black);
			int sourceWidth = (int)((float)bar.Width * progress);
			bar.Draw(spriteBatch, new Rectangle(xloc, yloc, (int)((float)location.Width * progress), location.Height), new Rectangle(bar.Width - sourceWidth, 0, sourceWidth, bar.Height), Color.White);
			this.textFlashTimer.Update(gameTime.ElapsedGameTime);
			Color.Lerp(Color.Red, Color.White, this.textFlashTimer.PercentComplete);
			if (this.textFlashTimer.Expired)
			{
				this.textFlashTimer.Reset();
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (this._callback())
			{
				this._game.MakeAboveGround(this._spawnOnTop);
				base.PopMe();
			}
			base.OnUpdate(game, gameTime);
		}

		private SpriteFont _largeFont;

		private string _text;

		private ProgressCallback _callback;

		private CastleMinerZGame _game;

		private bool _spawnOnTop;

		private OneShotTimer textFlashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));
	}
}
