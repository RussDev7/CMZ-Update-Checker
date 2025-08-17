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
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			float loadProgress = this._game.LoadProgress;
			string text = this._text;
			float num = (float)screenRect.Width * 0.8f;
			float num2 = (float)screenRect.Left + ((float)screenRect.Width - num) / 2f;
			Sprite sprite = this._game._uiSprites["Bar"];
			Vector2 vector = this._largeFont.MeasureString(text);
			Vector2 vector2 = new Vector2(num2, (float)(screenRect.Height / 2) + vector.Y / 2f);
			float num3 = vector2.Y + (float)this._largeFont.LineSpacing + 6.6666665f;
			Rectangle rectangle = new Rectangle((int)num2, (int)num3, (int)num, this._largeFont.LineSpacing);
			int left = rectangle.Left;
			int top = rectangle.Top;
			float num4 = (float)rectangle.Width / (float)sprite.Width;
			spriteBatch.Begin();
			Rectangle rectangle2 = new Rectangle(0, 0, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height);
			spriteBatch.Draw(this._game.MenuBackdrop, rectangle2, Color.White);
			this._game.Logo.Draw(spriteBatch, new Vector2((float)(screenRect.Center.X - this._game.Logo.Width / 2), 0f), Color.White);
			spriteBatch.DrawOutlinedText(this._largeFont, text, vector2, Color.White, Color.Black, 1);
			spriteBatch.Draw(this._game.DummyTexture, new Rectangle(left - 2, top - 2, rectangle.Width + 4, rectangle.Height + 4), Color.White);
			spriteBatch.Draw(this._game.DummyTexture, new Rectangle(left, top, rectangle.Width, rectangle.Height), Color.Black);
			int num5 = (int)((float)sprite.Width * loadProgress);
			sprite.Draw(spriteBatch, new Rectangle(left, top, (int)((float)rectangle.Width * loadProgress), rectangle.Height), new Rectangle(sprite.Width - num5, 0, num5, sprite.Height), Color.White);
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
