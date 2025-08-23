using System;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class HostOptionsTab : TabControl.TabPage
	{
		public HostOptionsTab(bool inGame)
			: base(Strings.Host_Options)
		{
			this._inGame = inGame;
			this._game = CastleMinerZGame.Instance;
			this._controlsFont = this._game._medFont;
			Color btnColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			this._restartDialog = new PCDialogScreen(Strings.Restart_Game, Strings.Are_you_sure_you_want_to_restart_this_game_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._restartDialog.UseDefaultValues();
			this._playerDropList.Frame = this._game.ButtonFrame;
			this._playerDropList.DropArrow = this._game._uiSprites["DropArrow"];
			this._playerDropList.Font = this._controlsFont;
			base.Children.Add(this._playerDropList);
			this._kickPlayerButton.Size = new Size(200, this._controlsFont.LineSpacing);
			this._kickPlayerButton.Text = Strings.Kick_Player;
			this._kickPlayerButton.Font = this._controlsFont;
			this._kickPlayerButton.Frame = this._game.ButtonFrame;
			this._kickPlayerButton.Pressed += this._kickPlayerButton_Pressed;
			base.Children.Add(this._kickPlayerButton);
			this._banPlayerButton.Size = new Size(200, this._controlsFont.LineSpacing);
			this._banPlayerButton.Text = Strings.Ban_Player;
			this._banPlayerButton.Font = this._controlsFont;
			this._banPlayerButton.Frame = this._game.ButtonFrame;
			this._banPlayerButton.Pressed += this._banPlayerButton_Pressed;
			base.Children.Add(this._banPlayerButton);
			this._clearBanListButton.Size = new Size(200, this._controlsFont.LineSpacing);
			this._clearBanListButton.Text = Strings.Clear_Ban_List;
			this._clearBanListButton.Font = this._controlsFont;
			this._clearBanListButton.Frame = this._game.ButtonFrame;
			this._clearBanListButton.Pressed += this._clearBanListButton_Pressed;
			base.Children.Add(this._clearBanListButton);
			this._restartButton.Size = new Size(135, this._controlsFont.LineSpacing);
			this._restartButton.Text = Strings.Restart;
			this._restartButton.Font = this._controlsFont;
			this._restartButton.Frame = this._game.ButtonFrame;
			this._restartButton.Pressed += this._restartButton_Pressed;
			this._restartButton.ButtonColor = btnColor;
			base.Children.Add(this._restartButton);
			this._pvpLabel = new TextControl("PVP:", this._controlsFont);
			base.Children.Add(this._pvpLabel);
			this._pvpDropList.Items.Add(Strings.Off);
			this._pvpDropList.Items.Add(Strings.On);
			this._pvpDropList.Items.Add(Strings.Non_Friends_Only);
			this._pvpDropList.Frame = this._game.ButtonFrame;
			this._pvpDropList.DropArrow = this._game._uiSprites["DropArrow"];
			this._pvpDropList.Font = this._controlsFont;
			this._pvpDropList.SelectedIndexChanged += this._pvpStr_SelectedIndexChanged;
			this._passwordLabel = new TextControl(Strings.Password + ":", this._controlsFont);
			base.Children.Add(this._passwordLabel);
			this._passwordTextbox.Frame = this._game.ButtonFrame;
			this._passwordTextbox.Font = this._controlsFont;
			this._passwordTextbox.Size = new Size(200, 40);
			base.Children.Add(this._passwordTextbox);
			this._serverMessageLabel = new TextControl(Strings.Server_Message + ":", this._controlsFont);
			base.Children.Add(this._serverMessageLabel);
			this._serverMessageTextbox.Frame = this._game.ButtonFrame;
			this._serverMessageTextbox.Font = this._controlsFont;
			this._serverMessageTextbox.Size = new Size(200, 40);
			base.Children.Add(this._serverMessageTextbox);
			this._whoCanJoinLabel = new TextControl(Strings.Who_can_join, this._controlsFont);
			base.Children.Add(this._whoCanJoinLabel);
			this._whoCanJoinDropList.Items.Add(Strings.Everyone);
			this._whoCanJoinDropList.Items.Add(Strings.Friends_only);
			this._whoCanJoinDropList.Items.Add(Strings.Invitation_only);
			this._whoCanJoinDropList.Frame = this._game.ButtonFrame;
			this._whoCanJoinDropList.DropArrow = this._game._uiSprites["DropArrow"];
			this._whoCanJoinDropList.Font = this._controlsFont;
			this._whoCanJoinDropList.SelectedIndexChanged += this._whoCanJoinDropList_SelectedIndexChanged;
			base.Children.Add(this._whoCanJoinDropList);
			base.Children.Add(this._pvpDropList);
		}

		public override void OnSelected()
		{
			this._pvpDropList.SelectedIndex = (int)this._game.PVPState;
			this._whoCanJoinDropList.SelectedIndex = (int)this._game.JoinGamePolicy;
			this._passwordTextbox.Text = this._game.CurrentWorld.ServerPassword;
			this._serverMessageTextbox.Text = this._game.ServerMessage;
			this._playerDropList.Items.Clear();
			this._playerDropList.Items.Add(new HostOptionsTab.PlayerItem(null));
			for (int i = 0; i < this._game.CurrentNetworkSession.RemoteGamers.Count; i++)
			{
				NetworkGamer gamer = this._game.CurrentNetworkSession.RemoteGamers[i];
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					this._playerDropList.Items.Add(new HostOptionsTab.PlayerItem(player));
				}
			}
			base.OnSelected();
		}

		private void _setPassword(string password)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				if (!string.IsNullOrWhiteSpace(this._game.CurrentWorld.ServerPassword))
				{
					if (this._game.IsOnlineGame)
					{
						this._game.CurrentNetworkSession.UpdateHostSession(null, new bool?(false), null, null);
					}
					this._game.CurrentWorld.ServerPassword = "";
					this._game.CurrentNetworkSession.Password = null;
					return;
				}
			}
			else if (this._game.CurrentWorld.ServerPassword != this._passwordTextbox.Text)
			{
				if (this._game.IsOnlineGame)
				{
					this._game.CurrentNetworkSession.UpdateHostSession(null, new bool?(true), null, null);
				}
				this._game.CurrentWorld.ServerPassword = password;
				this._game.CurrentNetworkSession.Password = password;
			}
		}

		private void _setServerMessage(string name)
		{
			if (!string.IsNullOrWhiteSpace(name) && this._game.ServerMessage != name.Trim())
			{
				this._game.ServerMessage = name.Trim();
			}
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (base.SelectedTab)
			{
				this._kickPlayerButton.Enabled = (this._banPlayerButton.Enabled = this._playerDropList.SelectedIndex > 0);
				this._clearBanListButton.Enabled = this._game.PlayerStats.BanList.Count > 0;
				if (!this._passwordTextbox.HasFocus)
				{
					this._setPassword(this._passwordTextbox.Text);
				}
				if (!this._serverMessageTextbox.HasFocus)
				{
					this._setServerMessage(this._serverMessageTextbox.Text);
				}
			}
			if (this.prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				this.prevScreenSize = Screen.Adjuster.ScreenRect;
				this._playerDropList.Scale = (this._kickPlayerButton.Scale = (this._banPlayerButton.Scale = (this._clearBanListButton.Scale = (this._restartButton.Scale = (this._pvpLabel.Scale = (this._pvpDropList.Scale = (this._passwordLabel.Scale = (this._passwordTextbox.Scale = (this._serverMessageLabel.Scale = (this._serverMessageTextbox.Scale = (this._whoCanJoinLabel.Scale = (this._whoCanJoinDropList.Scale = Screen.Adjuster.ScaleFactor.Y))))))))))));
				int height = (int)(50f * Screen.Adjuster.ScaleFactor.Y);
				Point loc = new Point(0, (int)(75f * Screen.Adjuster.ScaleFactor.Y));
				int btnOffset = (int)(210f * Screen.Adjuster.ScaleFactor.Y);
				this._restartButton.LocalPosition = new Point((int)(140f * Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
				this._pvpLabel.LocalPosition = loc;
				this._pvpDropList.LocalPosition = new Point(loc.X + btnOffset, loc.Y);
				loc.Y += height;
				this._serverMessageLabel.LocalPosition = loc;
				this._serverMessageTextbox.LocalPosition = new Point(loc.X + btnOffset, loc.Y);
				loc.Y += height;
				this._passwordLabel.LocalPosition = loc;
				this._passwordTextbox.LocalPosition = new Point(loc.X + btnOffset, loc.Y);
				loc.Y += height;
				this._whoCanJoinLabel.LocalPosition = loc;
				this._whoCanJoinDropList.LocalPosition = new Point(loc.X + btnOffset, loc.Y);
				loc.Y += height;
				this._playerDropList.LocalPosition = loc;
				this._kickPlayerButton.LocalPosition = new Point(loc.X + btnOffset, loc.Y);
				this._banPlayerButton.LocalPosition = new Point(loc.X + btnOffset * 2, loc.Y);
				this._clearBanListButton.LocalPosition = new Point(loc.X + btnOffset * 3, loc.Y);
			}
			base.OnUpdate(game, gameTime);
		}

		public override void OnLostFocus()
		{
			this._setPassword(this._passwordTextbox.Text);
			this._setServerMessage(this._serverMessageTextbox.Text);
			base.OnLostFocus();
		}

		private void _whoCanJoinDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._game.JoinGamePolicy = (JoinGamePolicy)this._whoCanJoinDropList.SelectedIndex;
		}

		private void _pvpStr_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._game.PVPState = (CastleMinerZGame.PVPEnum)this._pvpDropList.SelectedIndex;
			string txt = "";
			switch (this._game.PVPState)
			{
			case CastleMinerZGame.PVPEnum.Off:
				txt = "PVP: " + Strings.Off;
				break;
			case CastleMinerZGame.PVPEnum.Everyone:
				txt = "PVP: " + Strings.Everyone;
				break;
			case CastleMinerZGame.PVPEnum.NotFriends:
				txt = "PVP: " + Strings.Non_Friends_Only;
				break;
			}
			BroadcastTextMessage.Send(this._game.MyNetworkGamer, txt);
		}

		private void _restartButton_Pressed(object sender, EventArgs e)
		{
			this._game.GameScreen._uiGroup.ShowPCDialogScreen(this._restartDialog, delegate
			{
				if (this._restartDialog.OptionSelected != -1)
				{
					RestartLevelMessage.Send((LocalNetworkGamer)this._game.LocalPlayer.Gamer);
					BroadcastTextMessage.Send(this._game.MyNetworkGamer, this._game.LocalPlayer.Gamer.Gamertag + " " + Strings.Has_Restarted_The_Game);
					this._game.GameScreen.PopToHUD();
				}
			});
		}

		private void _clearBanListButton_Pressed(object sender, EventArgs e)
		{
			this._game.PlayerStats.BanList.Clear();
		}

		private void _banPlayerButton_Pressed(object sender, EventArgs e)
		{
			BroadcastTextMessage.Send(this._game.MyNetworkGamer, this._playerDropList.SelectedItem.Player.Gamer.Gamertag + " " + Strings.has_been_banned_by_the_host);
			KickMessage.Send(this._game.MyNetworkGamer, this._playerDropList.SelectedItem.Player.Gamer, true);
			this._game.PlayerStats.BanList[this._playerDropList.SelectedItem.Player.Gamer.AlternateAddress] = DateTime.UtcNow;
			this._game.SaveData();
		}

		private void _kickPlayerButton_Pressed(object sender, EventArgs e)
		{
			BroadcastTextMessage.Send(this._game.MyNetworkGamer, this._playerDropList.SelectedItem.Player.Gamer.Gamertag + " " + Strings.has_been_kicked_by_the_host);
			KickMessage.Send(this._game.MyNetworkGamer, this._playerDropList.SelectedItem.Player.Gamer, false);
		}

		private DropListControl<HostOptionsTab.PlayerItem> _playerDropList = new DropListControl<HostOptionsTab.PlayerItem>();

		private FrameButtonControl _kickPlayerButton = new FrameButtonControl();

		private FrameButtonControl _banPlayerButton = new FrameButtonControl();

		private FrameButtonControl _clearBanListButton = new FrameButtonControl();

		private FrameButtonControl _restartButton = new FrameButtonControl();

		private TextControl _pvpLabel;

		private DropListControl<string> _pvpDropList = new DropListControl<string>();

		private TextControl _passwordLabel;

		private TextEditControl _passwordTextbox = new TextEditControl();

		private TextControl _serverMessageLabel;

		private TextEditControl _serverMessageTextbox = new TextEditControl();

		private TextControl _whoCanJoinLabel;

		private DropListControl<string> _whoCanJoinDropList = new DropListControl<string>();

		private PCDialogScreen _restartDialog;

		private bool _inGame;

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private Rectangle prevScreenSize;

		private struct PlayerItem
		{
			public PlayerItem(Player player)
			{
				this.Player = player;
			}

			public override string ToString()
			{
				if (this.Player == null)
				{
					return Strings.Choose_a_Player;
				}
				return this.Player.Gamer.Gamertag;
			}

			public Player Player;
		}
	}
}
