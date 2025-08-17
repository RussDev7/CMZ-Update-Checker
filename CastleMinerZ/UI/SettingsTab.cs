using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class SettingsTab : TabControl.TabPage
	{
		public SettingsTab(bool inGame, ScreenGroup uiGroup)
			: base(Strings.Controls)
		{
			this._game = CastleMinerZGame.Instance;
			this._controlsFont = this._game._medFont;
			this._inGame = inGame;
			this._uiGroup = uiGroup;
			Color color = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			this._deleteStorageDialog = new PCDialogScreen(Strings.Erase_Storage, Strings.Are_you_sure_you_want_to_delete_everything_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._deleteStorageDialog.UseDefaultValues();
			if (!this._inGame)
			{
				this._eraseSaves.Size = new Size(225, this._controlsFont.LineSpacing);
				this._eraseSaves.Text = Strings.Erase_Storage;
				this._eraseSaves.Font = this._controlsFont;
				this._eraseSaves.Frame = this._game.ButtonFrame;
				this._eraseSaves.Pressed += this._eraseSaves_Pressed;
				this._eraseSaves.ButtonColor = color;
				base.Children.Add(this._eraseSaves);
			}
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
		}

		private void _eraseSaves_Pressed(object sender, EventArgs e)
		{
			this._uiGroup.ShowPCDialogScreen(this._deleteStorageDialog, delegate
			{
				if (this._deleteStorageDialog.OptionSelected != -1)
				{
					WaitScreen.DoWait(this._uiGroup, Strings.Deleting_Storage___, delegate
					{
						this._game.SaveDevice.DeleteStorage();
					}, null);
					this._game.FrontEnd.PopToStartScreen();
				}
			});
		}

		public override void OnSelected()
		{
			this._musicVolumeTrack.Value = (int)(this._game.PlayerStats.musicVolume * 100f);
			this._musicMute.Checked = this._game.PlayerStats.musicMute;
			this._autoClimb.Checked = this._game.PlayerStats.AutoClimb;
			base.OnSelected();
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

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (base.SelectedTab)
			{
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
				this._musicVolumeLabel.Scale = (this._musicMute.Scale = (this._autoClimb.Scale = (this._fadeInactiveTray.Scale = Screen.Adjuster.ScaleFactor.Y)));
				if (!this._inGame)
				{
					this._eraseSaves.Scale = Screen.Adjuster.ScaleFactor.Y;
				}
				int num = (int)(50f * Screen.Adjuster.ScaleFactor.Y);
				Point point = new Point(0, (int)(75f * Screen.Adjuster.ScaleFactor.Y));
				int num2 = (int)(200f * Screen.Adjuster.ScaleFactor.Y);
				this._musicVolumeLabel.LocalPosition = point;
				this._musicVolumeTrack.LocalPosition = new Point(point.X + num2, point.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				this._musicVolumeTrack.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), num);
				this._musicMute.LocalPosition = new Point(point.X + num2 * 2, point.Y + (int)(5f * Screen.Adjuster.ScaleFactor.Y));
				point.Y += num;
				this._autoClimb.LocalPosition = point;
				point.Y += num;
				this._fadeInactiveTray.LocalPosition = point;
				if (!this._inGame)
				{
					this._eraseSaves.LocalPosition = new Point((int)(140f * Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
				}
			}
			base.OnUpdate(game, gameTime);
		}

		private TrackBarControl _musicVolumeTrack = new TrackBarControl();

		private TextControl _musicVolumeLabel;

		private CheckBoxControl _musicMute = new CheckBoxControl();

		private CheckBoxControl _autoClimb = new CheckBoxControl();

		private CheckBoxControl _fadeInactiveTray = new CheckBoxControl();

		private FrameButtonControl _eraseSaves = new FrameButtonControl();

		private PCDialogScreen _deleteStorageDialog;

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private bool _inGame;

		private ScreenGroup _uiGroup;

		private Rectangle prevScreenSize;
	}
}
