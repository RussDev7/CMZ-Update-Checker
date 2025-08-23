using System;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class ReflectionCamera : PerspectiveCamera
	{
		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime time, FilterCallback<Entity> entityFilter)
		{
			if (!BlockTerrain.Instance.IsWaterWorld)
			{
				return;
			}
			CastleMinerZGame.Instance.DrawingReflection = true;
			PerspectiveCamera c = CastleMinerZGame.Instance.LocalPlayer.FPSCamera;
			this.FieldOfView = c.FieldOfView;
			this.NearPlane = c.NearPlane;
			this.FarPlane = c.FarPlane;
			Matrix i = c.LocalToWorld;
			Matrix newV = Matrix.Multiply(i, BlockTerrain.Instance.GetReflectionMatrix());
			base.LocalToParent = newV;
			base.Draw(device, spriteBatch, time, entityFilter);
			CastleMinerZGame.Instance.DrawingReflection = false;
		}
	}
}
