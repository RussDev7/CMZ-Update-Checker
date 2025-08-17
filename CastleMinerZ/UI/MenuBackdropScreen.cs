using System;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class MenuBackdropScreen : Screen
	{
		public MenuBackdropScreen(CastleMinerZGame game)
			: base(false, false)
		{
			this._game = game;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			try
			{
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
				spriteBatch.Draw(this._game.MenuBackdrop, screenRect, Color.White);
				spriteBatch.End();
			}
			catch
			{
			}
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private CastleMinerZGame _game;
	}
}
