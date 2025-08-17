using System;
using DNA.Drawing.UI;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class LoadScreen : Screen
	{
		public LoadScreen(Texture2D loadScreen, TimeSpan totalTime)
			: base(true, false)
		{
			this.display.MaxTime = totalTime - TimeSpan.FromSeconds(8.0);
			this._image = loadScreen;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle rectangle = Screen.Adjuster.TransformClipped(new Rectangle(0, 0, 1280, 720));
			spriteBatch.Begin();
			if (this.preBlackness.Expired)
			{
				if (this.fadeIn.Expired)
				{
					if (this.display.Expired)
					{
						if (this.fadeOut.Expired)
						{
							if (this.postBlackness.Expired)
							{
								this.Finished = true;
							}
							else
							{
								spriteBatch.Draw(this._image, rectangle, Color.Black);
							}
						}
						else
						{
							spriteBatch.Draw(this._image, rectangle, Color.Lerp(Color.White, Color.Black, this.fadeOut.PercentComplete));
						}
					}
					else
					{
						spriteBatch.Draw(this._image, rectangle, Color.White);
					}
				}
				else
				{
					spriteBatch.Draw(this._image, rectangle, Color.Lerp(Color.Black, Color.White, this.fadeIn.PercentComplete));
				}
			}
			else
			{
				spriteBatch.Draw(this._image, rectangle, Color.Black);
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (this.preBlackness.Expired)
			{
				if (this.fadeIn.Expired)
				{
					if (this.display.Expired)
					{
						if (this.fadeOut.Expired)
						{
							if (this.postBlackness.Expired)
							{
								this.Finished = true;
							}
							else
							{
								this.postBlackness.Update(gameTime.ElapsedGameTime);
							}
						}
						else
						{
							this.fadeOut.Update(gameTime.ElapsedGameTime);
						}
					}
					else
					{
						this.display.Update(gameTime.ElapsedGameTime);
					}
				}
				else
				{
					this.fadeIn.Update(gameTime.ElapsedGameTime);
				}
			}
			else
			{
				this.preBlackness.Update(gameTime.ElapsedGameTime);
			}
			base.OnUpdate(game, gameTime);
		}

		private OneShotTimer preBlackness = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer fadeIn = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer display = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer fadeOut = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private OneShotTimer postBlackness = new OneShotTimer(TimeSpan.FromSeconds(2.0));

		private Texture2D _image;

		public bool Finished;
	}
}
