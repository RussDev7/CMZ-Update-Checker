using System;
using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Terrain
{
	public class WaterPlane : Entity
	{
		public WaterPlane(GraphicsDevice gd, ContentManager cm)
		{
			WaterPlane.Instance = this;
			WaterPlane.PositionVX[] array = new WaterPlane.PositionVX[30];
			float num = 384f;
			float num2 = 384f;
			float num3 = -128f;
			array[0] = new WaterPlane.PositionVX(new Vector3(num, 0f, 0f));
			array[1] = new WaterPlane.PositionVX(new Vector3(num, 0f, num2));
			array[2] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			array[3] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			array[4] = new WaterPlane.PositionVX(new Vector3(num, 0f, num2));
			array[5] = new WaterPlane.PositionVX(new Vector3(0f, 0f, num2));
			this._waterVerts = new VertexBuffer(gd, typeof(WaterPlane.PositionVX), 6, BufferUsage.WriteOnly);
			this._waterVerts.SetData<WaterPlane.PositionVX>(array, 0, 6);
			array[0] = new WaterPlane.PositionVX(new Vector3(num, 0f, 0f));
			array[1] = new WaterPlane.PositionVX(new Vector3(num, num3, 0f));
			array[2] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			array[3] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			array[4] = new WaterPlane.PositionVX(new Vector3(num, num3, 0f));
			array[5] = new WaterPlane.PositionVX(new Vector3(0f, num3, 0f));
			array[6] = new WaterPlane.PositionVX(new Vector3(0f, 0f, num2));
			array[7] = new WaterPlane.PositionVX(new Vector3(0f, num3, num2));
			array[8] = new WaterPlane.PositionVX(new Vector3(num, 0f, num2));
			array[9] = new WaterPlane.PositionVX(new Vector3(num, 0f, num2));
			array[10] = new WaterPlane.PositionVX(new Vector3(0f, num3, num2));
			array[11] = new WaterPlane.PositionVX(new Vector3(num, num3, num2));
			array[12] = new WaterPlane.PositionVX(new Vector3(num, 0f, num2));
			array[13] = new WaterPlane.PositionVX(new Vector3(num, num3, num2));
			array[14] = new WaterPlane.PositionVX(new Vector3(num, 0f, 0f));
			array[15] = new WaterPlane.PositionVX(new Vector3(num, 0f, 0f));
			array[16] = new WaterPlane.PositionVX(new Vector3(num, num3, num2));
			array[17] = new WaterPlane.PositionVX(new Vector3(num, num3, 0f));
			array[18] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			array[19] = new WaterPlane.PositionVX(new Vector3(0f, num3, 0f));
			array[20] = new WaterPlane.PositionVX(new Vector3(0f, 0f, num2));
			array[21] = new WaterPlane.PositionVX(new Vector3(0f, 0f, num2));
			array[22] = new WaterPlane.PositionVX(new Vector3(0f, num3, 0f));
			array[23] = new WaterPlane.PositionVX(new Vector3(0f, num3, num2));
			array[24] = new WaterPlane.PositionVX(new Vector3(num, num3, 0f));
			array[25] = new WaterPlane.PositionVX(new Vector3(num, num3, num2));
			array[26] = new WaterPlane.PositionVX(new Vector3(0f, num3, 0f));
			array[27] = new WaterPlane.PositionVX(new Vector3(0f, num3, 0f));
			array[28] = new WaterPlane.PositionVX(new Vector3(num, num3, num2));
			array[29] = new WaterPlane.PositionVX(new Vector3(0f, num3, num2));
			this._wellVerts = new VertexBuffer(gd, typeof(WaterPlane.PositionVX), array.Length, BufferUsage.WriteOnly);
			this._wellVerts.SetData<WaterPlane.PositionVX>(array, 0, array.Length);
			this._reflectionTexture = new RenderTarget2D(CastleMinerZGame.Instance.GraphicsDevice, Screen.Adjuster.ScreenRect.Width, Screen.Adjuster.ScreenRect.Height, true, SurfaceFormat.Color, DepthFormat.Depth16);
			this._effect = cm.Load<Effect>("Shaders\\WaterEffect");
			this._normalMap = cm.Load<Texture2D>("Terrain\\water_normalmap");
			this._effect.Parameters["NormalTexture"].SetValue(this._normalMap);
			this.Collidee = false;
			this.DrawPriority = 1000;
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (!BlockTerrain.Instance.IsWaterWorld)
			{
				return;
			}
			if (CastleMinerZGame.Instance.DrawingReflection)
			{
				this._effect.Parameters["Reflection"].SetValue(view);
				return;
			}
			this._effect.Parameters["Projection"].SetValue(projection);
			this._effect.Parameters["View"].SetValue(view);
			this._effect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
			this._effect.Parameters["LightDirection"].SetValue(BlockTerrain.Instance.VectorToSun);
			Vector2 lightAtPoint = BlockTerrain.Instance.GetLightAtPoint(BlockTerrain.Instance.EyePos);
			this._effect.Parameters["SunLightColor"].SetValue(BlockTerrain.Instance.SunSpecular.ToVector3() * (float)Math.Pow((double)lightAtPoint.X, 10.0));
			this._effect.Parameters["TorchLightColor"].SetValue(BlockTerrain.Instance.TorchColor.ToVector3() * lightAtPoint.Y);
			Matrix identity = Matrix.Identity;
			Vector3 vector = IntVector3.ToVector3(BlockTerrain.Instance._worldMin);
			vector.Y = BlockTerrain.Instance.WaterLevel;
			identity.Translation = vector;
			this._effect.Parameters["World"].SetValue(identity);
			Vector3 eyePos = BlockTerrain.Instance.EyePos;
			this._effect.Parameters["EyePos"].SetValue(BlockTerrain.Instance.EyePos);
			this._effect.Parameters["ReflectionTexture"].SetValue(this._reflectionTexture);
			this._effect.Parameters["WaterColor"].SetValue(BlockTerrain.Instance.GetActualWaterColor());
			BlendState blendState = device.BlendState;
			RasterizerState rasterizerState = device.RasterizerState;
			if (BlockTerrain.Instance.EyePos.Y >= BlockTerrain.Instance.WaterLevel)
			{
				this._effect.CurrentTechnique = this._effect.Techniques[0];
				device.BlendState = BlendState.AlphaBlend;
			}
			else
			{
				this._effect.CurrentTechnique = this._effect.Techniques[1];
				device.BlendState = BlendState.Opaque;
			}
			device.DepthStencilState = DepthStencilState.DepthRead;
			device.SetVertexBuffer(this._wellVerts);
			this._effect.CurrentTechnique.Passes[1].Apply();
			device.DrawPrimitives(PrimitiveType.TriangleList, 0, 10);
			device.SetVertexBuffer(this._waterVerts);
			this._effect.CurrentTechnique.Passes[0].Apply();
			device.RasterizerState = RasterizerState.CullNone;
			device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
			device.DepthStencilState = DepthStencilState.Default;
			device.BlendState = blendState;
			device.RasterizerState = rasterizerState;
			base.Draw(device, gameTime, view, projection);
		}

		private VertexBuffer _waterVerts;

		private VertexBuffer _wellVerts;

		public RenderTarget2D _reflectionTexture;

		public Effect _effect;

		public Texture2D _normalMap;

		public static WaterPlane Instance;

		private struct PositionVX : IVertexType
		{
			VertexDeclaration IVertexType.VertexDeclaration
			{
				get
				{
					return WaterPlane.PositionVX.VertexDeclaration;
				}
			}

			public PositionVX(Vector3 pos)
			{
				this.Position = pos;
			}

			public Vector3 Position;

			public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[]
			{
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
			});
		}
	}
}
