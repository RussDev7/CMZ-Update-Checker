using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class GraphicsTab : TabControl.TabPage
	{
		static GraphicsTab()
		{
			foreach (DisplayMode displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
			{
				GraphicsTab._validResolutions[new Size(displayMode.Width, displayMode.Height)] = true;
			}
		}

		public GraphicsTab(bool inGame, ScreenGroup uiGroup)
			: base(Strings.Settings)
		{
			this._inGame = inGame;
			this._uiGroup = uiGroup;
			this._game = CastleMinerZGame.Instance;
			this._controlsFont = this._game._medFont;
			new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			this._autoClimb.Font = this._controlsFont;
			this._autoClimb.Text = Strings.Auto_Climb;
			this._autoClimb.CheckedImage = this._game._uiSprites["Checked"];
			this._autoClimb.UncheckedImage = this._game._uiSprites["Unchecked"];
			this._autoClimb.TextColor = Color.White;
			base.Children.Add(this._autoClimb);
			this._fadeInactiveTray.Font = this._controlsFont;
			this._fadeInactiveTray.Text = Strings.Fade_Inactive_Tray;
			this._fadeInactiveTray.CheckedImage = this._game._uiSprites["Checked"];
			this._fadeInactiveTray.UncheckedImage = this._game._uiSprites["Unchecked"];
			this._fadeInactiveTray.TextColor = Color.White;
			base.Children.Add(this._fadeInactiveTray);
			this._musicMute.Font = this._controlsFont;
			this._musicMute.Text = Strings.Mute + ":";
			this._musicMute.CheckedImage = this._game._uiSprites["Checked"];
			this._musicMute.UncheckedImage = this._game._uiSprites["Unchecked"];
			this._musicMute.TextColor = Color.White;
			this._musicMute.TextOnRight = false;
			base.Children.Add(this._musicMute);
			this._musicVolumeLabel = new TextControl(Strings.Music_Volume, this._controlsFont);
			this._musicVolumeTrack.MinValue = 0;
			this._musicVolumeTrack.MaxValue = 100;
			this._musicVolumeTrack.FillColor = CMZColors.MenuGreen;
			base.Children.Add(this._musicVolumeLabel);
			base.Children.Add(this._musicVolumeTrack);
			this._brightnessBar.MinValue = 0;
			this._brightnessBar.MaxValue = 100;
			this._brightnessBar.FillColor = CMZColors.MenuGreen;
			base.Children.Add(this._brightnessBar);
			this._brightnessLabel = new TextControl(Strings.Brightness, this._controlsFont);
			base.Children.Add(this._brightnessLabel);
			this._viewDistanceDropList.Items.Add(Strings.Lowest);
			this._viewDistanceDropList.Items.Add(Strings.Low);
			this._viewDistanceDropList.Items.Add(Strings.Medium);
			this._viewDistanceDropList.Items.Add(Strings.High);
			this._viewDistanceDropList.Items.Add(Strings.Ultra);
			this._viewDistanceDropList.SelectedIndexChanged += this._viewDistanceDropList_SelectedIndexChanged;
			this._viewDistanceDropList.Frame = this._game.ButtonFrame;
			this._viewDistanceDropList.DropArrow = this._game._uiSprites["DropArrow"];
			this._viewDistanceDropList.Font = this._controlsFont;
			this._viewDistanceLabel = new TextControl(Strings.View_Distance, this._controlsFont);
			base.Children.Add(this._viewDistanceLabel);
			this._textureQualityDropList.Items.Add(Strings.High);
			this._textureQualityDropList.Items.Add(Strings.Medium);
			this._textureQualityDropList.Items.Add(Strings.Low);
			this._textureQualityDropList.SelectedIndexChanged += this._textureQualityDropList_SelectedIndexChanged;
			this._textureQualityDropList.Frame = this._game.ButtonFrame;
			this._textureQualityDropList.DropArrow = this._game._uiSprites["DropArrow"];
			this._textureQualityDropList.Font = this._controlsFont;
			this._textureQualityLabel = new TextControl(Strings.Texture_Quality, this._controlsFont);
			base.Children.Add(this._textureQualityLabel);
			for (int i = 0; i < GraphicsTab._screenSizes.Length; i++)
			{
				if (GraphicsTab._validResolutions.ContainsKey(GraphicsTab._screenSizes[i]))
				{
					this._resolutionDropList.Items.Add(new GraphicsTab.Resolution(GraphicsTab._screenSizes[i]));
				}
			}
			this._resolutionDropList.SelectedIndexChanged += this._resolutionDropList_SelectedIndexChanged;
			this._resolutionDropList.Frame = this._game.ButtonFrame;
			this._resolutionDropList.DropArrow = this._game._uiSprites["DropArrow"];
			this._resolutionDropList.Font = this._controlsFont;
			this._resolutionLabel = new TextControl(Strings.Resolution, this._controlsFont);
			base.Children.Add(this._resolutionLabel);
			this._fullScreen = new CheckBoxControl(this._game._uiSprites["Unchecked"], this._game._uiSprites["Checked"]);
			this._fullScreen.Text = Strings.Full_Screen + ":";
			this._fullScreen.TextColor = Color.White;
			this._fullScreen.Font = this._controlsFont;
			base.Children.Add(this._fullScreen);
			base.Children.Add(this._resolutionDropList);
			base.Children.Add(this._textureQualityDropList);
			base.Children.Add(this._viewDistanceDropList);
			this._restartDialog = new PCDialogScreen(Strings.Texture_Quality, Strings.You_must_restart_the_game_to_apply_these_changes_, null, false, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._restartDialog.UseDefaultValues();
		}

		private void _resolutionDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Screen.Adjuster.ScreenSize != this._resolutionDropList.SelectedItem.ScreenSize)
			{
				CastleMinerZGame.GlobalSettings.ScreenSize = this._resolutionDropList.SelectedItem.ScreenSize;
				this._game.ChangeScreenSize(this._resolutionDropList.SelectedItem.ScreenSize);
			}
		}

		private void _textureQualityDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!this._ignoreTextureQualityChange)
			{
				CastleMinerZGame.GlobalSettings.TextureQualityLevel = this._textureQualityDropList.SelectedIndex + 1;
				this._uiGroup.ShowPCDialogScreen(this._restartDialog, null);
			}
			this._ignoreTextureQualityChange = false;
		}

		private void _viewDistanceDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._game.PlayerStats.DrawDistance = this._viewDistanceDropList.SelectedIndex;
		}

		public override void OnLostFocus()
		{
			try
			{
				this._game.SavePlayerStats(this._game.PlayerStats);
				CastleMinerZGame.GlobalSettings.Save();
			}
			catch
			{
			}
			base.OnLostFocus();
		}

		public override void OnSelected()
		{
			this._musicVolumeTrack.Value = (int)(this._game.PlayerStats.musicVolume * 100f);
			this._musicMute.Checked = this._game.PlayerStats.musicMute;
			this._autoClimb.Checked = this._game.PlayerStats.AutoClimb;
			this._fadeInactiveTray.Checked = this._game.PlayerStats.SecondTrayFaded;
			for (int i = 0; i < this._resolutionDropList.Items.Count; i++)
			{
				if (this._resolutionDropList.Items[i].ScreenSize == Screen.Adjuster.ScreenSize)
				{
					this._resolutionDropList.SelectedIndex = i;
					break;
				}
			}
			this._viewDistanceDropList.SelectedIndex = this._game.PlayerStats.DrawDistance;
			this._brightnessBar.Value = (int)(this._game.PlayerStats.brightness * 2f * 100f);
			this._fullScreen.Checked = this._game.IsFullScreen;
			if (this._textureQualityDropList.SelectedIndex != CastleMinerZGame.GlobalSettings.TextureQualityLevel - 1)
			{
				this._ignoreTextureQualityChange = true;
				this._textureQualityDropList.SelectedIndex = CastleMinerZGame.GlobalSettings.TextureQualityLevel - 1;
			}
			base.OnSelected();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (base.SelectedTab)
			{
				this._game.PlayerStats.brightness = (float)this._brightnessBar.Value / 100f / 2f;
				this._game.Brightness = this._game.PlayerStats.brightness;
				this._game.IsFullScreen = this._fullScreen.Checked;
				CastleMinerZGame.GlobalSettings.FullScreen = this._fullScreen.Checked;
				this._game.PlayerStats.musicMute = this._musicMute.Checked;
				this._game.PlayerStats.musicVolume = (float)this._musicVolumeTrack.Value / 100f;
				if (this._musicMute.Checked)
				{
					this._game.MusicSounds.SetVolume(0f);
				}
				else
				{
					this._game.MusicSounds.SetVolume((float)this._musicVolumeTrack.Value / 100f);
				}
				this._game.PlayerStats.AutoClimb = this._autoClimb.Checked;
				this._game.PlayerStats.SecondTrayFaded = this._fadeInactiveTray.Checked;
			}
			if (this.prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				this.prevScreenSize = Screen.Adjuster.ScreenRect;
				this._fullScreen.Scale = (this._resolutionLabel.Scale = (this._resolutionDropList.Scale = (this._textureQualityLabel.Scale = (this._textureQualityDropList.Scale = (this._viewDistanceLabel.Scale = (this._viewDistanceDropList.Scale = (this._brightnessLabel.Scale = Screen.Adjuster.ScaleFactor.Y)))))));
				this._musicVolumeLabel.Scale = (this._musicMute.Scale = (this._autoClimb.Scale = (this._fadeInactiveTray.Scale = Screen.Adjuster.ScaleFactor.Y)));
				int num = (int)(50f * Screen.Adjuster.ScaleFactor.Y);
				Point point = new Point(0, (int)(75f * Screen.Adjuster.ScaleFactor.Y));
				int num2 = (int)(215f * Screen.Adjuster.ScaleFactor.Y);
				this._musicVolumeLabel.LocalPosition = point;
				this._musicVolumeTrack.LocalPosition = new Point(point.X + num2, point.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				this._musicVolumeTrack.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				this._musicMute.LocalPosition = new Point(point.X + num2 * 2, point.Y + (int)(5f * Screen.Adjuster.ScaleFactor.Y));
				point.Y += num;
				this._autoClimb.LocalPosition = point;
				point.X += num2;
				this._fadeInactiveTray.LocalPosition = point;
				point.X -= num2;
				point.Y += num;
				this._brightnessBar.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				this._brightnessLabel.LocalPosition = point;
				this._brightnessBar.LocalPosition = new Point(point.X + num2, point.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				this._brightnessBar.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				point.Y += num;
				this._viewDistanceLabel.LocalPosition = point;
				this._viewDistanceDropList.LocalPosition = new Point(point.X + num2, point.Y);
				point.Y += num;
				this._textureQualityLabel.LocalPosition = point;
				this._textureQualityDropList.LocalPosition = new Point(point.X + num2, point.Y);
				point.Y += num;
				this._resolutionLabel.LocalPosition = point;
				this._resolutionDropList.LocalPosition = new Point(point.X + num2, point.Y);
				this._fullScreen.LocalPosition = new Point(point.X + num2 * 2, point.Y);
			}
			base.OnUpdate(game, gameTime);
		}

		private static Dictionary<Size, bool> _validResolutions = new Dictionary<Size, bool>();

		private TrackBarControl _brightnessBar = new TrackBarControl();

		private TextControl _brightnessLabel;

		private DropListControl<string> _viewDistanceDropList = new DropListControl<string>();

		private TextControl _viewDistanceLabel;

		private DropListControl<GraphicsTab.Resolution> _resolutionDropList = new DropListControl<GraphicsTab.Resolution>();

		private TextControl _resolutionLabel;

		private CheckBoxControl _fullScreen;

		private DropListControl<string> _textureQualityDropList = new DropListControl<string>();

		private TextControl _textureQualityLabel;

		private TrackBarControl _musicVolumeTrack = new TrackBarControl();

		private TextControl _musicVolumeLabel;

		private CheckBoxControl _musicMute = new CheckBoxControl();

		private CheckBoxControl _autoClimb = new CheckBoxControl();

		private CheckBoxControl _fadeInactiveTray = new CheckBoxControl();

		private bool _inGame;

		private ScreenGroup _uiGroup;

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private PCDialogScreen _restartDialog;

		private static Size[] _screenSizes = new Size[]
		{
			new Size(1920, 1200),
			new Size(1920, 1080),
			new Size(1680, 1050),
			new Size(1600, 900),
			new Size(1440, 900),
			new Size(1366, 768),
			new Size(1280, 800),
			new Size(1280, 720),
			new Size(1024, 768),
			new Size(800, 600)
		};

		private bool _ignoreTextureQualityChange;

		private Rectangle prevScreenSize;

		private struct Resolution
		{
			public override string ToString()
			{
				return this.ScreenSize.Width.ToString() + "x" + this.ScreenSize.Height.ToString();
			}

			public Resolution(Size size)
			{
				this.ScreenSize = size;
			}

			public Size ScreenSize;
		}
	}
}
