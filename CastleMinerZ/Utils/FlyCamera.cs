using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.Utils
{
	internal class FlyCamera : PerspectiveCamera
	{
		protected override void OnUpdate(GameTime gameTime)
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			GamePadState state = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
			this.angles.Y = this.angles.Y - state.ThumbSticks.Right.X * num;
			this.angles.X = this.angles.X + state.ThumbSticks.Right.Y * num;
			if (Math.Abs(this.angles.X) > 1.2566371f)
			{
				this.angles.X = 1.2566371f * (float)Math.Sign(this.angles.X);
			}
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(this.angles.Y, this.angles.X, 0f);
			this.velocity = state.ThumbSticks.Left.Y * base.LocalToWorld.Forward;
			this.velocity += state.ThumbSticks.Left.X * base.LocalToWorld.Right;
			base.LocalPosition += this.velocity * num * 3f;
			base.OnUpdate(gameTime);
		}

		public Vector3 velocity = Vector3.Zero;

		public Vector3 angles;
	}
}
