using System;
using System.Collections.Generic;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using DNA.Net.GamerServices;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class SelectPlayerScreen : UIControlScreen
	{
		private SelectPlayerScreen(CastleMinerZGame game, bool showME, bool drawBehind, SelectPlayerCallback callback)
			: base(drawBehind)
		{
			this._showMe = showME;
			this.font = game._medFont;
			this._game = game;
			this._callback = callback;
			ImageButtonControl imageButtonControl = new ImageButtonControl();
			imageButtonControl.Image = this._game._uiSprites["BackArrow"];
			imageButtonControl.Font = this._game._medFont;
			imageButtonControl.LocalPosition = new Point(32, 32);
			imageButtonControl.Pressed += this._backButton_Pressed;
			imageButtonControl.ImageDefaultColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			imageButtonControl.Text = " " + Strings.Back;
			base.Controls.Add(imageButtonControl);
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			base.PopMe();
		}

		private int _hitTest(Point p)
		{
			for (int i = 0; i < this._itemLocations.Length; i++)
			{
				if (this._itemLocations[i].Contains(p))
				{
					this._lastHitTestResult = i;
					return i;
				}
			}
			this._lastHitTestResult = -1;
			return -1;
		}

		public static void SelectPlayer(CastleMinerZGame game, ScreenGroup group, bool showME, bool drawBehind, SelectPlayerCallback callback)
		{
			SelectPlayerScreen selectPlayerScreen = new SelectPlayerScreen(game, showME, drawBehind, callback);
			group.PushScreen(selectPlayerScreen);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (this._itemLocations.Length != this.activeGamers.Count)
			{
				this._itemLocations = new Rectangle[this.activeGamers.Count];
			}
			this.flashTimer.Update(gameTime.ElapsedGameTime);
			if (this.flashTimer.Expired)
			{
				this.flashTimer.Reset();
				this.flashDir = !this.flashDir;
			}
			Rectangle rectangle = new Rectangle(25, 25, Screen.Adjuster.ScreenRect.Width - 50, Screen.Adjuster.ScreenRect.Height - 50);
			spriteBatch.Begin();
			int num = this.activeGamers.Count + (this._showMe ? 0 : 1);
			int maxGamers = this._game.CurrentNetworkSession.MaxGamers;
			this._builder.Length = 0;
			this._builder.Append(Strings.Players + " ").Concat(num).Append("/")
				.Concat(maxGamers);
			Vector2 vector = this.font.MeasureString(this._builder);
			spriteBatch.DrawOutlinedText(this.font, this._builder, new Vector2((float)rectangle.Right - vector.X, (float)rectangle.Bottom - vector.Y), Color.White, Color.Black, 2);
			float[] array = new float[1];
			float num2 = 0f;
			num2 += (array[0] = this.font.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float num3 = ((float)Screen.Adjuster.ScreenRect.Width - num2) / 2f + 2f;
			float num4 = (float)rectangle.Top;
			spriteBatch.DrawOutlinedText(this.font, Strings.Player, new Vector2(num3, num4), Color.Orange, Color.Black, 2);
			num4 += (float)(this.font.LineSpacing + 10);
			for (int i = 0; i < this.activeGamers.Count; i++)
			{
				NetworkGamer networkGamer = this.activeGamers[i];
				if (networkGamer.Tag != null)
				{
					Player player = (Player)networkGamer.Tag;
					if (i == this._selectedIndex)
					{
						Color color = Color.Black;
						if (this._lastHitTestResult == i)
						{
							color = Color.Gray;
						}
						spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle((int)num3 - 2, (int)num4, (int)num2 + 2, this.font.LineSpacing + 4), color);
					}
					this.font.MeasureString(player.Gamer.Gamertag);
					this._itemLocations[i] = new Rectangle((int)num3 - 2, (int)num4, (int)num2 + 2, this.font.LineSpacing + 4);
					spriteBatch.DrawOutlinedText(this.font, player.Gamer.Gamertag, new Vector2(num3, num4 + 2f), player.Gamer.IsLocal ? Color.Red : Color.White, Color.Black, 2);
					if (player.Profile != null)
					{
						float num5 = (float)this.font.LineSpacing * 0.9f;
						float num6 = (float)this.font.LineSpacing - num5;
						if (player.GamerPicture != null)
						{
							spriteBatch.Draw(player.GamerPicture, new Rectangle((int)(num3 - (float)this.font.LineSpacing), (int)(num4 + num6), (int)num5, (int)num5), Color.White);
						}
					}
					num4 += (float)(this.font.LineSpacing + 4);
					if (i == 15)
					{
						num4 = (float)(this.font.LineSpacing + 10 + rectangle.Top);
						num3 += num2 + 10f;
					}
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			this._setActiveGamers();
			base.OnUpdate(game, gameTime);
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			int num = this._hitTest(inputManager.Mouse.Position);
			if (num >= 0 && this._selectedIndex != num)
			{
				SoundManager.Instance.PlayInstance("Click");
				this._selectedIndex = num;
			}
			if (controller.PressedDPad.Down || (controller.CurrentState.ThumbSticks.Left.Y < -0.2f && controller.LastState.ThumbSticks.Left.Y >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Down))
			{
				this.waitScrollTimer.Reset();
				this.autoScrollTimer.Reset();
				if (this.SelectDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			if (controller.PressedDPad.Up || (controller.CurrentState.ThumbSticks.Left.Y > 0.2f && controller.LastState.ThumbSticks.Left.Y <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Up))
			{
				this.waitScrollTimer.Reset();
				this.autoScrollTimer.Reset();
				if (this.SelectUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			this.waitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (controller.PressedButtons.A || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || (inputManager.Mouse.LeftButtonPressed && num >= 0))
			{
				base.PopMe();
				if (this._callback != null)
				{
					this._callback(this.PlayerSelected);
				}
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				this.PlayerSelected = null;
				base.PopMe();
				if (this._callback != null)
				{
					this._callback(null);
				}
			}
			if (this.waitScrollTimer.Expired)
			{
				if (controller.CurrentState.ThumbSticks.Left.Y < -0.2f)
				{
					this.autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.autoScrollTimer.Expired)
					{
						this.autoScrollTimer.Reset();
						if (this.SelectDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.Y > 0.2f)
				{
					this.autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.autoScrollTimer.Expired)
					{
						this.autoScrollTimer.Reset();
						if (this.SelectUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			if (this._selectedIndex <= 0)
			{
				this._selectedIndex = 0;
			}
			if (this._selectedIndex >= this.activeGamers.Count)
			{
				this._selectedIndex = this.activeGamers.Count - 1;
			}
			this.SetSelection();
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		private void SetSelection()
		{
			this.PlayerSelected = null;
			int num = 0;
			for (int i = 0; i < this._game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = this._game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null && (this._showMe || networkGamer != this._game.MyNetworkGamer))
				{
					if (num == this._selectedIndex)
					{
						this.PlayerSelected = (Player)networkGamer.Tag;
					}
					num++;
				}
			}
		}

		private void _setActiveGamers()
		{
			this.activeGamers.Clear();
			for (int i = 0; i < this._game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = this._game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null && (this._showMe || networkGamer != this._game.MyNetworkGamer))
				{
					this.activeGamers.Add(networkGamer);
				}
			}
		}

		private bool SelectDown()
		{
			this._selectedIndex++;
			if (this._selectedIndex >= this.activeGamers.Count)
			{
				this._selectedIndex = this.activeGamers.Count - 1;
				return false;
			}
			return true;
		}

		private bool SelectUp()
		{
			this._selectedIndex--;
			if (this._selectedIndex <= 0)
			{
				this._selectedIndex = 0;
				return false;
			}
			return true;
		}

		public Player PlayerSelected;

		public SelectPlayerCallback _callback;

		private SpriteFont font;

		private int _selectedIndex;

		private bool _showMe;

		private CastleMinerZGame _game;

		private Rectangle[] _itemLocations = new Rectangle[0];

		private int _lastHitTestResult = -1;

		private OneShotTimer flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private bool flashDir;

		private StringBuilder _builder = new StringBuilder();

		private List<NetworkGamer> activeGamers = new List<NetworkGamer>();

		private OneShotTimer waitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer autoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));
	}
}
