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
			WaterPlane.PositionVX[] verts = new WaterPlane.PositionVX[30];
			float width = 384f;
			float depth = 384f;
			float height = -128f;
			verts[0] = new WaterPlane.PositionVX(new Vector3(width, 0f, 0f));
			verts[1] = new WaterPlane.PositionVX(new Vector3(width, 0f, depth));
			verts[2] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			verts[3] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			verts[4] = new WaterPlane.PositionVX(new Vector3(width, 0f, depth));
			verts[5] = new WaterPlane.PositionVX(new Vector3(0f, 0f, depth));
			this._waterVerts = new VertexBuffer(gd, typeof(WaterPlane.PositionVX), 6, BufferUsage.WriteOnly);
			this._waterVerts.SetData<WaterPlane.PositionVX>(verts, 0, 6);
			verts[0] = new WaterPlane.PositionVX(new Vector3(width, 0f, 0f));
			verts[1] = new WaterPlane.PositionVX(new Vector3(width, height, 0f));
			verts[2] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			verts[3] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			verts[4] = new WaterPlane.PositionVX(new Vector3(width, height, 0f));
			verts[5] = new WaterPlane.PositionVX(new Vector3(0f, height, 0f));
			verts[6] = new WaterPlane.PositionVX(new Vector3(0f, 0f, depth));
			verts[7] = new WaterPlane.PositionVX(new Vector3(0f, height, depth));
			verts[8] = new WaterPlane.PositionVX(new Vector3(width, 0f, depth));
			verts[9] = new WaterPlane.PositionVX(new Vector3(width, 0f, depth));
			verts[10] = new WaterPlane.PositionVX(new Vector3(0f, height, depth));
			verts[11] = new WaterPlane.PositionVX(new Vector3(width, height, depth));
			verts[12] = new WaterPlane.PositionVX(new Vector3(width, 0f, depth));
			verts[13] = new WaterPlane.PositionVX(new Vector3(width, height, depth));
			verts[14] = new WaterPlane.PositionVX(new Vector3(width, 0f, 0f));
			verts[15] = new WaterPlane.PositionVX(new Vector3(width, 0f, 0f));
			verts[16] = new WaterPlane.PositionVX(new Vector3(width, height, depth));
			verts[17] = new WaterPlane.PositionVX(new Vector3(width, height, 0f));
			verts[18] = new WaterPlane.PositionVX(new Vector3(0f, 0f, 0f));
			verts[19] = new WaterPlane.PositionVX(new Vector3(0f, height, 0f));
			verts[20] = new WaterPlane.PositionVX(new Vector3(0f, 0f, depth));
			verts[21] = new WaterPlane.PositionVX(new Vector3(0f, 0f, depth));
			verts[22] = new WaterPlane.PositionVX(new Vector3(0f, height, 0f));
			verts[23] = new WaterPlane.PositionVX(new Vector3(0f, height, depth));
			verts[24] = new WaterPlane.PositionVX(new Vector3(width, height, 0f));
			verts[25] = new WaterPlane.PositionVX(new Vector3(width, height, depth));
			verts[26] = new WaterPlane.PositionVX(new Vector3(0f, height, 0f));
			verts[27] = new WaterPlane.PositionVX(new Vector3(0f, height, 0f));
			verts[28] = new WaterPlane.PositionVX(new Vector3(width, height, depth));
			verts[29] = new WaterPlane.PositionVX(new Vector3(0f, height, depth));
			this._wellVerts = new VertexBuffer(gd, typeof(WaterPlane.PositionVX), verts.Length, BufferUsage.WriteOnly);
			this._wellVerts.SetData<WaterPlane.PositionVX>(verts, 0, verts.Length);
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
			Vector2 pl = BlockTerrain.Instance.GetLightAtPoint(BlockTerrain.Instance.EyePos);
			this._effect.Parameters["SunLightColor"].SetValue(BlockTerrain.Instance.SunSpecular.ToVector3() * (float)Math.Pow((double)pl.X, 10.0));
			this._effect.Parameters["TorchLightColor"].SetValue(BlockTerrain.Instance.TorchColor.ToVector3() * pl.Y);
			Matrix i = Matrix.Identity;
			Vector3 basev = IntVector3.ToVector3(BlockTerrain.Instance._worldMin);
			basev.Y = BlockTerrain.Instance.WaterLevel;
			i.Translation = basev;
			this._effect.Parameters["World"].SetValue(i);
			Vector3 eyePos = BlockTerrain.Instance.EyePos;
			this._effect.Parameters["EyePos"].SetValue(BlockTerrain.Instance.EyePos);
			this._effect.Parameters["ReflectionTexture"].SetValue(this._reflectionTexture);
			this._effect.Parameters["WaterColor"].SetValue(BlockTerrain.Instance.GetActualWaterColor());
			BlendState oldState = device.BlendState;
			RasterizerState oldRState = device.RasterizerState;
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
			device.BlendState = oldState;
			device.RasterizerState = oldRState;
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
