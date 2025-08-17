using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class HostOptions : MenuScreen
	{
		public HostOptions(CastleMinerZGame game)
			: base(game._largeFont, false)
		{
			this.TextColor = CMZColors.MenuGreen;
			this.SelectedColor = Color.White;
			this._game = game;
			this.ClickSound = "Click";
			this.SelectSound = "Click";
			base.AddMenuItem(Strings.Return_To_Game, HostOptionItems.Return);
			base.AddMenuItem(Strings.Kick_Player, HostOptionItems.KickPlayer);
			base.AddMenuItem(Strings.Ban_Player, HostOptionItems.BanPlayer);
			base.AddMenuItem(Strings.Restart, HostOptionItems.Restart);
			this.pvpItem = base.AddMenuItem("PVP:", HostOptionItems.PVP);
			base.AddMenuItem(Strings.Set_Password, HostOptionItems.Password);
			base.AddMenuItem(Strings.Server_Message, HostOptionItems.ServerMessage);
			this.banListItem = base.AddMenuItem(Strings.Clear_Ban_List, HostOptionItems.ClearBanList);
			this.joiningPolicyItem = base.AddMenuItem(Strings.Who_can_join, HostOptionItems.ChangeJoinPolicy);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			switch (this._game.PVPState)
			{
			case CastleMinerZGame.PVPEnum.Off:
				this.pvpItem.Text = "PVP: " + Strings.Off;
				break;
			case CastleMinerZGame.PVPEnum.Everyone:
				this.pvpItem.Text = "PVP: " + Strings.Everyone;
				break;
			case CastleMinerZGame.PVPEnum.NotFriends:
				this.pvpItem.Text = "PVP: " + Strings.Non_Friends_Only;
				break;
			}
			this.joiningPolicyItem.Text = Strings.Who_can_join + " ";
			switch (this._game.JoinGamePolicy)
			{
			case JoinGamePolicy.Anyone:
			{
				MenuItemElement menuItemElement = this.joiningPolicyItem;
				menuItemElement.Text += Strings.Anyone;
				break;
			}
			case JoinGamePolicy.FriendsOnly:
			{
				MenuItemElement menuItemElement2 = this.joiningPolicyItem;
				menuItemElement2.Text += Strings.Friends_only;
				break;
			}
			case JoinGamePolicy.InviteOnly:
			{
				MenuItemElement menuItemElement3 = this.joiningPolicyItem;
				menuItemElement3.Text += Strings.Invitation_only;
				break;
			}
			}
			this.banListItem.Visible = this._game.PlayerStats.BanList.Count > 0;
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			Rectangle rectangle = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
			spriteBatch.Draw(this._game.DummyTexture, rectangle, new Color(0f, 0f, 0f, 0.5f));
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private CastleMinerZGame _game;

		private MenuItemElement pvpItem;

		private MenuItemElement banListItem;

		private MenuItemElement joiningPolicyItem;
	}
}
