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
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			GamePadState controller = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
			this.angles.Y = this.angles.Y - controller.ThumbSticks.Right.X * dt;
			this.angles.X = this.angles.X + controller.ThumbSticks.Right.Y * dt;
			if (Math.Abs(this.angles.X) > 1.2566371f)
			{
				this.angles.X = 1.2566371f * (float)Math.Sign(this.angles.X);
			}
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(this.angles.Y, this.angles.X, 0f);
			this.velocity = controller.ThumbSticks.Left.Y * base.LocalToWorld.Forward;
			this.velocity += controller.ThumbSticks.Left.X * base.LocalToWorld.Right;
			base.LocalPosition += this.velocity * dt * 3f;
			base.OnUpdate(gameTime);
		}

		public Vector3 velocity = Vector3.Zero;

		public Vector3 angles;
	}
}
