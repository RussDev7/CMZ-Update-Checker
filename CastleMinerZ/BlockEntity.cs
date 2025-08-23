using System;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class BlockEntity : Entity
	{
		public static void Initialize()
		{
			CastleMinerZGame game = CastleMinerZGame.Instance;
			BlockEntity._effect = BlockTerrain.Instance._effect;
			BlockEntity._vb = new VertexBuffer(game.GraphicsDevice, typeof(BlockEntity.BlockEntityVertex), 24, BufferUsage.WriteOnly);
			BlockEntity.BlockEntityVertex[] verts = new BlockEntity.BlockEntityVertex[24];
			int[] txs = new int[] { 0, 0, 0, 0, 1, 2 };
			IntVector3 indexer = new IntVector3(1, 2, 4);
			for (BlockFace f = BlockFace.POSX; f < BlockFace.NUM_FACES; f++)
			{
				int idx = (int)(f * BlockFace.POSY);
				int num = txs[(int)f];
				int v = 0;
				while (v < 4)
				{
					verts[(int)(f * BlockFace.POSY + v)] = new BlockEntity.BlockEntityVertex(f, indexer.Dot(BlockVertex._faceVertices[idx]), v, (int)f);
					v++;
					idx++;
				}
			}
			BlockEntity._vb.SetData<BlockEntity.BlockEntityVertex>(verts, 0, verts.Length);
		}

		public BlockEntity(BlockTypeEnum blockType, ItemUse use, bool attachedToLocalPlayer)
		{
			this._blockType = blockType;
			this.AttachedToLocalPlayer = attachedToLocalPlayer;
			this.Context = use;
			if (attachedToLocalPlayer)
			{
				this.DrawPriority = 602;
				return;
			}
			if (BlockType.GetType(this._blockType).NeedsFancyLighting)
			{
				this.DrawPriority = 601;
				return;
			}
			this.DrawPriority = 600;
		}

		public static void InitUIRendering(Matrix projection)
		{
			BlockEntity._effect.Parameters["Projection"].SetValue(projection);
			BlockEntity._effect.Parameters["View"].SetValue(Matrix.Identity);
			BlockEntity._effect.Parameters["WaterDepth"].SetValue(10000);
			BlockEntity._effect.Parameters["WaterLevel"].SetValue(-10000);
			Vector3 waterCalculations = default(Vector3);
			waterCalculations.X = 0f;
			waterCalculations.Y = 1f;
			waterCalculations.Z = -1000f;
			BlockEntity._effect.Parameters["EyeWaterConstants"].SetValue(waterCalculations);
			BlockEntity._effect.Parameters["LightDirection"].SetValue(Vector3.Backward);
			BlockEntity._effect.Parameters["TorchLight"].SetValue(new Color(255, 235, 190).ToVector3());
			BlockEntity._effect.Parameters["SunLight"].SetValue(Vector3.One);
			BlockEntity._effect.Parameters["AmbientSun"].SetValue(new Vector3(0.4f, 0.4f, 0.4f));
			BlockEntity._effect.Parameters["SunSpecular"].SetValue(Vector3.One);
			BlockEntity._effect.Parameters["FogColor"].SetValue(Vector3.One);
			BlockEntity._effect.Parameters["BelowWaterColor"].SetValue(Vector3.Zero);
		}

		public void UIObject()
		{
			this._uiObject = true;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (!this._uiObject && BlockTerrain.Instance != null && BlockTerrain.Instance.IsReady)
			{
				Vector3 pos = base.WorldPosition;
				if (this.AttachedToLocalPlayer)
				{
					pos = CastleMinerZGame.Instance.LocalPlayer.FPSCamera.WorldPosition;
				}
				Vector2 light = BlockTerrain.Instance.GetLightAtPoint(pos);
				this.SunLight = light.X * 255f;
				this.TorchLight = Math.Max(125f, light.Y * 255f);
			}
			base.OnUpdate(gameTime);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			BlockType bt = BlockType.GetType(this._blockType);
			Matrix worldMat = base.LocalToWorld;
			BlockEntity._effect.Parameters["World"].SetValue(worldMat);
			BlockEntity._effect.Parameters["View"].SetValue(view);
			BlockEntity._effect.Parameters["Projection"].SetValue(projection);
			BlockEntity._effect.Parameters["InverseWorld"].SetValue(worldMat.QuickInvert());
			BlockEntity._effect.Parameters["CubeScaleSunTorch"].SetValue(new Vector3(this.Scale, this.SunLight, this.TorchLight));
			BlockEntity._effect.Parameters["CubeTx"].SetValue(bt.TileIndices);
			GraphicsDevice g = CastleMinerZGame.Instance.GraphicsDevice;
			g.SetVertexBuffer(BlockEntity._vb);
			g.Indices = BlockTerrain.Instance._staticIB;
			if (this._uiObject)
			{
				Vector3 v = worldMat.Translation;
				v.Z = 0f;
				BlockEntity._effect.Parameters["EyePosition"].SetValue(v);
				BlockEntity._effect.CurrentTechnique = BlockEntity._effect.Techniques[6];
				BlockEntity._effect.CurrentTechnique.Passes[bt.NeedsFancyLighting ? 3 : 2].Apply();
			}
			else if (CastleMinerZGame.Instance.DrawingReflection)
			{
				BlockEntity._effect.CurrentTechnique = BlockEntity._effect.Techniques[4];
				BlockEntity._effect.CurrentTechnique.Passes[2].Apply();
			}
			else
			{
				BlockEntity._effect.CurrentTechnique = BlockEntity._effect.Techniques[BlockTerrain.Instance.IsWaterWorld ? 4 : 6];
				BlockEntity._effect.CurrentTechnique.Passes[bt.NeedsFancyLighting ? 1 : 0].Apply();
			}
			g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
		}

		private static Effect _effect;

		private static VertexBuffer _vb;

		private static float[] _materialTx = new float[6];

		private BlockTypeEnum _blockType;

		public float Scale;

		public float SunLight = 255f;

		public float TorchLight;

		public bool _uiObject;

		public bool AttachedToLocalPlayer = true;

		public ItemUse Context = ItemUse.Hand;

		private struct BlockEntityVertex : IVertexType
		{
			public BlockEntityVertex(BlockFace face, int cvx, int fvx, int tx)
			{
				this._fvxFaceTxCvx = fvx | (int)((int)face << 8) | (tx << 16) | (cvx << 24);
			}

			VertexDeclaration IVertexType.VertexDeclaration
			{
				get
				{
					return BlockEntity.BlockEntityVertex.VertexDeclaration;
				}
			}

			private int _fvxFaceTxCvx;

			public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[]
			{
				new VertexElement(0, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 0)
			});
		}
	}
}
