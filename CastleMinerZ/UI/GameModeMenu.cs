using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class GameModeMenu : MenuScreen
	{
		public GameModeMenu(CastleMinerZGame game)
			: base(game._largeFont, CMZColors.MenuGreen, Color.White, false)
		{
			this._game = game;
			SpriteFont largeFont = this._game._largeFont;
			this.SelectSound = "Click";
			this.ClickSound = "Click";
			this._descriptionText = new TextRegionElement(this._game._medLargeFont);
			this.HorizontalAlignment = MenuScreen.HorizontalAlignmentTypes.Right;
			this.VerticalAlignment = MenuScreen.VerticalAlignmentTypes.Top;
			this.LineSpacing = new int?(-10);
			base.AddMenuItem(Strings.Endurance, Strings.Earn_awards_by_seeing_how_far_you_can_travel_from_the_start_point__Changes_to_the_world_will_not_be_saved_in_this_mode_, GameModeTypes.Endurance);
			this.SurvivalControl = base.AddMenuItem(Strings.Survival, Strings.Mine_resources_and_build_your_fortress_while_defending_yourself_from_the_undead_horde__Your_creations_will_be_saved_in_this_mode__You_can_play_with_or_without_enemies_, GameModeTypes.Survival);
			this.DragonEnduranceControl = base.AddMenuItem(Strings.Dragon_Endurance, Strings.Fend_off_wave_after_wave_of_dragons__Unlock_this_mode_by_defeating_the_undead_dragon_in_Endurance_Mode__Your_creations_will_be_saved_in_this_mode_, GameModeTypes.DragonEndurance);
			this.CreativeControl = base.AddMenuItem(Strings.Creative, Strings.Build_structures_with_unlimited_resources__You_can_play_with_or_without_enemies, GameModeTypes.Creative);
			this.ExplorationControl = base.AddMenuItem(Strings.Exploration, Strings.Exploration_description, GameModeTypes.Exploration);
			this.ScavengerControl = base.AddMenuItem(Strings.Scavenger, Strings.Scavenger_description, GameModeTypes.Scavenger);
			base.AddMenuItem(Strings.Back, Strings.Back_to_main_menu_, null);
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			this._descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screenRect = Screen.Adjuster.ScreenRect;
			int width = (int)(512f * Screen.Adjuster.ScaleFactor.Y);
			int height = this._game.Logo.Height * width / this._game.Logo.Width;
			this.DrawArea = new Rectangle?(new Rectangle(0, (int)((double)height * 0.75), (int)((float)(Screen.Adjuster.ScreenRect.Width / 2) - 125f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - height));
			this._descriptionText.Location = new Vector2((float)Screen.Adjuster.ScreenRect.Center.X + 50f * Screen.Adjuster.ScaleFactor.X, (float)this.DrawArea.Value.Y + 20f * Screen.Adjuster.ScaleFactor.Y);
			this._descriptionText.Size = new Vector2((float)Screen.Adjuster.ScreenRect.Right - this._descriptionText.Location.X - 10f, (float)Screen.Adjuster.ScreenRect.Bottom - this._descriptionText.Location.Y);
			string title = Strings.Choose_a_Game_Mode;
			spriteBatch.Begin();
			spriteBatch.DrawOutlinedText(this._game._largeFont, title, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - this._game._largeFont.MeasureString(title).X / 2f, 0f), CMZColors.MenuGreen, Color.Black, 2);
			this._descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			this.DragonEnduranceControl.TextColor = new Color?((CastleMinerZGame.Instance.PlayerStats.UndeadDragonKills > 0) ? CMZColors.MenuGreen : Color.Gray);
			this.SurvivalControl.TextColor = new Color?(CMZColors.MenuGreen);
			this.CreativeControl.TextColor = new Color?(CMZColors.MenuGreen);
			this.ExplorationControl.TextColor = new Color?(CMZColors.MenuGreen);
			this.ScavengerControl.TextColor = new Color?(CMZColors.MenuGreen);
			base.OnUpdate(game, gameTime);
		}

		private CastleMinerZGame _game;

		private TextRegionElement _descriptionText;

		private MenuItemElement SurvivalControl;

		private MenuItemElement DragonEnduranceControl;

		private MenuItemElement CreativeControl;

		private MenuItemElement ExplorationControl;

		private MenuItemElement ScavengerControl;
	}
}
