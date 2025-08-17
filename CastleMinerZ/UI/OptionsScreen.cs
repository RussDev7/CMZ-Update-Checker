using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class OptionsScreen : UIControlScreen
	{
		public OptionsScreen(bool inGame, ScreenGroup uiGroup)
			: base(false)
		{
			this._inGame = inGame;
			Color color = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			this.tabControl.Font = this._game._medFont;
			this.tabControl.Size = new Size(Screen.Adjuster.ScreenRect.Width, (int)(620f * Screen.Adjuster.ScaleFactor.Y));
			this.tabControl.LocalPosition = new Point(Screen.Adjuster.ScreenRect.Left, Screen.Adjuster.ScreenRect.Top + (int)(100f * Screen.Adjuster.ScaleFactor.Y));
			this.tabControl.TextColor = color;
			this.tabControl.TextSelectedColor = color;
			this.tabControl.TextHoverColor = Color.Gray;
			this.tabControl.TextPressedColor = Color.Black;
			this.tabControl.BarColor = color;
			this.tabControl.BarSelectedColor = color;
			this.tabControl.BarHoverColor = Color.Black;
			this.tabControl.BarPressedColor = Color.Black;
			this.tabControl.Size = new Size(960, 620);
			base.Controls.Add(this.tabControl);
			this._controlsTab = new ControlsTab(inGame, uiGroup);
			this._graphicsTab = new GraphicsTab(inGame, uiGroup);
			if (inGame)
			{
				this._hostOptionsTab = new HostOptionsTab(inGame);
			}
			this.tabControl.Tabs.Add(this._graphicsTab);
			this.tabControl.Tabs.Add(this._controlsTab);
			if (inGame)
			{
				this.tabControl.Tabs.Add(this._hostOptionsTab);
			}
			this._backButton.Size = new Size(135, this._game._medFont.LineSpacing);
			this._backButton.Text = Strings.Back;
			this._backButton.Font = this._game._medFont;
			this._backButton.Frame = this._game.ButtonFrame;
			this._backButton.Pressed += this._backButton_Pressed;
			this._backButton.ButtonColor = color;
			base.Controls.Add(this._backButton);
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			base.PopMe();
		}

		public override void OnPushed()
		{
			if (this._inGame && this._game.IsOnlineGame && this._game.IsGameHost)
			{
				if (!this.tabControl.Tabs.Contains(this._hostOptionsTab))
				{
					this.tabControl.Tabs.Add(this._hostOptionsTab);
				}
			}
			else if (this.tabControl.Tabs.Contains(this._hostOptionsTab))
			{
				this.tabControl.Tabs.Remove(this._hostOptionsTab);
			}
			this.tabControl.SelectedTab.OnSelected();
			base.OnPushed();
		}

		public override void OnPoped()
		{
			this.tabControl.SelectedTab.OnLostFocus();
			base.OnPoped();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (this.prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				this.prevScreenSize = Screen.Adjuster.ScreenRect;
				this.tabControl.LocalPosition = new Point(Screen.Adjuster.ScreenRect.Center.X - (int)(480f * Screen.Adjuster.ScaleFactor.Y), Screen.Adjuster.ScreenRect.Top + (int)(100f * Screen.Adjuster.ScaleFactor.Y));
				this.tabControl.Scale = Screen.Adjuster.ScaleFactor.Y;
				for (int i = 0; i < this.tabControl.Tabs.Count; i++)
				{
					this.tabControl.Tabs[i].LocalPosition = Point.Zero;
					this.tabControl.Tabs[i].Size = this.tabControl.Size;
				}
				this._backButton.Scale = Screen.Adjuster.ScaleFactor.Y;
				this._backButton.LocalPosition = new Point(this.tabControl.LocalPosition.X, Screen.Adjuster.ScreenRect.Bottom - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
			}
			base.OnUpdate(game, gameTime);
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				base.PopMe();
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		private ControlsTab _controlsTab;

		private HostOptionsTab _hostOptionsTab;

		private GraphicsTab _graphicsTab;

		private TabControl tabControl = new TabControl();

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private bool _inGame;

		private FrameButtonControl _backButton = new FrameButtonControl();

		private Rectangle prevScreenSize;
	}
}
