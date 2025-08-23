using System;
using System.Text;
using System.Threading;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class WaitScreen : Screen
	{
		public static void DoWait(ScreenGroup group, string message)
		{
			WaitScreen screen = new WaitScreen(message, null);
			screen.Start(group);
		}

		public static void DoWait(ScreenGroup group, string message, ProgressCallback progress)
		{
			if (progress())
			{
				return;
			}
			WaitScreen screen = new WaitScreen(message, progress);
			screen.Start(group);
		}

		public static void DoWait(ScreenGroup group, string message, ThreadStart longOperation, ThreadStart onComplete)
		{
			WaitScreen screen = new WaitScreen(message, longOperation, onComplete);
			screen.Start(group);
		}

		public string Message
		{
			set
			{
				this._message = value;
			}
		}

		private WaitScreen(string message, ThreadStart operation, ThreadStart onComplete)
			: base(true, false)
		{
			this._operation = operation;
			this._message = message;
			this._complete = onComplete;
		}

		public WaitScreen(string message, bool drawProgress, ThreadStart operation, ThreadStart onComplete)
			: base(true, false)
		{
			this._operation = operation;
			this._message = message;
			this._complete = onComplete;
			this._drawProgress = drawProgress;
		}

		private WaitScreen(string message, ProgressCallback progress)
			: base(true, false)
		{
			this._progress = progress;
			this._message = message;
		}

		public WaitScreen(string message)
			: base(true, false)
		{
			this._message = message;
		}

		public void Start(ScreenGroup group)
		{
			group.PushScreen(this);
			if (this._operation != null)
			{
				CastleMinerZGame.Instance.TaskScheduler.DoUserWorkItem(new ParameterizedThreadStart(this.DoOperation), null);
			}
		}

		public void DoOperation(object state)
		{
			this._operation();
			base.PopMe();
			if (this._complete != null)
			{
				this._complete();
			}
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (this._progress != null && this._progress())
			{
				base.PopMe();
				if (this._complete != null)
				{
					this._complete();
				}
			}
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteFont largeFont = CastleMinerZGame.Instance._largeFont;
			Vector2 size = largeFont.MeasureString(this._message);
			Vector2 position = new Vector2((float)(Screen.Adjuster.ScreenRect.Width / 2) - size.X / 2f, (float)(Screen.Adjuster.ScreenRect.Height / 2) + size.Y / 2f);
			this.textFlashTimer.Update(gameTime.ElapsedGameTime);
			Color currentColor = Color.Lerp(CMZColors.MenuGreen, Color.White, this.textFlashTimer.PercentComplete);
			if (this.textFlashTimer.Expired)
			{
				this.textFlashTimer.Reset();
			}
			spriteBatch.Begin();
			spriteBatch.DrawOutlinedText(largeFont, this._message, position, currentColor, Color.Black, 1);
			if (this._drawProgress)
			{
				this.sbuilder.Length = 0;
				this.sbuilder.Concat(this.Progress);
				this.sbuilder.Append("%");
				float location = position.X + largeFont.MeasureString(this._message).X + largeFont.MeasureString(" 100%").X - largeFont.MeasureString(this.sbuilder).X;
				spriteBatch.DrawOutlinedText(largeFont, this.sbuilder, new Vector2(location, position.Y), currentColor, Color.Black, 1);
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private ProgressCallback _progress;

		private ThreadStart _operation;

		private ThreadStart _complete;

		private string _message;

		public bool _drawProgress;

		public int Progress;

		private OneShotTimer textFlashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5), true);

		private StringBuilder sbuilder = new StringBuilder();
	}
}
