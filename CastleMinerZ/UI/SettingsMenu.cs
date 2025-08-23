using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.UI
{
	public class SettingsMenu : SettingScreen
	{
		public SettingsMenu(CastleMinerZGame game)
			: base(game, game._medLargeFont, Color.White, Color.Red, false)
		{
			this._game = game;
			this.ClickSound = "Click";
			this.SelectSound = "Click";
			this.BrightnessBar = new BarSettingItem(Strings.Brightness, this._game.Brightness);
			base.MenuItems.Add(this.BrightnessBar);
			this.ControllerSensitivityBar = new BarSettingItem(Strings.Controller_Sensitivity, this._game.PlayerStats.controllerSensitivity);
			base.MenuItems.Add(this.ControllerSensitivityBar);
			this.MusicVolumeBar = new BarSettingItem(Strings.Music_Volume, this._game.PlayerStats.musicVolume);
			base.MenuItems.Add(this.MusicVolumeBar);
			List<object> drawDistances = new List<object>
			{
				Strings.Lowest,
				Strings.Low,
				Strings.Medium,
				Strings.High,
				Strings.Ultra
			};
			this.DrawDistanceBar = new ListSettingItem(Strings.Graphics, drawDistances, this._game.PlayerStats.DrawDistance);
			base.MenuItems.Add(this.DrawDistanceBar);
			this.InvertYaxis = new BoolSettingItem(Strings.Invert_Y_Axis, this._game.PlayerStats.InvertYAxis, Strings.Inverted, Strings.Regular);
			base.MenuItems.Add(this.InvertYaxis);
			this.FadeInactiveTray = new BoolSettingItem(Strings.Fade_Inactive_Tray, this._game.PlayerStats.SecondTrayFaded, Strings.Faded, Strings.Regular);
			base.MenuItems.Add(this.FadeInactiveTray);
			this.AutoClimb = new BoolSettingItem(Strings.Fade_Inactive_Tray, this._game.PlayerStats.AutoClimb, Strings.On, Strings.Off);
			base.MenuItems.Add(this.AutoClimb);
			this.FullScreen = new BoolSettingItem(Strings.Full_Screen, this._game.IsFullScreen, Strings.On, Strings.Off);
			base.MenuItems.Add(this.FullScreen);
			ImageButtonControl _backButton = new ImageButtonControl();
			_backButton.Image = this._game._uiSprites["BackArrow"];
			_backButton.Font = this._game._medFont;
			_backButton.LocalPosition = new Point(15, 15);
			_backButton.Pressed += this._backButton_Pressed;
			_backButton.Text = " " + Strings.Back;
			_backButton.ImageDefaultColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			base.Controls.Add(_backButton);
			int yoffset = (int)(Screen.Adjuster.ScaleFactor.Y * 25f);
			int xoffset = (int)(Screen.Adjuster.ScaleFactor.X * 25f);
			this.DrawArea = new Rectangle?(new Rectangle(Screen.Adjuster.ScreenRect.X + xoffset, Screen.Adjuster.ScreenRect.Y + yoffset, Screen.Adjuster.ScreenRect.Width - xoffset * 2, Screen.Adjuster.ScreenRect.Height - yoffset * 2));
			this.SelectedColor = CMZColors.MenuGreen;
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			base.PopMe();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			this.DrawArea = new Rectangle?(new Rectangle(Screen.Adjuster.ScreenRect.X + 15, Screen.Adjuster.ScreenRect.Y + 55, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height - 55));
			this._game.Brightness = this.BrightnessBar.Value / 2f;
			this._game.PlayerStats.brightness = this.BrightnessBar.Value / 2f;
			this._game.PlayerStats.InvertYAxis = this.InvertYaxis.On;
			this._game.PlayerStats.SecondTrayFaded = this.FadeInactiveTray.On;
			this._game.PlayerStats.DrawDistance = this.DrawDistanceBar.Index;
			this._game.PlayerStats.musicVolume = this.MusicVolumeBar.Value;
			this._game.MusicSounds.SetVolume(this.MusicVolumeBar.Value);
			this._game.PlayerStats.AutoClimb = this.AutoClimb.On;
			CastleMinerZGame.GlobalSettings.FullScreen = this.FullScreen.On;
			this._game.IsFullScreen = this.FullScreen.On;
			if ((double)this.ControllerSensitivityBar.Value < 0.5)
			{
				this._game.PlayerStats.controllerSensitivity = this.ControllerSensitivityBar.Value + 0.5f;
			}
			else
			{
				this._game.PlayerStats.controllerSensitivity = this.ControllerSensitivityBar.Value * 2f;
			}
			base.OnUpdate(game, gameTime);
		}

		public override void OnPushed()
		{
			this.BrightnessBar.Value = this._game.Brightness * 2f;
			this.FadeInactiveTray.On = this._game.PlayerStats.SecondTrayFaded;
			this.InvertYaxis.On = this._game.PlayerStats.InvertYAxis;
			this.MusicVolumeBar.Value = this._game.PlayerStats.musicVolume;
			this.AutoClimb.On = this._game.PlayerStats.AutoClimb;
			this.FullScreen.On = this._game.IsFullScreen;
			this.DrawDistanceBar.Index = this._game.PlayerStats.DrawDistance;
			if (this._game.PlayerStats.controllerSensitivity < 1f)
			{
				this.ControllerSensitivityBar.Value = this._game.PlayerStats.controllerSensitivity - 0.5f;
			}
			else
			{
				this.ControllerSensitivityBar.Value = this._game.PlayerStats.controllerSensitivity / 2f;
			}
			base.OnPushed();
		}

		public override void OnPoped()
		{
			try
			{
				this._game.SavePlayerStats(this._game.PlayerStats);
			}
			catch
			{
			}
			base.OnPoped();
		}

		private CastleMinerZGame _game;

		private BarSettingItem BrightnessBar;

		private BarSettingItem MusicVolumeBar;

		private ListSettingItem DrawDistanceBar;

		private BoolSettingItem FadeInactiveTray;

		private BoolSettingItem InvertYaxis;

		private BarSettingItem ControllerSensitivityBar;

		private BoolSettingItem AutoClimb;

		private BoolSettingItem FullScreen;
	}
}
