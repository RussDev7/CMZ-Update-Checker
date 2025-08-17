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
			CastleMinerZGame instance = CastleMinerZGame.Instance;
			BlockEntity._effect = BlockTerrain.Instance._effect;
			BlockEntity._vb = new VertexBuffer(instance.GraphicsDevice, typeof(BlockEntity.BlockEntityVertex), 24, BufferUsage.WriteOnly);
			BlockEntity.BlockEntityVertex[] array = new BlockEntity.BlockEntityVertex[24];
			int[] array2 = new int[] { 0, 0, 0, 0, 1, 2 };
			IntVector3 intVector = new IntVector3(1, 2, 4);
			for (BlockFace blockFace = BlockFace.POSX; blockFace < BlockFace.NUM_FACES; blockFace++)
			{
				int num = (int)(blockFace * BlockFace.POSY);
				int num2 = array2[(int)blockFace];
				int i = 0;
				while (i < 4)
				{
					array[(int)(blockFace * BlockFace.POSY + i)] = new BlockEntity.BlockEntityVertex(blockFace, intVector.Dot(BlockVertex._faceVertices[num]), i, (int)blockFace);
					i++;
					num++;
				}
			}
			BlockEntity._vb.SetData<BlockEntity.BlockEntityVertex>(array, 0, array.Length);
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
			Vector3 vector = default(Vector3);
			vector.X = 0f;
			vector.Y = 1f;
			vector.Z = -1000f;
			BlockEntity._effect.Parameters["EyeWaterConstants"].SetValue(vector);
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
				Vector3 vector = base.WorldPosition;
				if (this.AttachedToLocalPlayer)
				{
					vector = CastleMinerZGame.Instance.LocalPlayer.FPSCamera.WorldPosition;
				}
				Vector2 lightAtPoint = BlockTerrain.Instance.GetLightAtPoint(vector);
				this.SunLight = lightAtPoint.X * 255f;
				this.TorchLight = Math.Max(125f, lightAtPoint.Y * 255f);
			}
			base.OnUpdate(gameTime);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			BlockType type = BlockType.GetType(this._blockType);
			Matrix localToWorld = base.LocalToWorld;
			BlockEntity._effect.Parameters["World"].SetValue(localToWorld);
			BlockEntity._effect.Parameters["View"].SetValue(view);
			BlockEntity._effect.Parameters["Projection"].SetValue(projection);
			BlockEntity._effect.Parameters["InverseWorld"].SetValue(localToWorld.QuickInvert());
			BlockEntity._effect.Parameters["CubeScaleSunTorch"].SetValue(new Vector3(this.Scale, this.SunLight, this.TorchLight));
			BlockEntity._effect.Parameters["CubeTx"].SetValue(type.TileIndices);
			GraphicsDevice graphicsDevice = CastleMinerZGame.Instance.GraphicsDevice;
			graphicsDevice.SetVertexBuffer(BlockEntity._vb);
			graphicsDevice.Indices = BlockTerrain.Instance._staticIB;
			if (this._uiObject)
			{
				Vector3 translation = localToWorld.Translation;
				translation.Z = 0f;
				BlockEntity._effect.Parameters["EyePosition"].SetValue(translation);
				BlockEntity._effect.CurrentTechnique = BlockEntity._effect.Techniques[6];
				BlockEntity._effect.CurrentTechnique.Passes[type.NeedsFancyLighting ? 3 : 2].Apply();
			}
			else if (CastleMinerZGame.Instance.DrawingReflection)
			{
				BlockEntity._effect.CurrentTechnique = BlockEntity._effect.Techniques[4];
				BlockEntity._effect.CurrentTechnique.Passes[2].Apply();
			}
			else
			{
				BlockEntity._effect.CurrentTechnique = BlockEntity._effect.Techniques[BlockTerrain.Instance.IsWaterWorld ? 4 : 6];
				BlockEntity._effect.CurrentTechnique.Passes[type.NeedsFancyLighting ? 1 : 0].Apply();
			}
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
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
