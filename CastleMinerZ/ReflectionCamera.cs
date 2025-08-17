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
			PerspectiveCamera fpscamera = CastleMinerZGame.Instance.LocalPlayer.FPSCamera;
			this.FieldOfView = fpscamera.FieldOfView;
			this.NearPlane = fpscamera.NearPlane;
			this.FarPlane = fpscamera.FarPlane;
			Matrix localToWorld = fpscamera.LocalToWorld;
			Matrix matrix = Matrix.Multiply(localToWorld, BlockTerrain.Instance.GetReflectionMatrix());
			base.LocalToParent = matrix;
			base.Draw(device, spriteBatch, time, entityFilter);
			CastleMinerZGame.Instance.DrawingReflection = false;
		}
	}
}
