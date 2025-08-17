using System;
using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	internal class ControllerScreen : UIControlScreen
	{
		public ControllerScreen(CastleMinerZGame game, bool InGame, ScreenGroup uiGroup)
			: base(false)
		{
			this._game = game;
			this._controlsFont = this._game._medFont;
			this._smallControlsFont = this._game._smallFont;
			this.inGame = InGame;
			this._uiGroup = uiGroup;
			float num = this._controlsFont.MeasureString(Strings.Invert_Y_Axis).X;
			float num2 = Math.Max(this._controlsFont.MeasureString(Strings.Selects_the_appropriate_quickbar_item).X, this._controlsFont.MeasureString(Strings.Opens_the_menu__Pauses_offline_games).X);
			float num3 = this._smallControlsFont.MeasureString(Strings.Invert_Y_Axis).X;
			float num4 = Math.Max(this._smallControlsFont.MeasureString(Strings.Selects_the_appropriate_quickbar_item).X, this._smallControlsFont.MeasureString(Strings.Opens_the_menu__Pauses_offline_games).X);
			bool flag = false;
			this._buttonLabelLengths = new float[this._buttonLabels.Length];
			this._smallButtonLabelLengths = new float[this._buttonLabels.Length];
			for (int i = 0; i < this._buttonLabels.Length; i++)
			{
				if (this._buttonLabels[i] != null)
				{
					this._buttonLabelLengths[i] = this._controlsFont.MeasureString(this._buttonLabels[i]).X;
					this._smallButtonLabelLengths[i] = this._smallControlsFont.MeasureString(this._buttonLabels[i]).X;
					if (flag)
					{
						num2 = Math.Max(num2, this._buttonLabelLengths[i]);
						num4 = Math.Max(num4, this._smallButtonLabelLengths[i]);
					}
					else
					{
						num = Math.Max(num, this._buttonLabelLengths[i]);
						num3 = Math.Max(num3, this._smallButtonLabelLengths[i]);
					}
				}
				else
				{
					flag = true;
				}
			}
			this._normalFontWidth = (int)(num + num2 + 270f + 10f + 10f);
			this._smallFontWidth = (int)(num3 + num4 + 270f + 10f + 10f);
			this._normalLeftSize = (int)num + 5;
			this._smallLeftSize = (int)num3 + 5;
			Color color = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			ImageButtonControl imageButtonControl = new ImageButtonControl();
			imageButtonControl.Image = this._game._uiSprites["BackArrow"];
			imageButtonControl.Font = this._game._medFont;
			imageButtonControl.LocalPosition = new Point(15, 15);
			imageButtonControl.Pressed += this._backButton_Pressed;
			imageButtonControl.Text = " " + Strings.Back;
			imageButtonControl.ImageDefaultColor = color;
			base.Controls.Add(imageButtonControl);
			this._defaultButton.LocalPosition = new Point(690, 595);
			this._defaultButton.Size = new Size(225, 40);
			this._defaultButton.Text = Strings.Reset_To_Default;
			this._defaultButton.Font = this._controlsFont;
			this._defaultButton.Frame = this._game.ButtonFrame;
			this._defaultButton.Pressed += this._defaultButton_Pressed;
			this._defaultButton.ButtonColor = color;
			base.Controls.Add(this._defaultButton);
			this.invertYButton.LocalPosition = new Point(310, 595);
			this.invertYButton.Size = new Size(135, 40);
			this.invertYButton.Text = (this._game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			this.invertYButton.Font = this._controlsFont;
			this.invertYButton.Frame = this._game.ButtonFrame;
			this.invertYButton.Pressed += this.invertYButton_Pressed;
			this.invertYButton.ButtonColor = color;
			base.Controls.Add(this.invertYButton);
			Point point = new Point(Screen.Adjuster.ScreenRect.Center.X - 140, 145);
			int num5 = (int)((float)this._controlsFont.LineSpacing * 1.25f);
			this._buttons = new ControllerScreen.BindingScreenButtonControl[this._buttonOrder.Length - 1];
			int num6 = 0;
			for (int j = 0; j < this._buttonOrder.Length; j++)
			{
				if (this._buttonOrder[j] == CastleMinerZControllerMapping.CMZControllerFunctions.Count)
				{
					point = new Point(Screen.Adjuster.ScreenRect.Center.X + 5, 145);
				}
				else
				{
					ControllerScreen.BindingScreenButtonControl bindingScreenButtonControl = new ControllerScreen.BindingScreenButtonControl(this._buttonOrder[j]);
					bindingScreenButtonControl.LocalPosition = point;
					bindingScreenButtonControl.Size = new Size(135, 40);
					bindingScreenButtonControl.Text = InputBinding.KeyString(this._binding.GetBinding((int)this._buttonOrder[j], InputBinding.Slot.KeyMouse1));
					bindingScreenButtonControl.Font = this._controlsFont;
					bindingScreenButtonControl.Frame = this._game.ButtonFrame;
					bindingScreenButtonControl.Pressed += this._bindingBtn_Pressed;
					base.Controls.Add(bindingScreenButtonControl);
					this._buttons[num6++] = bindingScreenButtonControl;
					point.Y += num5;
					bindingScreenButtonControl.ButtonColor = color;
				}
			}
		}

		private void _bindingBtn_Pressed(object sender, EventArgs e)
		{
			ControllerScreen.BindingScreenButtonControl bindingScreenButtonControl = sender as ControllerScreen.BindingScreenButtonControl;
			if (bindingScreenButtonControl != null)
			{
				this._uiGroup.PushScreen(new ControllerScreen.SelectButtonDialog(this._controlsFont, bindingScreenButtonControl.Function, bindingScreenButtonControl, this));
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

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			base.PopMe();
		}

		public void ResetAllButtonText()
		{
			for (int i = 0; i < this._buttons.Length; i++)
			{
				this._buttons[i].Text = InputBinding.KeyString(this._binding.GetBinding((int)this._buttons[i].Function, InputBinding.Slot.KeyMouse1));
			}
		}

		public override void OnPushed()
		{
			if (!this._binding.Initialized)
			{
				CastleMinerZGame.Instance._controllerMapping.SetToDefault();
			}
			this.invertYButton.Text = (this._game.PlayerStats.InvertYAxis ? Strings.Inverted : Strings.Regular);
			this.ResetAllButtonText();
			base.OnPushed();
		}

		public override void OnPoped()
		{
			try
			{
				this._game.SavePlayerStats(this._game.PlayerStats);
				CastleMinerZGame.GlobalSettings.Save();
			}
			catch
			{
			}
			base.OnPoped();
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (controller.PressedButtons.B || controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				base.PopMe();
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			if (this.inGame)
			{
				spriteBatch.Draw(this._game.DummyTexture, Screen.Adjuster.ScreenRect, new Color(0f, 0f, 0f, 0.5f));
			}
			float num = (float)this._controlsFont.LineSpacing * 1.35f;
			int num2 = Math.Max(55, Screen.Adjuster.ScreenRect.Center.Y - (int)num * 5);
			Vector2 vector = new Vector2(125f, (float)num2);
			Point point = new Point(Screen.Adjuster.ScreenRect.Center.X - this._normalFontWidth / 2 + this._normalLeftSize, num2);
			int num3 = 0;
			SpriteFont spriteFont = this._controlsFont;
			float[] array;
			if (Screen.Adjuster.ScreenRect.Width < this._normalFontWidth)
			{
				point.X = Screen.Adjuster.ScreenRect.Center.X - this._smallFontWidth / 2 + this._smallLeftSize;
				vector.Y = (float)(num2 + 7);
				spriteFont = this._smallControlsFont;
				array = this._smallButtonLabelLengths;
			}
			else
			{
				array = this._buttonLabelLengths;
			}
			bool flag = false;
			for (int i = 0; i < this._buttonLabels.Length; i++)
			{
				if (this._buttonLabels[i] == null)
				{
					flag = true;
					point = new Point(point.X + 145, num2);
					vector = new Vector2((float)(point.X + 140), (float)num2);
					if (Screen.Adjuster.ScreenRect.Width < this._normalFontWidth)
					{
						vector.Y = (float)(num2 + 7);
					}
				}
				else
				{
					this._buttons[num3].LocalPosition = point;
					point.Y += (int)num;
					num3++;
					if (!flag)
					{
						vector.X = (float)point.X - array[i] - 5f;
					}
					spriteBatch.DrawOutlinedText(spriteFont, this._buttonLabels[i], vector, Color.White, Color.Black, 1);
					vector.Y += num;
				}
			}
			spriteBatch.DrawOutlinedText(this._controlsFont, "1 - 8", vector - new Vector2(100f, 0f), CMZColors.MenuGreen, Color.Black, 1);
			spriteBatch.DrawOutlinedText(spriteFont, Strings.Selects_the_appropriate_quickbar_item, vector, Color.White, Color.Black, 1);
			vector.Y += num;
			spriteBatch.DrawOutlinedText(this._controlsFont, "Esc", vector - new Vector2(100f, 0f), CMZColors.MenuGreen, Color.Black, 1);
			spriteBatch.DrawOutlinedText(spriteFont, Strings.Opens_the_menu__Pauses_offline_games, vector, Color.White, Color.Black, 1);
			point.Y += (int)num * 2;
			this._defaultButton.LocalPosition = point;
			point.X = this._buttons[0].LocalPosition.X;
			this.invertYButton.LocalPosition = point;
			vector.Y += num;
			vector.X = (float)(point.X - 10) - spriteFont.MeasureString(Strings.Invert_Y_Axis).X;
			spriteBatch.DrawOutlinedText(spriteFont, Strings.Invert_Y_Axis + ":", vector, Color.White, Color.Black, 1);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private CastleMinerZGame _game;

		private SpriteFont _controlsFont;

		private SpriteFont _smallControlsFont;

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
			CastleMinerZControllerMapping.CMZControllerFunctions.TextChat,
			CastleMinerZControllerMapping.CMZControllerFunctions.Count,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder_Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.Shoulder,
			CastleMinerZControllerMapping.CMZControllerFunctions.Activate,
			CastleMinerZControllerMapping.CMZControllerFunctions.NextItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.PreviousItem,
			CastleMinerZControllerMapping.CMZControllerFunctions.BlockUI,
			CastleMinerZControllerMapping.CMZControllerFunctions.DropQuickBarItem
		};

		private string[] _buttonLabels = new string[]
		{
			Strings.Use_Shoot,
			Strings.Move_forward,
			Strings.Move_backward,
			Strings.Strafe_left,
			Strings.Strafe_right,
			Strings.Jump,
			Strings.Coming_soon,
			Strings.Reload,
			Strings.Show_Player_List,
			Strings.Show_Chat,
			null,
			Strings.Activate_Shoulder,
			Strings.Shoulder,
			Strings.Activate,
			Strings.Next_Item,
			Strings.Previous_Item,
			Strings.Opens_your_inventory,
			Strings.Drop_an_item_from_the_quick_bar
		};

		private ControllerScreen.BindingScreenButtonControl[] _buttons;

		private float[] _buttonLabelLengths;

		private float[] _smallButtonLabelLengths;

		private ScreenGroup _uiGroup;

		private FrameButtonControl _defaultButton = new FrameButtonControl();

		private int _normalFontWidth;

		private int _smallFontWidth;

		private int _normalLeftSize;

		private int _smallLeftSize;

		private FrameButtonControl invertYButton = new FrameButtonControl();

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
			public SelectButtonDialog(SpriteFont font, CastleMinerZControllerMapping.CMZControllerFunctions function, FrameButtonControl hotkeyButton, ControllerScreen controllerScreen)
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
					InputBinding.Bindable bindable = this._binding.SenseBindable(InputBinding.Slot.KeyMouse1, inputManager.Keyboard, inputManager.Mouse, inputManager.Controllers[0]);
					InputBinding.Bindable bindable2 = bindable;
					if (bindable2 != InputBinding.Bindable.None)
					{
						if (bindable2 != InputBinding.Bindable.KeyEscape)
						{
							switch (bindable2)
							{
							case InputBinding.Bindable.KeyD1:
							case InputBinding.Bindable.KeyD2:
							case InputBinding.Bindable.KeyD3:
							case InputBinding.Bindable.KeyD4:
							case InputBinding.Bindable.KeyD5:
							case InputBinding.Bindable.KeyD6:
							case InputBinding.Bindable.KeyD7:
							case InputBinding.Bindable.KeyD8:
								SoundManager.Instance.PlayInstance("Error");
								this._binding.InitBindableSensor();
								break;
							default:
								this._binding.Bind((int)this._function, InputBinding.Slot.KeyMouse1, bindable);
								this._controllerScreen.ResetAllButtonText();
								SoundManager.Instance.PlayInstance("Click");
								base.PopMe();
								break;
							}
						}
						else
						{
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

			private ControllerScreen _controllerScreen;

			private TextRegionControl textRegion;

			private SpriteFont _font;
		}
	}
}
