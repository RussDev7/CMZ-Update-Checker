using System;
using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class ControlsTab : TabControl.TabPage
	{
		public ControlsTab(bool InGame, ScreenGroup uiGroup)
			: base(Strings.Controls)
		{
			this._game = CastleMinerZGame.Instance;
			this._controlsFont = this._game._smallFont;
			this._menuButtonFont = this._game._medFont;
			this.inGame = InGame;
			this._uiGroup = uiGroup;
			Color btnColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			this._defaultButton.LocalPosition = new Point(690, 595);
			this._defaultButton.Size = new Size(225, this._menuButtonFont.LineSpacing);
			this._defaultButton.Text = Strings.Reset_To_Default;
			this._defaultButton.Font = this._menuButtonFont;
			this._defaultButton.Frame = this._game.ButtonFrame;
			this._defaultButton.Pressed += this._defaultButton_Pressed;
			this._defaultButton.ButtonColor = btnColor;
			base.Children.Add(this._defaultButton);
			this.invertYButton.LocalPosition = new Point(310, 595);
			this.invertYButton.Size = new Size(135, this._controlsFont.LineSpacing);
			this.invertYButton.Text = (this._game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			this.invertYButton.Font = this._controlsFont;
			this.invertYButton.Frame = this._game.ButtonFrame;
			this.invertYButton.Pressed += this.invertYButton_Pressed;
			this.invertYButton.ButtonColor = btnColor;
			base.Children.Add(this.invertYButton);
			this._buttons = new ControlsTab.BindingScreenButtonControl[this._buttonOrder.Length - 1];
			this._labels = new TextControl[this._buttonOrder.Length - 1];
			int currentButton = 0;
			for (int i = 0; i < this._buttonOrder.Length; i++)
			{
				if (this._buttonLabels[i] != null)
				{
					ControlsTab.BindingScreenButtonControl btn = new ControlsTab.BindingScreenButtonControl(this._buttonOrder[i]);
					btn.Size = new Size(135, this._controlsFont.LineSpacing);
					btn.Text = InputBinding.KeyString(this._binding.GetBinding((int)this._buttonOrder[i], InputBinding.Slot.KeyMouse1));
					btn.Font = this._controlsFont;
					btn.Frame = this._game.ButtonFrame;
					btn.Pressed += this._bindingBtn_Pressed;
					btn.ButtonColor = btnColor;
					base.Children.Add(btn);
					this._buttons[currentButton] = btn;
					TextControl text = new TextControl(this._buttonLabels[i], this._controlsFont);
					base.Children.Add(text);
					this._labels[currentButton] = text;
					currentButton++;
				}
			}
			this._esc = new TextControl(this._controlsFont, "Esc", Point.Zero, CMZColors.MenuGreen);
			this._opensMenu = new TextControl(Strings.Opens_the_menu__Pauses_offline_games, this._controlsFont);
			this._invertY = new TextControl(Strings.Invert_Y_Axis + ":", this._controlsFont);
			this._sensitivityLabel = new TextControl(Strings.Controller_Sensitivity, this._controlsFont);
			this._pressToRebind = new TextControl(Strings.Press_a_button_to_rebind_keys, this._menuButtonFont);
			base.Children.Add(this._esc);
			base.Children.Add(this._opensMenu);
			base.Children.Add(this._invertY);
			base.Children.Add(this._sensitivityLabel);
			base.Children.Add(this._pressToRebind);
			this._sensitivityControl.Size = new Size(185, this._controlsFont.LineSpacing);
			this._sensitivityControl.MinValue = 0;
			this._sensitivityControl.MaxValue = 100;
			this._sensitivityControl.FillColor = CMZColors.MenuGreen;
			base.Children.Add(this._sensitivityControl);
		}

		private void _bindingBtn_Pressed(object sender, EventArgs e)
		{
			ControlsTab.BindingScreenButtonControl ctrl = sender as ControlsTab.BindingScreenButtonControl;
			if (ctrl != null)
			{
				this._uiGroup.PushScreen(new ControlsTab.SelectButtonDialog(this._controlsFont, ctrl.Function, ctrl, this));
			}
		}

		private void _defaultButton_Pressed(object sender, EventArgs e)
		{
			this._game._controllerMapping.SetToDefault();
			this.ResetAllButtonText();
		}

		private void invertYButton_Pressed(object sender, EventArgs e)
		{
			this._game.PlayerStats.InvertYAxis = !this._game.PlayerStats.InvertYAxis;
			this.invertYButton.Text = (this._game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
		}

		public void ResetAllButtonText()
		{
			for (int i = 0; i < this._buttons.Length; i++)
			{
				this._buttons[i].Text = InputBinding.KeyString(this._binding.GetBinding((int)this._buttons[i].Function, InputBinding.Slot.KeyMouse1));
			}
		}

		public override void OnSelected()
		{
			if (!this._binding.Initialized)
			{
				CastleMinerZGame.Instance._controllerMapping.SetToDefault();
			}
			this.invertYButton.Text = (this._game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			this._sensitivityControl.Value = (int)(100f * (this._game.PlayerStats.controllerSensitivity - 0.01f));
			this.ResetAllButtonText();
			base.OnSelected();
		}

		public override void OnLostFocus()
		{
			try
			{
				this._game.SavePlayerStats(this._game.PlayerStats);
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
				this._game.PlayerStats.controllerSensitivity = 0.01f + (float)this._sensitivityControl.Value / 100f;
			}
			if (this._prevScreenSize != Screen.Adjuster.ScreenRect)
			{
				this._prevScreenSize = Screen.Adjuster.ScreenRect;
				int buttonHeight = 40;
				if (this._buttons.Length > 1)
				{
					buttonHeight = (int)((float)this._buttons[0].Size.Height * 1.3f);
				}
				int height = (int)((float)buttonHeight * Screen.Adjuster.ScaleFactor.Y);
				Point loc = new Point(0, (int)(55f * Screen.Adjuster.ScaleFactor.Y));
				int btnOffset = (int)(300f * Screen.Adjuster.ScaleFactor.Y);
				int btn = 0;
				this._pressToRebind.LocalPosition = new Point((int)(300f * Screen.Adjuster.ScaleFactor.Y), this._pressToRebind.LocalPosition.Y);
				for (int i = 0; i < this._buttonLabels.Length; i++)
				{
					if (this._buttonLabels[i] == null)
					{
						loc = new Point((int)(475f * Screen.Adjuster.ScaleFactor.Y), this._buttons[0].LocalPosition.Y);
					}
					else
					{
						this._buttons[btn].LocalPosition = new Point(loc.X + btnOffset, loc.Y);
						this._labels[btn].LocalPosition = loc;
						this._labels[btn].Scale = (this._buttons[btn].Scale = Screen.Adjuster.ScaleFactor.Y);
						btn++;
						loc.Y += height;
					}
				}
				this._esc.Scale = (this._opensMenu.Scale = (this._sensitivityLabel.Scale = (this._invertY.Scale = (this._defaultButton.Scale = (this.invertYButton.Scale = (this._pressToRebind.Scale = Screen.Adjuster.ScaleFactor.Y))))));
				this._esc.LocalPosition = new Point((int)((double)loc.X + (double)btnOffset * 1.2), loc.Y);
				this._opensMenu.LocalPosition = loc;
				loc.Y += height;
				this.invertYButton.LocalPosition = new Point(loc.X + btnOffset, loc.Y);
				this._invertY.LocalPosition = loc;
				loc.Y += height;
				this._sensitivityControl.LocalPosition = new Point((int)((double)loc.X + (double)btnOffset * 0.85), loc.Y + (int)(10f * Screen.Adjuster.ScaleFactor.Y));
				this._sensitivityControl.Size = new Size((int)(185f * Screen.Adjuster.ScaleFactor.Y), height);
				this._sensitivityLabel.LocalPosition = new Point(loc.X, loc.Y);
				loc.Y += height;
				this._defaultButton.LocalPosition = new Point((int)(140f * Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40f * Screen.Adjuster.ScaleFactor.Y));
			}
			base.OnUpdate(game, gameTime);
		}

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private SpriteFont _menuButtonFont;

		private InputBinding _binding = CastleMinerZGame.Instance._controllerMapping.Binding;

		private bool inGame;

		private CastleMinerZControllerMapping.CMZControllerFunctions[] _buttonOrder = new CastleMinerZControllerMapping.CMZControllerFunctions[]
		{
			CastleMinerZControllerMapping.CMZControllerFunctions.Use,
			CastleMinerZControllerMapping.CMZControllerFunctions.MoveForward,
			CastleMinerZControllerMapping.CMZControllerFunctions.MoveBackward,
			CastleMinerZControllerMapping.CMZControllerFunctions.StrafeLeft,
			CastleMinerZControllerMapping.CMZControllerFunctions.StrafeRight,
			CastleMinerZControllerMapping.CMZControllerFunctions.Sprint,
			CastleMinerZControllerMapping.CMZControllerFunctions.Jump,
			CastleMinerZControllerMapping.CMZControllerFunctions.Reload,
			CastleMinerZControllerMapping.CMZControllerFunctions.PlayersScreen,
			CastleMinerZControllerMapping.CMZControllerFunctions.SwitchTray,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem1,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem2,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem3,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem4,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem5,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem6,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem7,
			CastleMinerZControllerMapping.CMZControllerFunctions.UseItem8,
			CastleMinerZControllerMapping.CMZControllerFunctions.Count,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder_Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder,
			CastleMinerZControllerMapping.CMZControllerFunctions.Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.NextItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.PreviousItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.BlockUI,
			CastleMinerZControllerMapping.CMZControllerFunctions.DropQuickBarItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.FlyMode,
			CastleMinerZControllerMapping.CMZControllerFunctions.TextChat
		};

		private string[] _buttonLabels = new string[]
		{
			Strings.Use_Shoot,
			Strings.Move_forward,
			Strings.Move_backward,
			Strings.Strafe_left,
			Strings.Strafe_right,
			Strings.Sprint,
			Strings.Jump,
			Strings.Reload,
			Strings.Show_Player_List,
			Strings.Switch_Tray,
			Strings.Slot_1,
			Strings.Slot_2,
			Strings.Slot_3,
			Strings.Slot_4,
			Strings.Slot_5,
			Strings.Slot_6,
			Strings.Slot_7,
			Strings.Slot_8,
			null,
			Strings.Activate_Shoulder,
			Strings.Shoulder,
			Strings.Activate,
			Strings.Next_Item,
			Strings.Previous_Item,
			Strings.Opens_your_inventory,
			Strings.Drop_an_item_from_the_quick_bar,
			Strings.Fly_Mode,
			Strings.Show_Chat
		};

		private TextControl _esc;

		private TextControl _opensMenu;

		private TextControl _invertY;

		private TextControl _sensitivityLabel;

		private TextControl _pressToRebind;

		private TrackBarControl _sensitivityControl = new TrackBarControl();

		private ControlsTab.BindingScreenButtonControl[] _buttons;

		private TextControl[] _labels;

		private ScreenGroup _uiGroup;

		private FrameButtonControl _defaultButton = new FrameButtonControl();

		private FrameButtonControl invertYButton = new FrameButtonControl();

		private Rectangle _prevScreenSize = Rectangle.Empty;

		private class BindingScreenButtonControl : FrameButtonControl
		{
			public BindingScreenButtonControl(CastleMinerZControllerMapping.CMZControllerFunctions function)
			{
				this.Function = function;
			}

			public CastleMinerZControllerMapping.CMZControllerFunctions Function;
		}

		private class SelectButtonDialog : UIControlScreen
		{
			public SelectButtonDialog(SpriteFont font, CastleMinerZControllerMapping.CMZControllerFunctions function, FrameButtonControl hotkeyButton, ControlsTab controllerScreen)
				: base(true)
			{
				this._font = font;
				this._window = new Rectangle(Screen.Adjuster.ScreenRect.Width / 2 - 150, Screen.Adjuster.ScreenRect.Height / 2 - 75, 300, 150);
				this._function = function;
				this._hotkeyButton = hotkeyButton;
				this._currentBinding = this._binding.GetBinding((int)this._function, InputBinding.Slot.KeyMouse1);
				this._controllerScreen = controllerScreen;
				this.textRegion = new TextRegionControl(font);
				this.textRegion.LocalBounds = new Rectangle(this._window.Left + 10, this._window.Top + 30, this._window.Width - 20, this._window.Height - 40);
				this.textRegion.Text = Strings.Press_the_key_you_would_like_to_set_for_this_control;
				this.textRegion.Color = Color.Black;
				base.Controls.Add(this.textRegion);
				this.cancelButton.Font = font;
				this.cancelButton.Frame = CastleMinerZGame.Instance.ButtonFrame;
				this.cancelButton.Text = Strings.Cancel;
				this.cancelButton.Size = new Size(100, font.LineSpacing);
				this.cancelButton.LocalPosition = new Point(this._window.Right - 110, this._window.Bottom - font.LineSpacing - 10);
				this.cancelButton.Pressed += this.cancelButton_Pressed;
				base.Controls.Add(this.cancelButton);
			}

			public override void OnPushed()
			{
				SoundManager.Instance.PlayInstance("Popup");
				this._binding.InitBindableSensor();
				base.OnPushed();
			}

			protected override void OnUpdate(DNAGame game, GameTime gameTime)
			{
				this._window = new Rectangle(Screen.Adjuster.ScreenRect.Width / 2 - 150, Screen.Adjuster.ScreenRect.Height / 2 - 75, 300, 150);
				this.textRegion.LocalBounds = new Rectangle(this._window.Left + 10, this._window.Top + 30, this._window.Width - 20, this._window.Height - 40);
				this.cancelButton.LocalPosition = new Point(this._window.Right - 110, this._window.Bottom - this._font.LineSpacing - 10);
				base.OnUpdate(game, gameTime);
			}

			protected override bool OnInput(InputManager inputManager, GameTime gameTime)
			{
				if (!inputManager.Mouse.LeftButtonPressed || !this.cancelButton.HitTest(inputManager.Mouse.Position))
				{
					InputBinding.Bindable bound = this._binding.SenseBindable(InputBinding.Slot.KeyMouse1, inputManager.Keyboard, inputManager.Mouse, inputManager.Controllers[0]);
					InputBinding.Bindable bindable = bound;
					if (bindable != InputBinding.Bindable.None)
					{
						if (bindable == InputBinding.Bindable.KeyEscape)
						{
							SoundManager.Instance.PlayInstance("Click");
							base.PopMe();
						}
						else
						{
							this._binding.Bind((int)this._function, InputBinding.Slot.KeyMouse1, bound);
							this._controllerScreen.ResetAllButtonText();
							SoundManager.Instance.PlayInstance("Click");
							base.PopMe();
						}
					}
				}
				return base.OnInput(inputManager, gameTime);
			}

			private void cancelButton_Pressed(object sender, EventArgs e)
			{
				base.PopMe();
			}

			protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
			{
				spriteBatch.Begin();
				CastleMinerZGame.Instance.ButtonFrame.Draw(spriteBatch, this._window, new Color(0.87f, 0.87f, 0.87f, 0.87f));
				spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle(this._window.X, this._window.Y, this._window.Width, 20), new Color(0f, 0f, 0f, 0.87f));
				spriteBatch.End();
				base.OnDraw(device, spriteBatch, gameTime);
			}

			private Rectangle _window;

			private CastleMinerZControllerMapping.CMZControllerFunctions _function;

			private FrameButtonControl _hotkeyButton;

			private InputBinding _binding = CastleMinerZGame.Instance._controllerMapping.Binding;

			private FrameButtonControl cancelButton = new FrameButtonControl();

			private InputBinding.Bindable _currentBinding;

			private ControlsTab _controllerScreen;

			private TextRegionControl textRegion;

			private SpriteFont _font;
		}
	}
}
