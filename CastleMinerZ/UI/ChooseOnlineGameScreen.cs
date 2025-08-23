using System;
using System.Collections.Generic;
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
	public class ChooseOnlineGameScreen : ScrollingListScreen
	{
		public ChooseOnlineGameScreen()
			: base(false, new Size(692, 60), new Rectangle(50, 200, 1180, 500))
		{
			this.ClickSound = "Click";
			this._serverPasswordScreen = new PCKeyboardInputScreen(this._game, Strings.Server_Password, Strings.Enter_a_password_for_this_server + ": ", this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._serverPasswordScreen.ClickSound = "Click";
			this._serverPasswordScreen.OpenSound = "Popup";
			Color transpGreen = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			base.SelectButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 125),
				Size = new Size(300, 40),
				Text = Strings.Join_Game,
				Font = this._game._medFont,
				Frame = this._game.ButtonFrame,
				ButtonColor = transpGreen
			};
			this._refreshButton = new FrameButtonControl();
			this._refreshButton.LocalPosition = new Point(900, 170);
			this._refreshButton.Size = new Size(300, 40);
			this._refreshButton.Text = Strings.Search_Again;
			this._refreshButton.Font = this._game._medFont;
			this._refreshButton.Frame = this._game.ButtonFrame;
			this._refreshButton.Pressed += this._refreshButton_Pressed;
			this._refreshButton.ButtonColor = transpGreen;
			base.Controls.Add(this._refreshButton);
			base.BackButton = new ImageButtonControl
			{
				Image = this._game._uiSprites["BackArrow"],
				Font = this._game._medFont,
				LocalPosition = new Point(15, 15),
				Text = " " + Strings.Back,
				ImageDefaultColor = transpGreen
			};
			int hostSize = 237;
			Point loc = new Point(40, 100);
			this._nameButton.LocalPosition = loc;
			this._nameButton.Size = new Size(hostSize - 1, 18);
			this._nameButton.Text = "SERVER NAME";
			this._nameButton.Font = this._game._smallFont;
			this._nameButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._nameButton.Frame = this._game.ButtonFrame;
			this._nameButton.Pressed += this._nameButton_Pressed;
			base.Controls.Add(this._nameButton);
			loc.X += hostSize;
			this._dateButton.LocalPosition = loc;
			this._dateButton.Size = new Size(149, 18);
			this._dateButton.Text = "DATE \u02c5";
			this._dateButton.Font = this._game._smallFont;
			this._dateButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._dateButton.Frame = this._game.ButtonFrame;
			this._dateButton.Pressed += this._dateButton_Pressed;
			base.Controls.Add(this._dateButton);
			loc.X += 150;
			this._numPLayersButton.LocalPosition = loc;
			this._numPLayersButton.Size = new Size(74, 18);
			this._numPLayersButton.Text = "PLAYERS";
			this._numPLayersButton.Font = this._game._smallFont;
			this._numPLayersButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._numPLayersButton.Frame = this._game.ButtonFrame;
			this._numPLayersButton.Pressed += this._numPLayersButton_Pressed;
			base.Controls.Add(this._numPLayersButton);
			loc.X += 75;
			this._MaxPlayersButton.LocalPosition = loc;
			this._MaxPlayersButton.Size = new Size(54, 18);
			this._MaxPlayersButton.Text = "MAX";
			this._MaxPlayersButton.Font = this._game._smallFont;
			this._MaxPlayersButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._MaxPlayersButton.Frame = this._game.ButtonFrame;
			this._MaxPlayersButton.Pressed += this._MaxPlayersButton_Pressed;
			base.Controls.Add(this._MaxPlayersButton);
			loc.X += 55;
			this._modeButton.LocalPosition = loc;
			this._modeButton.Size = new Size(105, 18);
			this._modeButton.Text = "GAME MODE";
			this._modeButton.Font = this._game._smallFont;
			this._modeButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._modeButton.Frame = this._game.ButtonFrame;
			this._modeButton.Pressed += this._modeButton_Pressed;
			base.Controls.Add(this._modeButton);
			loc.X += 106;
			this._numberFriendsButton.LocalPosition = loc;
			this._numberFriendsButton.Size = new Size(70, 18);
			this._numberFriendsButton.Text = "FRIENDS";
			this._numberFriendsButton.Font = this._game._smallFont;
			this._numberFriendsButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._numberFriendsButton.Frame = this._game.ButtonFrame;
			this._numberFriendsButton.Pressed += this._numberFriendsButton_Pressed;
			base.Controls.Add(this._numberFriendsButton);
		}

		private void _refreshButton_Pressed(object sender, EventArgs e)
		{
			this._game.GetNetworkSessions(delegate(AvailableNetworkSessionCollection result)
			{
				this.Populate(result);
			});
		}

		private void _numberFriendsButton_Pressed(object sender, EventArgs e)
		{
			this._sortByFriendCount(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		public void ShutdownHostDiscovery()
		{
			if (this._hostDiscovery != null)
			{
				this._hostDiscovery.Shutdown();
			}
			this._hostDiscovery = null;
		}

		public override void OnPoped()
		{
			this.ShutdownHostDiscovery();
			base.OnPoped();
		}

		public override void OnPushed()
		{
			this._currentSort = ChooseOnlineGameScreen.SortBy.NumFriendsDesc;
			this._hostDiscovery = NetworkSession.GetHostDiscoveryObject("CastleMinerZSteam", 4, DNAGame.GetLocalID());
			base.OnPushed();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (this._hostDiscovery != null)
			{
				this._hostDiscovery.Update();
			}
			for (int i = 0; i < this.Items.Count; i++)
			{
				ChooseOnlineGameScreen.OnlineGameMenuItem item = this.Items[i] as ChooseOnlineGameScreen.OnlineGameMenuItem;
				if (item != null)
				{
					item.UpdateServerInfo(this._hostDiscovery);
				}
			}
			if (Screen.Adjuster.ScreenRect != this.prevScreenRect)
			{
				this.prevScreenRect = Screen.Adjuster.ScreenRect;
				int areaWidth = (int)(540f * Screen.Adjuster.ScaleFactor.X);
				this._selectButton.Scale = (this._refreshButton.Scale = (this._MaxPlayersButton.Scale = (this._numPLayersButton.Scale = (this._nameButton.Scale = (this._dateButton.Scale = (this._modeButton.Scale = (this._numberFriendsButton.Scale = Screen.Adjuster.ScaleFactor.X)))))));
				Point loc = new Point(40, 100);
				this._nameButton.LocalPosition = loc;
				loc.X += this._nameButton.Size.Width + 1;
				this._dateButton.LocalPosition = loc;
				loc.X += this._dateButton.Size.Width + 1;
				this._numPLayersButton.LocalPosition = loc;
				loc.X += this._numPLayersButton.Size.Width + 1;
				this._MaxPlayersButton.LocalPosition = loc;
				loc.X += this._MaxPlayersButton.Size.Width + 1;
				this._modeButton.LocalPosition = loc;
				loc.X += this._modeButton.Size.Width + 1;
				this._numberFriendsButton.LocalPosition = loc;
				int xLoc = (int)(740f * Screen.Adjuster.ScaleFactor.X) + areaWidth / 2 - this._selectButton.Size.Width / 2;
				int scaledHeight = this._selectButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				this._selectButton.LocalPosition = new Point(xLoc, this._selectButton.LocalPosition.Y);
				this._refreshButton.LocalPosition = new Point(xLoc, this._selectButton.LocalPosition.Y + scaledHeight);
				this._itemSize.Width = (int)(700f * Screen.Adjuster.ScaleFactor.X);
				this._itemSize.Height = (int)(60f * Screen.Adjuster.ScaleFactor.X);
				for (int j = 0; j < this.Items.Count; j++)
				{
					this.Items[j].Size = this._itemSize;
				}
				int yloc = this._nameButton.LocalPosition.Y + this._nameButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				this._drawArea = new Rectangle((int)(10f * Screen.Adjuster.ScaleFactor.X), yloc, (int)((float)Screen.Adjuster.ScreenRect.Width - 10f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - yloc);
				base._updateControlsOnSort();
			}
			base.OnUpdate(game, gameTime);
		}

		public override void Click()
		{
			ChooseOnlineGameScreen.OnlineGameMenuItem item = (ChooseOnlineGameScreen.OnlineGameMenuItem)base.SelectedItem;
			if (item.NetworkSession.PasswordProtected)
			{
				this._serverPasswordScreen.DefaultText = item.Password;
				this._game.FrontEnd.ShowPCDialogScreen(this._serverPasswordScreen, delegate
				{
					if (this._serverPasswordScreen.OptionSelected != -1)
					{
						string password = this._serverPasswordScreen.TextInput;
						if (!string.IsNullOrWhiteSpace(password))
						{
							item.Password = password;
						}
						this.<>n__FabricatedMethod5();
					}
				});
				return;
			}
			base.Click();
		}

		public void Populate(AvailableNetworkSessionCollection sessions)
		{
			List<ListItemControl> items = new List<ListItemControl>();
			foreach (AvailableNetworkSession sess in sessions)
			{
				if (sess.HostGamertag != Screen.CurrentGamer.Gamertag)
				{
					items.Add(new ChooseOnlineGameScreen.OnlineGameMenuItem(sess, this._itemSize));
				}
			}
			switch (this._currentSort)
			{
			case ChooseOnlineGameScreen.SortBy.DateAsc:
				this._sortByDate(ChooseOnlineGameScreen.SortBy.DateDesc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.DateDesc:
				this._sortByDate(ChooseOnlineGameScreen.SortBy.DateAsc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.NumPlayersAsc:
				this._sortByNumPlayers(ChooseOnlineGameScreen.SortBy.NumPlayersDesc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.NumPlayersDesc:
				this._sortByNumPlayers(ChooseOnlineGameScreen.SortBy.NumPlayersAsc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.ModeAsc:
				this._sortByMode(ChooseOnlineGameScreen.SortBy.ModeDesc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.ModeDesc:
				this._sortByMode(ChooseOnlineGameScreen.SortBy.ModeAsc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.NameAsc:
				this._sortByName(ChooseOnlineGameScreen.SortBy.NameDesc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.NameDesc:
				this._sortByName(ChooseOnlineGameScreen.SortBy.NameAsc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.MaxPlayersAsc:
				this._sortByMaxPLayers(ChooseOnlineGameScreen.SortBy.MaxPlayersDesc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.MaxPlayersDesc:
				this._sortByMaxPLayers(ChooseOnlineGameScreen.SortBy.MaxPlayersAsc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.NumFriendsAsc:
				this._sortByFriendCount(ChooseOnlineGameScreen.SortBy.NumFriendsDesc, items);
				break;
			case ChooseOnlineGameScreen.SortBy.NumFriendsDesc:
				this._sortByFriendCount(ChooseOnlineGameScreen.SortBy.NumFriendsAsc, items);
				break;
			}
			this.Items = items;
			if (this.Items.Count == 0)
			{
				this._modeButton.Visible = (this._nameButton.Visible = (this._dateButton.Visible = (this._numPLayersButton.Visible = (this._MaxPlayersButton.Visible = (this._numberFriendsButton.Visible = false)))));
			}
			else
			{
				this._modeButton.Visible = (this._nameButton.Visible = (this._dateButton.Visible = (this._numPLayersButton.Visible = (this._MaxPlayersButton.Visible = (this._numberFriendsButton.Visible = true)))));
			}
			base.UpdateAfterPopulate();
		}

		private void _resetSortButtonText()
		{
			this._nameButton.Text = "SERVER NAME";
			this._dateButton.Text = "DATE";
			this._numPLayersButton.Text = "PLAYERS";
			this._MaxPlayersButton.Text = "MAX";
			this._modeButton.Text = "GAME MODE";
			this._numberFriendsButton.Text = "FRIENDS";
		}

		private void _modeButton_Pressed(object sender, EventArgs e)
		{
			this._sortByMode(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _sortByMode(ChooseOnlineGameScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseOnlineGameScreen.SortBy.ModeAsc)
			{
				this._modeButton.Text = "GAME MODE \u02c5";
				this._currentSort = ChooseOnlineGameScreen.SortBy.ModeDesc;
				items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByGameModeDesc));
				return;
			}
			this._modeButton.Text = "GAME MODE \u02c4";
			this._currentSort = ChooseOnlineGameScreen.SortBy.ModeAsc;
			items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByGameModeAsc));
		}

		private void _MaxPlayersButton_Pressed(object sender, EventArgs e)
		{
			this._sortByMaxPLayers(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _sortByMaxPLayers(ChooseOnlineGameScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseOnlineGameScreen.SortBy.MaxPlayersDesc)
			{
				this._MaxPlayersButton.Text = "MAX \u02c4";
				this._currentSort = ChooseOnlineGameScreen.SortBy.MaxPlayersAsc;
				items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByMaxAsc));
				return;
			}
			this._MaxPlayersButton.Text = "MAX \u02c5";
			this._currentSort = ChooseOnlineGameScreen.SortBy.MaxPlayersDesc;
			items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByMaxDesc));
		}

		private void _numPLayersButton_Pressed(object sender, EventArgs e)
		{
			this._sortByNumPlayers(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _sortByNumPlayers(ChooseOnlineGameScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseOnlineGameScreen.SortBy.NumPlayersAsc)
			{
				this._numPLayersButton.Text = "PLAYERS \u02c5";
				this._currentSort = ChooseOnlineGameScreen.SortBy.NumPlayersDesc;
				items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByPlayersDesc));
				return;
			}
			this._numPLayersButton.Text = "PLAYERS \u02c4";
			this._currentSort = ChooseOnlineGameScreen.SortBy.NumPlayersAsc;
			items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByPlayersAsc));
		}

		private void _sortByFriendCount(ChooseOnlineGameScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseOnlineGameScreen.SortBy.NumFriendsAsc)
			{
				this._numberFriendsButton.Text = "FRIENDS \u02c5";
				this._currentSort = ChooseOnlineGameScreen.SortBy.NumFriendsDesc;
				items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByFriendCountDesc));
				return;
			}
			this._numberFriendsButton.Text = "FRIENDS \u02c4";
			this._currentSort = ChooseOnlineGameScreen.SortBy.NumFriendsAsc;
			items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByFriendCountAsc));
		}

		private void _dateButton_Pressed(object sender, EventArgs e)
		{
			this._sortByDate(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _sortByDate(ChooseOnlineGameScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseOnlineGameScreen.SortBy.DateDesc)
			{
				this._dateButton.Text = "DATE \u02c4";
				this._currentSort = ChooseOnlineGameScreen.SortBy.DateAsc;
				items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByDateAsc));
				return;
			}
			this._dateButton.Text = "DATE \u02c5";
			this._currentSort = ChooseOnlineGameScreen.SortBy.DateDesc;
			items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByDateDesc));
		}

		private void _nameButton_Pressed(object sender, EventArgs e)
		{
			this._sortByName(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _sortByName(ChooseOnlineGameScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseOnlineGameScreen.SortBy.NameAsc)
			{
				this._nameButton.Text = "SERVER NAME \u02c5";
				this._currentSort = ChooseOnlineGameScreen.SortBy.NameDesc;
				items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByNameDesc));
				return;
			}
			this._nameButton.Text = "SERVER NAME \u02c4";
			this._currentSort = ChooseOnlineGameScreen.SortBy.NameAsc;
			items.Sort(new Comparison<ListItemControl>(ChooseOnlineGameScreen.SortByNameAsc));
		}

		private static bool SortCheckForNulls(ListItemControl a, ListItemControl b, out int comp)
		{
			bool result = true;
			if (a == null)
			{
				if (b == null)
				{
					comp = 0;
				}
				else
				{
					comp = -1;
				}
			}
			else if (b == null)
			{
				comp = 1;
			}
			else
			{
				result = false;
				comp = 0;
			}
			return result;
		}

		private static int SortSubSort(ChooseOnlineGameScreen.OnlineGameMenuItem one, ChooseOnlineGameScreen.OnlineGameMenuItem two, int comp)
		{
			if (comp != 0)
			{
				return comp;
			}
			return Math.Sign(one.Proximity - two.Proximity);
		}

		private static int SortByNameAsc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByName(a, b, ChooseOnlineGameScreen.SortDirection.Ascending);
		}

		private static int SortByNameDesc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByName(a, b, ChooseOnlineGameScreen.SortDirection.Descending);
		}

		private static int SortByName(ListItemControl a, ListItemControl b, ChooseOnlineGameScreen.SortDirection direction)
		{
			int result;
			if (!ChooseOnlineGameScreen.SortCheckForNulls(a, b, out result))
			{
				ChooseOnlineGameScreen.OnlineGameMenuItem one = (ChooseOnlineGameScreen.OnlineGameMenuItem)a;
				ChooseOnlineGameScreen.OnlineGameMenuItem two = (ChooseOnlineGameScreen.OnlineGameMenuItem)b;
				result = string.Compare(one.NetworkSession.ServerMessage, two.NetworkSession.ServerMessage, true) * (int)direction;
				result = ChooseOnlineGameScreen.SortSubSort(one, two, result);
			}
			return result;
		}

		private static int SortByDateAsc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByDate(a, b, ChooseOnlineGameScreen.SortDirection.Ascending);
		}

		private static int SortByDateDesc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByDate(a, b, ChooseOnlineGameScreen.SortDirection.Descending);
		}

		private static int SortByDate(ListItemControl a, ListItemControl b, ChooseOnlineGameScreen.SortDirection direction)
		{
			int result;
			if (!ChooseOnlineGameScreen.SortCheckForNulls(a, b, out result))
			{
				ChooseOnlineGameScreen.OnlineGameMenuItem one = (ChooseOnlineGameScreen.OnlineGameMenuItem)a;
				ChooseOnlineGameScreen.OnlineGameMenuItem two = (ChooseOnlineGameScreen.OnlineGameMenuItem)b;
				result = DateTime.Compare(one.NetworkSession.DateCreated, two.NetworkSession.DateCreated) * (int)direction;
				result = ChooseOnlineGameScreen.SortSubSort(one, two, result);
			}
			return result;
		}

		private static int SortByFriendCountAsc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByFriendCount(a, b, ChooseOnlineGameScreen.SortDirection.Ascending);
		}

		private static int SortByFriendCountDesc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByFriendCount(a, b, ChooseOnlineGameScreen.SortDirection.Descending);
		}

		private static int SortByFriendCount(ListItemControl a, ListItemControl b, ChooseOnlineGameScreen.SortDirection direction)
		{
			int result;
			if (!ChooseOnlineGameScreen.SortCheckForNulls(a, b, out result))
			{
				ChooseOnlineGameScreen.OnlineGameMenuItem one = (ChooseOnlineGameScreen.OnlineGameMenuItem)a;
				ChooseOnlineGameScreen.OnlineGameMenuItem two = (ChooseOnlineGameScreen.OnlineGameMenuItem)b;
				result = Math.Sign(one.NumFriends - two.NumFriends) * (int)direction;
				result = ChooseOnlineGameScreen.SortSubSort(one, two, result);
			}
			return result;
		}

		private static int SortByPlayersAsc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByPlayers(a, b, ChooseOnlineGameScreen.SortDirection.Ascending);
		}

		private static int SortByPlayersDesc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByPlayers(a, b, ChooseOnlineGameScreen.SortDirection.Descending);
		}

		private static int SortByPlayers(ListItemControl a, ListItemControl b, ChooseOnlineGameScreen.SortDirection direction)
		{
			int result;
			if (!ChooseOnlineGameScreen.SortCheckForNulls(a, b, out result))
			{
				ChooseOnlineGameScreen.OnlineGameMenuItem one = (ChooseOnlineGameScreen.OnlineGameMenuItem)a;
				ChooseOnlineGameScreen.OnlineGameMenuItem two = (ChooseOnlineGameScreen.OnlineGameMenuItem)b;
				result = Math.Sign(one.NetworkSession.CurrentGamerCount - two.NetworkSession.CurrentGamerCount) * (int)direction;
				result = ChooseOnlineGameScreen.SortSubSort(one, two, result);
			}
			return result;
		}

		private static int SortByMaxAsc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByMax(a, b, ChooseOnlineGameScreen.SortDirection.Ascending);
		}

		private static int SortByMaxDesc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByMax(a, b, ChooseOnlineGameScreen.SortDirection.Descending);
		}

		private static int SortByMax(ListItemControl a, ListItemControl b, ChooseOnlineGameScreen.SortDirection direction)
		{
			int result;
			if (!ChooseOnlineGameScreen.SortCheckForNulls(a, b, out result))
			{
				ChooseOnlineGameScreen.OnlineGameMenuItem one = (ChooseOnlineGameScreen.OnlineGameMenuItem)a;
				ChooseOnlineGameScreen.OnlineGameMenuItem two = (ChooseOnlineGameScreen.OnlineGameMenuItem)b;
				result = Math.Sign(one.NetworkSession.MaxGamerCount - two.NetworkSession.MaxGamerCount) * (int)direction;
				result = ChooseOnlineGameScreen.SortSubSort(one, two, result);
			}
			return result;
		}

		private static int SortByGameModeAsc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByGameMode(a, b, ChooseOnlineGameScreen.SortDirection.Ascending);
		}

		private static int SortByGameModeDesc(ListItemControl a, ListItemControl b)
		{
			return ChooseOnlineGameScreen.SortByGameMode(a, b, ChooseOnlineGameScreen.SortDirection.Descending);
		}

		private static int SortByGameMode(ListItemControl a, ListItemControl b, ChooseOnlineGameScreen.SortDirection direction)
		{
			int result;
			if (!ChooseOnlineGameScreen.SortCheckForNulls(a, b, out result))
			{
				ChooseOnlineGameScreen.OnlineGameMenuItem one = (ChooseOnlineGameScreen.OnlineGameMenuItem)a;
				ChooseOnlineGameScreen.OnlineGameMenuItem two = (ChooseOnlineGameScreen.OnlineGameMenuItem)b;
				result = string.Compare(one.GameModeString, two.GameModeString, true) * (int)direction;
				result = ChooseOnlineGameScreen.SortSubSort(one, two, result);
			}
			return result;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont font = this._game._largeFont;
			spriteBatch.Begin();
			if (this.Items.Count == 0)
			{
				string msg = Strings.No_Servers_Available;
				Vector2 size = font.MeasureString(msg);
				int lineSpacing = font.LineSpacing;
				spriteBatch.DrawOutlinedText(font, msg, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - size.X / 2f, (float)Screen.Adjuster.ScreenRect.Center.Y - size.Y / 2f), CMZColors.MenuGreen, Color.Black, 1);
			}
			else
			{
				string msg = Strings.Choose_A_Server;
				Vector2 size = font.MeasureString(msg);
				spriteBatch.DrawOutlinedText(font, msg, new Vector2((float)Screen.Adjuster.ScreenRect.Center.X - size.X / 2f, 10f), CMZColors.MenuGreen, Color.Black, 1);
				ChooseOnlineGameScreen.OnlineGameMenuItem selected = (ChooseOnlineGameScreen.OnlineGameMenuItem)base.SelectedItem;
				selected.DrawSelected(spriteBatch, new Vector2((float)this._refreshButton.LocalPosition.X, (float)(this._refreshButton.LocalPosition.Y + this._refreshButton.Size.Height + 5)));
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private HostDiscovery _hostDiscovery;

		private ChooseOnlineGameScreen.SortBy _currentSort = ChooseOnlineGameScreen.SortBy.NumFriendsDesc;

		private FrameButtonControl _dateButton = new FrameButtonControl();

		private FrameButtonControl _numPLayersButton = new FrameButtonControl();

		private FrameButtonControl _modeButton = new FrameButtonControl();

		private FrameButtonControl _nameButton = new FrameButtonControl();

		private FrameButtonControl _MaxPlayersButton = new FrameButtonControl();

		private FrameButtonControl _numberFriendsButton = new FrameButtonControl();

		private FrameButtonControl _refreshButton = new FrameButtonControl();

		private PCKeyboardInputScreen _serverPasswordScreen;

		private Rectangle prevScreenRect;

		private enum SortBy
		{
			DateAsc,
			DateDesc,
			NumPlayersAsc,
			NumPlayersDesc,
			ModeAsc,
			ModeDesc,
			NameAsc,
			NameDesc,
			MaxPlayersAsc,
			MaxPlayersDesc,
			NumFriendsAsc,
			NumFriendsDesc
		}

		private enum SortDirection
		{
			Ascending = 1,
			Descending = -1
		}

		public class OnlineGameMenuItem : ListItemControl
		{
			public AvailableNetworkSession NetworkSession
			{
				get
				{
					return this._serverInfo.Session;
				}
			}

			public string GameModeString
			{
				get
				{
					return this._serverInfo.GameModeString;
				}
			}

			public int NumFriends
			{
				get
				{
					return this._serverInfo.NumFriends;
				}
			}

			public int Proximity
			{
				get
				{
					return this._serverInfo.Proximity;
				}
			}

			protected override void OnUpdate(DNAGame game, GameTime gameTime)
			{
				this.Active = this._serverInfo.IsOnline;
				base.OnUpdate(game, gameTime);
			}

			public OnlineGameMenuItem(AvailableNetworkSession networkSession, Size itemSize)
				: base(itemSize)
			{
				networkSession.ConvertToIPV4();
				this._dateCreated = networkSession.DateCreated.ToString();
				this._serverInfo = new ServerInfo(networkSession);
			}

			public void UpdateServerInfo(HostDiscovery hostDiscovery)
			{
				this._serverInfo.RefreshServerStatus(hostDiscovery);
			}

			protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
			{
				base.OnDraw(device, spriteBatch, gameTime);
				Color color = this.TextColor;
				if (base.CaptureInput || this.Selected)
				{
					color = this.TextPressedColor;
				}
				else if (base.Hovering)
				{
					color = this.TextHoverColor;
				}
				Vector2 loc = new Vector2((float)base.LocalPosition.X + 10f * Screen.Adjuster.ScaleFactor.X, (float)base.LocalPosition.Y + 5f * Screen.Adjuster.ScaleFactor.X);
				if (this._serverInfo.IsOnline)
				{
					spriteBatch.DrawString(this._medFont, this.NetworkSession.ServerMessage, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				else
				{
					spriteBatch.DrawString(this._medFont, this._serverInfo.ServerName, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				string hostName = this.NetworkSession.HostGamertag;
				if (!hostName.Equals("[unknown]", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(hostName))
				{
					spriteBatch.DrawString(this._smallFont, Strings.Hosted_By + ": " + hostName, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				int hostSize = 237;
				loc.X += (float)hostSize * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(this._smallFont, this._dateCreated, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.X += 175f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(this._smallFont, this._serverInfo.NumberPlayerString, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.X += 30f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(this._smallFont, "/", loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.X += (30f + this._smallFont.MeasureString("/").X) * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(this._smallFont, this._serverInfo.MaxPlayersString, new Vector2(loc.X - this._smallFont.MeasureString(this._serverInfo.MaxPlayersString).X, loc.Y), color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.X += 35f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(this._smallFont, this._serverInfo.GameModeString, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.X += 106f * Screen.Adjuster.ScaleFactor.X;
				if ((float)this.Size.Width > loc.X)
				{
					spriteBatch.DrawString(this._smallFont, this._serverInfo.NumFriendsStr, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
			}

			public void DrawSelected(SpriteBatch spriteBatch, Vector2 loc)
			{
				Color color = this.TextColor;
				if (base.CaptureInput || this.Selected)
				{
					color = this.TextPressedColor;
				}
				else if (base.Hovering)
				{
					color = this.TextHoverColor;
				}
				spriteBatch.DrawOutlinedText(this._medFont, this.NetworkSession.ServerMessage, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				string hostName = this.NetworkSession.HostGamertag;
				if (!hostName.Equals("[unknown]", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(hostName))
				{
					spriteBatch.DrawOutlinedText(this._medFont, Strings.Hosted_By + ": " + hostName, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				}
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, Strings.Created + ": " + this._dateCreated, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, Strings.Players + ": ", loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				Vector2 newLoc = loc;
				newLoc.X += 100f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, this._serverInfo.NumberPlayerString, newLoc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				newLoc.X += 40f * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, "/", newLoc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				newLoc.X += (40f + this._medFont.MeasureString("/").X) * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, this._serverInfo.MaxPlayersString, new Vector2(newLoc.X - this._medFont.MeasureString(this._serverInfo.MaxPlayersString).X, loc.Y), Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(this._medFont, Strings.Game_Mode + ": " + this._serverInfo.GameModeString, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawString(this._medFont, "PVP: " + this._serverInfo.PVPstr, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				if (this._serverInfo.GameMode == GameModeTypes.Survival || this._serverInfo.GameMode == GameModeTypes.Scavenger)
				{
					loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
					spriteBatch.DrawString(this._medFont, Strings.Difficulty + ": " + this._serverInfo.GameDifficultyString, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
				if (this._serverInfo.PasswordProtected)
				{
					loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X * 2f;
					spriteBatch.DrawString(this._medFont, Strings.Server_Requires_A_Password, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
			}

			private ServerInfo _serverInfo;

			public string Password = "";

			private SpriteFont _largeFont = CastleMinerZGame.Instance._medLargeFont;

			private SpriteFont _medFont = CastleMinerZGame.Instance._medFont;

			private SpriteFont _smallFont = CastleMinerZGame.Instance._smallFont;

			private string _dateCreated;
		}
	}
}
