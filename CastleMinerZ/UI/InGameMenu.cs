using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class InGameMenu : MenuScreen
	{
		public InGameMenu(CastleMinerZGame game)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, true)
		{
			SpriteFont largeFont = game._largeFont;
			this._game = game;
			this.SelectSound = "Click";
			this.ClickSound = "Click";
			base.AddMenuItem(Strings.Return_To_Game, InGameMenuItems.Return);
			base.AddMenuItem(Strings.Inventory, InGameMenuItems.MyBlocks);
			if (this._game.GameMode != GameModeTypes.Endurance && this._game.Difficulty != GameDifficultyTypes.HARDCORE)
			{
				this.teleport = base.AddMenuItem(Strings.Teleport, InGameMenuItems.Teleport);
			}
			this.inviteControl = base.AddMenuItem(Strings.Invite_Friends, InGameMenuItems.Invite);
			base.AddMenuItem(Strings.Options, InGameMenuItems.Options);
			base.AddMenuItem(Strings.Main_Menu, InGameMenuItems.Quit);
			this.inviteControl.Visible = false;
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			spriteBatch.Begin();
			spriteBatch.Draw(this._game.DummyTexture, Screen.Adjuster.ScreenRect, new Color(0f, 0f, 0f, 0.5f));
			if (CastleMinerZGame.Instance.IsOnlineGame && CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				string text = Strings.Server_Message + ": " + CastleMinerZGame.Instance.CurrentNetworkSession.ServerMessage;
				spriteBatch.DrawOutlinedText(this._game._consoleFont, text, new Vector2((float)(screenRect.Left + 22), (float)screenRect.Top), Color.White, Color.Black, 1);
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		public override void OnPushed()
		{
			if (!CastleMinerZGame.Instance.IsOnlineGame)
			{
				CastleMinerZGame.Instance.GameScreen.mainScene.DoUpdate = false;
				CastleMinerZGame.Instance.GameScreen.DoUpdate = false;
			}
			base.OnPushed();
		}

		public override void OnPoped()
		{
			if (!CastleMinerZGame.Instance.IsOnlineGame)
			{
				CastleMinerZGame.Instance.GameScreen.mainScene.DoUpdate = true;
				CastleMinerZGame.Instance.GameScreen.DoUpdate = true;
			}
			base.OnPoped();
		}

		private CastleMinerZGame _game;

		private MenuItemElement inviteControl;

		private MenuItemElement teleport;
	}
}
