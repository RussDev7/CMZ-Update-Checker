using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class ChooseSavedWorldScreen : ScrollingListScreen
	{
		private WorldManager WorldManager
		{
			get
			{
				return this._game.FrontEnd.WorldManager;
			}
		}

		public ButtonControl DeleteButton
		{
			set
			{
				if (this._deleteButton != null)
				{
					base.Controls.Remove(this._deleteButton);
				}
				this._deleteButton = value;
				this._deleteButton.Pressed += this._deleteButton_Pressed;
				base.Controls.Add(this._deleteButton);
			}
		}

		private void _deleteButton_Pressed(object sender, EventArgs e)
		{
			this._game.FrontEnd._uiGroup.ShowPCDialogScreen(this._deleteWorldDialog, delegate
			{
				if (this._deleteWorldDialog.OptionSelected != -1)
				{
					ChooseSavedWorldScreen.SavedWorldItem selected = (ChooseSavedWorldScreen.SavedWorldItem)base.SelectedItem;
					WaitScreen.DoWait(this._game.FrontEnd._uiGroup, Strings.Deleting_World___, delegate
					{
						this.WorldManager.Delete(selected.World);
						this.Items.Remove(selected);
						this._updateControlsOnSort();
						this._game.SaveDevice.Flush();
						if (this.Items.Count == 0)
						{
							this._deleteButton.Visible = false;
							this._renameButton.Visible = false;
						}
					}, null);
				}
			});
		}

		public ButtonControl RenameButton
		{
			set
			{
				if (this._renameButton != null)
				{
					base.Controls.Remove(this._renameButton);
				}
				this._renameButton = value;
				this._renameButton.Pressed += this._renameButton_Pressed;
				base.Controls.Add(this._renameButton);
			}
		}

		private void _renameButton_Pressed(object sender, EventArgs e)
		{
			ChooseSavedWorldScreen.SavedWorldItem selected = (ChooseSavedWorldScreen.SavedWorldItem)base.SelectedItem;
			this._keyboardInputScreen.Title = Strings.Rename + " " + selected.World.Name;
			this._keyboardInputScreen.DefaultText = selected.World.Name;
			CastleMinerZGame.Instance.FrontEnd._uiGroup.ShowPCDialogScreen(this._keyboardInputScreen, delegate
			{
				if (this._keyboardInputScreen.OptionSelected != -1)
				{
					string nameText = this._keyboardInputScreen.TextInput;
					if (nameText != null)
					{
						if (nameText.Length > 25)
						{
							selected.World.Name = nameText.Substring(0, 25);
						}
						else
						{
							selected.World.Name = nameText;
						}
						selected.World.SaveToStorage(Screen.CurrentGamer, CastleMinerZGame.Instance.SaveDevice);
						this._resetSortButtonText();
					}
				}
			});
		}

		public ButtonControl NewWorldButton
		{
			set
			{
				if (this._newWorldButton != null)
				{
					base.Controls.Remove(this._renameButton);
				}
				this._newWorldButton = value;
				this._newWorldButton.Pressed += this._newWorldButton_Pressed;
				base.Controls.Add(this._newWorldButton);
			}
		}

		private void _newWorldButton_Pressed(object sender, EventArgs e)
		{
			if (this.ClickSound != null)
			{
				SoundManager.Instance.PlayInstance(this.ClickSound);
			}
			this._game.FrontEnd.startWorld();
		}

		public ChooseSavedWorldScreen()
			: base(false, new Size(700, 60), new Rectangle(10, 60, Screen.Adjuster.ScreenRect.Width - 10, Screen.Adjuster.ScreenRect.Height - 60))
		{
			this.ClickSound = "Click";
			Color transpGreen = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			this._deleteStorageDialog = new PCDialogScreen(Strings.Erase_Storage, Strings.Are_you_sure_you_want_to_delete_everything_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._deleteStorageDialog.UseDefaultValues();
			this._eraseSaves.Size = new Size(300, 40);
			this._eraseSaves.Text = Strings.Erase_Storage;
			this._eraseSaves.Font = this._game._medFont;
			this._eraseSaves.Frame = this._game.ButtonFrame;
			this._eraseSaves.Pressed += this._eraseSaves_Pressed;
			this._eraseSaves.ButtonColor = transpGreen;
			base.Controls.Add(this._eraseSaves);
			base.SelectButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 170),
				Size = new Size(300, 40),
				Text = Strings.Start_Game,
				Font = this._game._medFont,
				Frame = this._game.ButtonFrame,
				ButtonColor = transpGreen
			};
			this.DeleteButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 215),
				Size = new Size(300, 40),
				Text = Strings.Delete_World,
				Font = this._game._medFont,
				Frame = this._game.ButtonFrame,
				ButtonColor = transpGreen
			};
			this.RenameButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 260),
				Size = new Size(300, 40),
				Text = Strings.Rename_World,
				Font = this._game._medFont,
				Frame = this._game.ButtonFrame,
				ButtonColor = transpGreen
			};
			this.NewWorldButton = new FrameButtonControl
			{
				LocalPosition = new Point(900, 125),
				Size = new Size(300, 40),
				Text = Strings.New_World,
				Font = this._game._medFont,
				Frame = this._game.ButtonFrame,
				ButtonColor = transpGreen
			};
			base.BackButton = new ImageButtonControl
			{
				Image = this._game._uiSprites["BackArrow"],
				Font = this._game._medFont,
				LocalPosition = new Point(15, 15),
				Text = " " + Strings.Back,
				ImageDefaultColor = transpGreen
			};
			Point loc = new Point(40, 100);
			this._nameButton.LocalPosition = loc;
			this._nameButton.Size = new Size(217, 18);
			this._nameButton.Text = "SERVER NAME";
			this._nameButton.Font = this._game._smallFont;
			this._nameButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._nameButton.Frame = this._game.ButtonFrame;
			this._nameButton.Pressed += this._nameButton_Pressed;
			base.Controls.Add(this._nameButton);
			loc.X += this._nameButton.Size.Width + 1;
			this._dateButton.LocalPosition = loc;
			this._dateButton.Size = new Size(160, 18);
			this._dateButton.Text = "DATE \u02c5";
			this._dateButton.Font = this._game._smallFont;
			this._dateButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._dateButton.Frame = this._game.ButtonFrame;
			this._dateButton.Pressed += this._dateButton_Pressed;
			base.Controls.Add(this._dateButton);
			loc.X += this._dateButton.Size.Width + 1;
			this._creatorButton.LocalPosition = loc;
			this._creatorButton.Size = new Size(160, 18);
			this._creatorButton.Text = "CREATED BY";
			this._creatorButton.Font = this._game._smallFont;
			this._creatorButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._creatorButton.Frame = this._game.ButtonFrame;
			this._creatorButton.Pressed += this._creatorButton_Pressed;
			base.Controls.Add(this._creatorButton);
			loc.X += this._creatorButton.Size.Width + 1;
			this._ownerButton.LocalPosition = loc;
			this._ownerButton.Size = new Size(160, 18);
			this._ownerButton.Text = "HOST";
			this._ownerButton.Font = this._game._smallFont;
			this._ownerButton.TextAlignment = FrameButtonControl.Alignment.Left;
			this._ownerButton.Frame = this._game.ButtonFrame;
			this._ownerButton.Pressed += this._ownerButton_Pressed;
			base.Controls.Add(this._ownerButton);
			this._deleteWorldDialog = new PCDialogScreen(Strings.Delete_World, Strings.Are_you_sure_you_want_to_delete_this_world_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._deleteWorldDialog.UseDefaultValues();
			this._takeOverTerrain = new PCDialogScreen(Strings.Take_Over_World, Strings.This_world_belongs_to_someone_else_Would_you_like_to_make_your_own_copy_You_will_be_able_to_make_changes_locally_and_host_it_yourself, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._takeOverTerrain.UseDefaultValues();
			this._infiniteModeConversion = new PCDialogScreen(Strings.Creative_Mode, Strings.Are_you_sure_you_want_to_play_this_world_in_Creative_Mode__You_will_not_be_able_to_load_it_in_normal_mode_again_, null, true, this._game.DialogScreenImage, this._game._myriadMed, true, this._game.ButtonFrame);
			this._infiniteModeConversion.UseDefaultValues();
			this._keyboardInputScreen = new PCKeyboardInputScreen(CastleMinerZGame.Instance, Strings.Rename, Strings.Enter_A_New_Name, CastleMinerZGame.Instance.DialogScreenImage, CastleMinerZGame.Instance._myriadMed, true, this._game.ButtonFrame);
			this._keyboardInputScreen.ClickSound = "Click";
			this._keyboardInputScreen.OpenSound = "Popup";
		}

		private void _ownerButton_Pressed(object sender, EventArgs e)
		{
			this._sortByOwner(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _creatorButton_Pressed(object sender, EventArgs e)
		{
			this._sortByCreator(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _eraseSaves_Pressed(object sender, EventArgs e)
		{
			this._game.FrontEnd._uiGroup.ShowPCDialogScreen(this._deleteStorageDialog, delegate
			{
				if (this._deleteStorageDialog.OptionSelected != -1)
				{
					WaitScreen.DoWait(this._game.FrontEnd._uiGroup, Strings.Deleting_Storage___, delegate
					{
						this._game.SaveDevice.DeleteStorage();
					}, null);
					this._game.FrontEnd.PopToStartScreen();
				}
			});
		}

		public void Populate()
		{
			WorldInfo[] worlds = this.WorldManager.GetWorlds();
			List<ListItemControl> items = new List<ListItemControl>();
			foreach (WorldInfo info in worlds)
			{
				if (info.InfiniteResourceMode == this._game.InfiniteResourceMode || this._game.InfiniteResourceMode)
				{
					items.Add(new ChooseSavedWorldScreen.SavedWorldItem(info, this._itemSize));
				}
			}
			switch (this._currentSort)
			{
			case ChooseSavedWorldScreen.SortBy.DateAsc:
				this._sortByDate(ChooseSavedWorldScreen.SortBy.DateDesc, items);
				break;
			case ChooseSavedWorldScreen.SortBy.DateDesc:
				this._sortByDate(ChooseSavedWorldScreen.SortBy.DateAsc, items);
				break;
			case ChooseSavedWorldScreen.SortBy.NameAsc:
				this._sortByName(ChooseSavedWorldScreen.SortBy.NameDesc, items);
				break;
			case ChooseSavedWorldScreen.SortBy.NameDesc:
				this._sortByName(ChooseSavedWorldScreen.SortBy.NameAsc, items);
				break;
			case ChooseSavedWorldScreen.SortBy.CreatorAsc:
				this._sortByCreator(ChooseSavedWorldScreen.SortBy.CreatorDesc, items);
				break;
			case ChooseSavedWorldScreen.SortBy.CreatorDesc:
				this._sortByCreator(ChooseSavedWorldScreen.SortBy.CreatorAsc, items);
				break;
			case ChooseSavedWorldScreen.SortBy.OwnerAsc:
				this._sortByOwner(ChooseSavedWorldScreen.SortBy.OwnerDesc, items);
				break;
			case ChooseSavedWorldScreen.SortBy.OwnerDesc:
				this._sortByOwner(ChooseSavedWorldScreen.SortBy.OwnerAsc, items);
				break;
			}
			this.Items = items;
			if (this.Items.Count == 0)
			{
				this._creatorButton.Visible = (this._nameButton.Visible = (this._dateButton.Visible = (this._ownerButton.Visible = (this._deleteButton.Visible = (this._renameButton.Visible = (this._eraseSaves.Visible = false))))));
			}
			else
			{
				this._creatorButton.Visible = (this._nameButton.Visible = (this._dateButton.Visible = (this._ownerButton.Visible = (this._deleteButton.Visible = (this._renameButton.Visible = (this._eraseSaves.Visible = true))))));
			}
			base.OnPushed();
		}

		private void _resetSortButtonText()
		{
			this._nameButton.Text = "SERVER NAME";
			this._dateButton.Text = "DATE";
			this._creatorButton.Text = "CREATED BY";
			this._ownerButton.Text = "HOST";
		}

		private void _dateButton_Pressed(object sender, EventArgs e)
		{
			this._sortByDate(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _sortByDate(ChooseSavedWorldScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseSavedWorldScreen.SortBy.DateDesc)
			{
				this._dateButton.Text = "DATE \u02c4";
				this._currentSort = ChooseSavedWorldScreen.SortBy.DateAsc;
				items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByDate));
				return;
			}
			this._dateButton.Text = "DATE \u02c5";
			this._currentSort = ChooseSavedWorldScreen.SortBy.DateDesc;
			items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByDate));
			items.Reverse();
		}

		private void _sortByCreator(ChooseSavedWorldScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseSavedWorldScreen.SortBy.CreatorDesc)
			{
				this._creatorButton.Text = "CREATED BY \u02c4";
				this._currentSort = ChooseSavedWorldScreen.SortBy.CreatorAsc;
				items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByCreator));
				return;
			}
			this._creatorButton.Text = "CREATED BY \u02c5";
			this._currentSort = ChooseSavedWorldScreen.SortBy.CreatorDesc;
			items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByCreator));
			items.Reverse();
		}

		private void _sortByOwner(ChooseSavedWorldScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseSavedWorldScreen.SortBy.OwnerDesc)
			{
				this._ownerButton.Text = "HOST \u02c4";
				this._currentSort = ChooseSavedWorldScreen.SortBy.OwnerAsc;
				items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByOwner));
				return;
			}
			this._ownerButton.Text = "HOST \u02c5";
			this._currentSort = ChooseSavedWorldScreen.SortBy.OwnerDesc;
			items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByOwner));
			items.Reverse();
		}

		private void _nameButton_Pressed(object sender, EventArgs e)
		{
			this._sortByName(this._currentSort, this.Items);
			base._updateControlsOnSort();
		}

		private void _sortByName(ChooseSavedWorldScreen.SortBy sort, List<ListItemControl> items)
		{
			this._resetSortButtonText();
			if (sort == ChooseSavedWorldScreen.SortBy.NameAsc)
			{
				this._nameButton.Text = "SERVER NAME \u02c5";
				this._currentSort = ChooseSavedWorldScreen.SortBy.NameDesc;
				items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByName));
				items.Reverse();
				return;
			}
			this._nameButton.Text = "SERVER NAME \u02c4";
			this._currentSort = ChooseSavedWorldScreen.SortBy.NameAsc;
			items.Sort(new Comparison<ListItemControl>(ChooseSavedWorldScreen.SortByName));
		}

		public static int SortByName(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			else
			{
				if (b == null)
				{
					return 1;
				}
				ChooseSavedWorldScreen.SavedWorldItem one = (ChooseSavedWorldScreen.SavedWorldItem)a;
				ChooseSavedWorldScreen.SavedWorldItem two = (ChooseSavedWorldScreen.SavedWorldItem)b;
				return string.Compare(one.World.Name, two.World.Name, true);
			}
		}

		public static int SortByCreator(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			else
			{
				if (b == null)
				{
					return 1;
				}
				ChooseSavedWorldScreen.SavedWorldItem one = (ChooseSavedWorldScreen.SavedWorldItem)a;
				ChooseSavedWorldScreen.SavedWorldItem two = (ChooseSavedWorldScreen.SavedWorldItem)b;
				return string.Compare(one.World.CreatorGamerTag, two.World.CreatorGamerTag, true);
			}
		}

		public static int SortByOwner(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			else
			{
				if (b == null)
				{
					return 1;
				}
				ChooseSavedWorldScreen.SavedWorldItem one = (ChooseSavedWorldScreen.SavedWorldItem)a;
				ChooseSavedWorldScreen.SavedWorldItem two = (ChooseSavedWorldScreen.SavedWorldItem)b;
				if (one.World.OwnerGamerTag == Screen.CurrentGamer.Gamertag)
				{
					if (two.World.OwnerGamerTag == Screen.CurrentGamer.Gamertag)
					{
						return 0;
					}
					return 1;
				}
				else
				{
					if (two.World.OwnerGamerTag == Screen.CurrentGamer.Gamertag)
					{
						return -1;
					}
					return string.Compare(one.World.OwnerGamerTag, two.World.OwnerGamerTag, true);
				}
			}
		}

		public static int SortByDate(ListItemControl a, ListItemControl b)
		{
			if (a == null)
			{
				if (b == null)
				{
					return 0;
				}
				return -1;
			}
			else
			{
				if (b == null)
				{
					return 1;
				}
				ChooseSavedWorldScreen.SavedWorldItem one = (ChooseSavedWorldScreen.SavedWorldItem)a;
				ChooseSavedWorldScreen.SavedWorldItem two = (ChooseSavedWorldScreen.SavedWorldItem)b;
				return DateTime.Compare(one.World.LastPlayedDate, two.World.LastPlayedDate);
			}
		}

		public override void OnPushed()
		{
			this.Populate();
			base.OnPushed();
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (Screen.Adjuster.ScreenRect != this.prevScreenRect)
			{
				this.prevScreenRect = Screen.Adjuster.ScreenRect;
				int areaWidth = (int)(540f * Screen.Adjuster.ScaleFactor.X);
				this._selectButton.Scale = (this._deleteButton.Scale = (this._newWorldButton.Scale = (this._eraseSaves.Scale = (this._renameButton.Scale = (this._nameButton.Scale = (this._dateButton.Scale = (this._creatorButton.Scale = (this._ownerButton.Scale = Screen.Adjuster.ScaleFactor.X))))))));
				Point loc = new Point(40, 100);
				this._nameButton.LocalPosition = loc;
				loc.X += this._nameButton.Size.Width + 1;
				this._dateButton.LocalPosition = loc;
				loc.X += this._dateButton.Size.Width + 1;
				this._creatorButton.LocalPosition = loc;
				loc.X += this._creatorButton.Size.Width + 1;
				this._ownerButton.LocalPosition = loc;
				int xLoc = (int)(740f * Screen.Adjuster.ScaleFactor.X) + areaWidth / 2 - this._deleteButton.Size.Width / 2;
				int scaledHeight = this._selectButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				this._selectButton.LocalPosition = new Point(xLoc, this._selectButton.LocalPosition.Y);
				this._newWorldButton.LocalPosition = new Point(xLoc, this._selectButton.LocalPosition.Y + scaledHeight);
				this._deleteButton.LocalPosition = new Point(xLoc, this._newWorldButton.LocalPosition.Y + scaledHeight);
				this._renameButton.LocalPosition = new Point(xLoc, this._deleteButton.LocalPosition.Y + scaledHeight);
				this._eraseSaves.LocalPosition = new Point(xLoc, this._renameButton.LocalPosition.Y + scaledHeight);
				this._itemSize.Width = (int)(700f * Screen.Adjuster.ScaleFactor.X);
				this._itemSize.Height = (int)(60f * Screen.Adjuster.ScaleFactor.X);
				int yloc = this._nameButton.LocalPosition.Y + this._nameButton.Size.Height + (int)(5f * Screen.Adjuster.ScaleFactor.X);
				this._drawArea = new Rectangle((int)(10f * Screen.Adjuster.ScaleFactor.X), yloc, (int)((float)Screen.Adjuster.ScreenRect.Width - 10f * Screen.Adjuster.ScaleFactor.X), Screen.Adjuster.ScreenRect.Height - yloc);
				for (int i = 0; i < this.Items.Count; i++)
				{
					this.Items[i].Size = this._itemSize;
				}
				base._updateControlsOnSort();
			}
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont font = this._game._largeFont;
			spriteBatch.Begin();
			if (this.Items.Count == 0)
			{
				string msg = "No Saved Worlds";
				Vector2 size = font.MeasureString(msg);
				int lineSpacing = font.LineSpacing;
				spriteBatch.DrawOutlinedText(font, msg, new Vector2(75f * Screen.Adjuster.ScaleFactor.X, 170f), CMZColors.MenuGreen, Color.Black, 2);
			}
			else
			{
				string msg = Strings.Choose_A_Server;
				Vector2 size = font.MeasureString(msg);
				int lineSpacing2 = font.LineSpacing;
				spriteBatch.DrawOutlinedText(font, msg, new Vector2((float)(Screen.Adjuster.ScreenRect.Width / 2) - size.X / 2f, 10f), CMZColors.MenuGreen, Color.Black, 1);
				ChooseSavedWorldScreen.SavedWorldItem selected = (ChooseSavedWorldScreen.SavedWorldItem)base.SelectedItem;
				selected.DrawSelectedInfo(spriteBatch, new Vector2((float)this._eraseSaves.LocalPosition.X, (float)(this._eraseSaves.LocalPosition.Y + this._eraseSaves.Size.Height + 5)));
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private CastleMinerZGame _game = CastleMinerZGame.Instance;

		private FrameButtonControl _eraseSaves = new FrameButtonControl();

		private PCDialogScreen _deleteStorageDialog;

		public PCDialogScreen _takeOverTerrain;

		public PCDialogScreen _infiniteModeConversion;

		private PCDialogScreen _deleteWorldDialog;

		private ChooseSavedWorldScreen.SortBy _currentSort = ChooseSavedWorldScreen.SortBy.DateDesc;

		private FrameButtonControl _dateButton = new FrameButtonControl();

		private FrameButtonControl _creatorButton = new FrameButtonControl();

		private FrameButtonControl _nameButton = new FrameButtonControl();

		private FrameButtonControl _ownerButton = new FrameButtonControl();

		private PCKeyboardInputScreen _keyboardInputScreen;

		private ButtonControl _deleteButton;

		private ButtonControl _renameButton;

		private ButtonControl _newWorldButton;

		private Rectangle prevScreenRect;

		private enum SortBy
		{
			DateAsc,
			DateDesc,
			NameAsc,
			NameDesc,
			CreatorAsc,
			CreatorDesc,
			OwnerAsc,
			OwnerDesc
		}

		public class SavedWorldItem : ListItemControl
		{
			public SavedWorldItem(WorldInfo world, Size itemSize)
				: base(itemSize)
			{
				this._lastPlayedDate = world.LastPlayedDate.ToString();
				this.World = world;
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
				spriteBatch.DrawString(this._medFont, this.World.Name, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				int hostSize = (int)(215f * Screen.Adjuster.ScaleFactor.X);
				loc.X += (float)hostSize;
				spriteBatch.DrawString(this._smallFont, this._lastPlayedDate, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.X += (float)((int)(161f * Screen.Adjuster.ScaleFactor.X));
				spriteBatch.DrawString(this._smallFont, this.World.CreatorGamerTag, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				loc.X += (float)((int)(161f * Screen.Adjuster.ScaleFactor.X));
				if ((float)this.Size.Width > loc.X)
				{
					spriteBatch.DrawString(this._smallFont, this.World.OwnerGamerTag, loc, color, 0f, Vector2.Zero, Screen.Adjuster.ScaleFactor.X, SpriteEffects.None, 0f);
				}
			}

			public void DrawSelectedInfo(SpriteBatch spriteBatch, Vector2 loc)
			{
				spriteBatch.DrawOutlinedText(this._medFont, this.World.Name, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, Strings.Created_By + ": " + this.World.CreatorGamerTag, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, Strings.Last_Played + ": " + this._lastPlayedDate, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
				loc.Y += (float)this._medFont.LineSpacing * Screen.Adjuster.ScaleFactor.X;
				spriteBatch.DrawOutlinedText(this._medFont, Strings.Hosted_By + ": " + this.World.OwnerGamerTag, loc, Color.White, Color.Black, 1, Screen.Adjuster.ScaleFactor.X, 0f, Vector2.Zero);
			}

			public WorldInfo World;

			private SpriteFont _largeFont = CastleMinerZGame.Instance._medLargeFont;

			private SpriteFont _medFont = CastleMinerZGame.Instance._medFont;

			private SpriteFont _smallFont = CastleMinerZGame.Instance._smallFont;

			private string _lastPlayedDate;
		}
	}
}
