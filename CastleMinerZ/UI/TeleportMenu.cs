using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class TeleportMenu : MenuScreen
	{
		public TeleportMenu(CastleMinerZGame game)
			: base(game._largeFont, false)
		{
			this.TextColor = CMZColors.MenuGreen;
			this.SelectedColor = Color.White;
			this._game = game;
			this.ClickSound = "Click";
			this.SelectSound = "Click";
			base.AddMenuItem(Strings.Return_To_Game, TeleportMenuItems.Quit);
			base.AddMenuItem(Strings.Teleport_To_Surface, TeleportMenuItems.Surface);
			base.AddMenuItem(Strings.Teleport_To_Start, TeleportMenuItems.Origin);
			this.toPLayer = base.AddMenuItem(Strings.Teleport_To_Player, TeleportMenuItems.Player);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			this.toPLayer.Visible = this._game.IsOnlineGame && this._game.PVPState == CastleMinerZGame.PVPEnum.Off;
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(this._game.DummyTexture, Screen.Adjuster.ScreenRect, new Color(0f, 0f, 0f, 0.5f));
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private MenuItemElement toPLayer;

		private CastleMinerZGame _game;
	}
}
