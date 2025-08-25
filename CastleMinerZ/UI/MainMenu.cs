using System;
using System.Diagnostics;
using System.Text;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class MainMenu : MenuScreen
	{
		public MainMenu(CastleMinerZGame game)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, false)
		{
			SpriteFont largeFont = game._largeFont;
			this._game = game;
			this.ClickSound = "Click";
			this.SelectSound = "Click";
			this.HorizontalAlignment = MenuScreen.HorizontalAlignmentTypes.Right;
			this.VerticalAlignment = MenuScreen.VerticalAlignmentTypes.Top;
			this.LineSpacing = new int?(-10);
			this.hostOnlineControl = base.AddMenuItem(Strings.Host_Online, MainMenuItems.HostOnline);
			this.joinOnlineControl = base.AddMenuItem(Strings.Join_Online, MainMenuItems.JoinOnline);
			base.AddMenuItem(Strings.Play_Offline, MainMenuItems.PlayOffline);
			base.AddMenuItem(Strings.Options, MainMenuItems.Options);
			base.AddMenuItem(Strings.Exit, MainMenuItems.Quit);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			this.adRect = new Rectangle((int)((float)Screen.Adjuster.ScreenRect.Width * 0.6f), Screen.Adjuster.ScreenRect.Height / 4, (int)((float)CastleMinerZGame.Instance.CMZREAd.Width * Screen.Adjuster.ScaleFactor.Y), (int)((float)CastleMinerZGame.Instance.CMZREAd.Height * Screen.Adjuster.ScaleFactor.Y));
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			spriteBatch.Begin();
			string playerName = Screen.CurrentGamer.Gamertag;
			spriteBatch.DrawOutlinedText(this._game._myriadMed, playerName, Vector2.Zero, Color.White, Color.Black, 1);
			this._nameRect = new Rectangle(0, 0, (int)this._game._myriadMed.MeasureString(playerName).X, (int)this._game._myriadMed.MeasureString(playerName).Y);
			int width = (int)(512f * Screen.Adjuster.ScaleFactor.Y);
			int height = this._game.Logo.Height * width / this._game.Logo.Width;
			this._game.Logo.Draw(spriteBatch, new Rectangle(Screen.Adjuster.ScreenRect.Center.X - width / 2, 0, width, height), Color.White);
			this.DrawArea = new Rectangle?(new Rectangle(0, (int)((double)height * 0.75), (int)((float)(Screen.Adjuster.ScreenRect.Width / 2) - 125f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - height));
			spriteBatch.Draw(this.adSel ? CastleMinerZGame.Instance.CMZREAdSel : CastleMinerZGame.Instance.CMZREAd, this.adRect, Color.White);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			if (this.adRect.Contains(input.Mouse.Position))
			{
				if (input.Mouse.LeftButtonPressed)
				{
					this.adClicked = true;
				}
				this.adSel = true;
				if (input.Mouse.LeftButtonReleased && this.adSel && this.adClicked)
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "https://store.steampowered.com/app/3631230?utm_source=CMZ_INGAMEAD",
						UseShellExecute = true
					});
				}
			}
			else if (!input.Mouse.LeftButtonDown)
			{
				this.adSel = false;
			}
			if (!input.Mouse.LeftButtonDown)
			{
				this.adClicked = false;
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				this._game.FrontEnd.ConfirmExit();
				return false;
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			this.hostOnlineControl.TextColor = new Color?(CMZColors.MenuGreen);
			this.joinOnlineControl.TextColor = new Color?(CMZColors.MenuGreen);
			base.OnUpdate(game, gameTime);
		}

		private CastleMinerZGame _game;

		private MenuItemElement hostOnlineControl;

		private MenuItemElement joinOnlineControl;

		private Rectangle adRect;

		private bool adSel;

		private StringBuilder builder = new StringBuilder();

		private Rectangle _nameRect = Rectangle.Empty;

		private bool adClicked;
	}
}
