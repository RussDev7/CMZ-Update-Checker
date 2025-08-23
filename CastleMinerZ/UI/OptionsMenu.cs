using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class OptionsMenu : MenuScreen
	{
		public OptionsMenu(CastleMinerZGame game, ScreenGroup uiGroup, SpriteBatch spriteBatch)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, false)
		{
			this.SpriteBatch = spriteBatch;
			SpriteFont largeFont = game._largeFont;
			this._uiGroup = uiGroup;
			this._game = game;
			this.ClickSound = "Click";
			this.SelectSound = "Click";
			this.HorizontalAlignment = MenuScreen.HorizontalAlignmentTypes.Right;
			this.VerticalAlignment = MenuScreen.VerticalAlignmentTypes.Top;
			this.LineSpacing = new int?(-10);
			this._descriptionText = new TextRegionElement(this._game._medLargeFont);
			base.AddMenuItem(Strings.Controls, Strings.View_in_game_controls_and_settings, OptionsMenuItems.Controls);
			base.AddMenuItem(Strings.Erase_Storage, Strings.Erase_all_worlds_and_stats_, OptionsMenuItems.EraseStorage);
			base.AddMenuItem(Strings.Settings, Strings.Change_game_settings_such_as_volume_and_brightness_, OptionsMenuItems.Settings);
			base.AddMenuItem(Strings.Release_Notes, Strings.View_the_release_notes_, OptionsMenuItems.ReleaseNotes);
			base.AddMenuItem(Strings.Back, Strings.Back_to_main_menu_, OptionsMenuItems.Back);
			this._deleteStorageDialog = new PCDialogScreen(Strings.Erase_Storage, Strings.Are_you_sure_you_want_to_delete_everything_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._deleteStorageDialog.UseDefaultValues();
			this._controllerScreen = new ControllerScreen(this._game, false, this._uiGroup);
			this._settingsMenu = new SettingsMenu(this._game);
			base.MenuItemSelected += this.OptionsMenu_MenuItemSelected;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			int width = (int)(512f * Screen.Adjuster.ScaleFactor.Y);
			int height = this._game.Logo.Height * width / this._game.Logo.Width;
			this.DrawArea = new Rectangle?(new Rectangle(0, (int)((double)height * 0.75), (int)((float)(Screen.Adjuster.ScreenRect.Width / 2) - 125f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - height));
			this._descriptionText.Location = new Vector2((float)Screen.Adjuster.ScreenRect.Center.X + 50f * Screen.Adjuster.ScaleFactor.X, (float)this.DrawArea.Value.Y + 20f * Screen.Adjuster.ScaleFactor.Y);
			this._descriptionText.Size = new Vector2((float)Screen.Adjuster.ScreenRect.Right - this._descriptionText.Location.X - 10f, (float)Screen.Adjuster.ScreenRect.Bottom - this._descriptionText.Location.Y);
			spriteBatch.Begin();
			this._descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			this._descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		private void OptionsMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			switch ((OptionsMenuItems)e.MenuItem.Tag)
			{
			case OptionsMenuItems.Controls:
				this._uiGroup.PushScreen(this._controllerScreen);
				return;
			case OptionsMenuItems.EraseStorage:
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
				return;
			case OptionsMenuItems.Settings:
				this._uiGroup.PushScreen(this._settingsMenu);
				return;
			case OptionsMenuItems.ReleaseNotes:
				this._game.FrontEnd.PushReleaseNotesScreen();
				return;
			case OptionsMenuItems.Back:
				base.PopMe();
				return;
			default:
				return;
			}
		}

		private CastleMinerZGame _game;

		public PCDialogScreen _deleteStorageDialog;

		private ControllerScreen _controllerScreen;

		private SettingsMenu _settingsMenu;

		private SpriteBatch SpriteBatch;

		private ScreenGroup _uiGroup;

		private TextRegionElement _descriptionText;

		private bool Cancel;
	}
}
