using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DNA.CastleMinerZ.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Terrain
{
	public class RenderChunk : IReleaseable, ILinkedListNode
	{
		public int AddRef()
		{
			return Interlocked.Increment(ref this._refcount);
		}

		public void FillFaceColors(ref int[] vxsun, ref int[] vxtorch, ref float[] sun, ref float[] torch)
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				float num2 = 0f;
				float num3 = 0f;
				float num4 = 0f;
				int num5 = 0;
				for (int j = 0; j < 4; j++)
				{
					int num6 = RenderChunk._lightNeighborIndexes[num++];
					if (j != 3 || num5 != 2)
					{
						if (sun[num6] >= 0f)
						{
							num2 += 1f;
							num3 += sun[num6];
							num4 += torch[num6];
						}
					}
					else
					{
						num5++;
					}
				}
				if (num2 == 0f)
				{
					vxsun[i] = 0;
					vxtorch[i] = 0;
				}
				else
				{
					float num7 = num3 / (num2 * 15f);
					vxsun[i] = (int)Math.Floor((double)(num7 * 255f + 0.5f));
					num7 = num4 / (num2 * 15f);
					vxtorch[i] = (int)Math.Floor((double)(num7 * 255f + 0.5f));
				}
			}
		}

		public void MakeFace(IntVector3 iv, IntVector3 chunkLocal, IntVector3 local, BlockFace face, BlockType t, BlockBuildData bd, int block)
		{
			bool needsFancyLighting = t.NeedsFancyLighting;
			int num = 0;
			bd._min.SetToMin(iv);
			bd._max.SetToMax(iv);
			BlockTerrain.Instance.FillFaceLightTable(local, face, ref bd._sun, ref bd._torch);
			if (t.LightAsTranslucent)
			{
				float num2 = (float)Block.GetSunLightLevel(block);
				float num3 = (float)Block.GetTorchLightLevel(block);
				for (int i = 0; i < 9; i++)
				{
					bd._sun[i] = Math.Max(bd._sun[i], num2);
					bd._torch[i] = Math.Max(bd._torch[i], num3);
				}
			}
			for (int j = 0; j < 9; j++)
			{
				if (bd._sun[j] < 0f)
				{
					if (j < 4)
					{
						num |= 1 << j;
					}
					else if (j > 4)
					{
						num |= 1 << j - 1;
					}
				}
			}
			this.FillFaceColors(ref bd._vxsun, ref bd._vxtorch, ref bd._sun, ref bd._torch);
			for (int k = 0; k < 4; k++)
			{
				bd.AddVertex(new BlockVertex(chunkLocal, face, k, t, bd._vxsun[k], bd._vxtorch[k], num), needsFancyLighting);
			}
		}

		public void AddBlock(IntVector3 iv, BlockBuildData bd)
		{
			int safeBlockAtABS = BlockTerrain.Instance.GetSafeBlockAtABS(iv);
			BlockType type = Block.GetType(safeBlockAtABS);
			bool interiorFaces = type.InteriorFaces;
			if (type[BlockFace.NEGY] == -1)
			{
				return;
			}
			IntVector3 intVector = IntVector3.Subtract(iv, BlockTerrain.Instance._worldMin);
			IntVector3 intVector2 = IntVector3.Subtract(iv, this._worldMin);
			if (!Block.HasAlpha(safeBlockAtABS))
			{
				for (BlockFace blockFace = BlockFace.POSX; blockFace < BlockFace.NUM_FACES; blockFace++)
				{
					IntVector3 intVector3 = IntVector3.Add(intVector, BlockTerrain._faceNeighbors[(int)blockFace]);
					int num = intVector3.Y.Clamp(0, 127) + intVector3.X.Clamp(0, 383) * 128 + intVector3.Z.Clamp(0, 383) * 128 * 384;
					if (Block.HasAlpha(BlockTerrain.Instance._blocks[num]))
					{
						this.MakeFace(iv, intVector2, intVector, blockFace, type, bd, safeBlockAtABS);
					}
				}
				return;
			}
			if (interiorFaces)
			{
				for (BlockFace blockFace2 = BlockFace.POSX; blockFace2 < BlockFace.NUM_FACES; blockFace2++)
				{
					this.MakeFace(iv, intVector2, intVector, blockFace2, type, bd, safeBlockAtABS);
					IntVector3 intVector4 = IntVector3.Add(intVector, BlockTerrain._faceNeighbors[(int)blockFace2]);
					int num2 = intVector4.Y.Clamp(0, 127) + intVector4.X.Clamp(0, 383) * 128 + intVector4.Z.Clamp(0, 383) * 128 * 384;
					int num3 = BlockTerrain.Instance._blocks[num2];
					BlockType type2 = Block.GetType(num3);
					if (type2[BlockFace.NEGY] == -1)
					{
						this.MakeFace(IntVector3.Add(iv, BlockTerrain._faceNeighbors[(int)blockFace2]), IntVector3.Add(intVector2, BlockTerrain._faceNeighbors[(int)blockFace2]), intVector4, Block.OpposingFace[(int)blockFace2], type, bd, num3);
					}
				}
				return;
			}
			for (BlockFace blockFace3 = BlockFace.POSX; blockFace3 < BlockFace.NUM_FACES; blockFace3++)
			{
				IntVector3 intVector5 = IntVector3.Add(intVector, BlockTerrain._faceNeighbors[(int)blockFace3]);
				int num4 = intVector5.Y.Clamp(0, 127) + intVector5.X.Clamp(0, 383) * 128 + intVector5.Z.Clamp(0, 383) * 128 * 384;
				if (Block.GetType(BlockTerrain.Instance._blocks[num4]) != type)
				{
					this.MakeFace(iv, intVector2, intVector, blockFace3, type, bd, safeBlockAtABS);
				}
			}
		}

		public bool BuildFaces(GraphicsDevice gd)
		{
			BlockBuildData blockBuildData = BlockBuildData.Alloc();
			IntVector3 worldMin = this._worldMin;
			IntVector3 intVector = IntVector3.Add(new IntVector3(16, 128, 16), this._worldMin);
			worldMin.Y = intVector.Y - 1;
			while (worldMin.Y >= this._worldMin.Y)
			{
				if (!BlockTerrain.Instance.IsReady)
				{
					blockBuildData.Release();
					return false;
				}
				worldMin.Z = this._worldMin.Z;
				while (worldMin.Z < intVector.Z)
				{
					worldMin.X = this._worldMin.X;
					while (worldMin.X < intVector.X)
					{
						this.AddBlock(worldMin, blockBuildData);
						worldMin.X++;
					}
					worldMin.Z++;
				}
				worldMin.Y--;
			}
			if (blockBuildData.HasVertexes)
			{
				if (this._vbs == null)
				{
					this._vbs = new List<VertexBufferKeeper>();
				}
				if (this._fancyVBs == null)
				{
					this._fancyVBs = new List<VertexBufferKeeper>();
				}
				blockBuildData._max = IntVector3.Add(new IntVector3(1, 1, 1), blockBuildData._max);
				IntVector3.FillBoundingCorners(blockBuildData._min, blockBuildData._max, ref this._boundingCorners);
				this._pendingBuildData = blockBuildData;
				return true;
			}
			blockBuildData.Release();
			return false;
		}

		public void SkipBuildingBuffers()
		{
			this._pendingBuildData.Release();
			this._pendingBuildData = null;
		}

		public void FinishBuildingBuffers(GraphicsDevice gd)
		{
			this._pendingBuildData.BuildVBs(gd, ref this._vbs, ref this._fancyVBs);
			this._pendingBuildData.Release();
			this._pendingBuildData = null;
		}

		public bool ChunkIsOutsidePlane(Plane plane)
		{
			for (int i = 0; i < 8; i++)
			{
				if (plane.DotCoordinate(this._boundingCorners[i]) <= 0.001f)
				{
					return false;
				}
			}
			return true;
		}

		public bool HasGeometry()
		{
			return (this._vbs != null && this._vbs.Count != 0) || (this._fancyVBs != null && this._fancyVBs.Count != 0);
		}

		public bool TouchesFrustum(BoundingFrustum frustum)
		{
			return this.HasGeometry() && (!this.ChunkIsOutsidePlane(frustum.Near) && !this.ChunkIsOutsidePlane(frustum.Far) && !this.ChunkIsOutsidePlane(frustum.Left) && !this.ChunkIsOutsidePlane(frustum.Right) && !this.ChunkIsOutsidePlane(frustum.Bottom) && !this.ChunkIsOutsidePlane(frustum.Top));
		}

		public void DrawReflection(GraphicsDevice gd, Effect effect, BoundingFrustum frustum)
		{
			effect.Parameters["WorldBase"].SetValue(this._basePosition);
			if (this._vbs != null && this._vbs.Count > 0)
			{
				for (int i = 0; i < this._vbs.Count; i++)
				{
					VertexBuffer buffer = this._vbs[i].Buffer;
					int numVertexesUsed = this._vbs[i].NumVertexesUsed;
					gd.SetVertexBuffer(buffer);
					effect.CurrentTechnique.Passes[2].Apply();
					gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexesUsed, 0, numVertexesUsed / 2);
				}
			}
			if (this._fancyVBs != null && this._fancyVBs.Count > 0)
			{
				for (int j = 0; j < this._fancyVBs.Count; j++)
				{
					VertexBuffer buffer2 = this._fancyVBs[j].Buffer;
					int numVertexesUsed2 = this._fancyVBs[j].NumVertexesUsed;
					gd.SetVertexBuffer(buffer2);
					effect.CurrentTechnique.Passes[2].Apply();
					gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexesUsed2, 0, numVertexesUsed2 / 2);
				}
			}
		}

		public void Draw(GraphicsDevice gd, Effect effect, bool fancy, BoundingFrustum frustum)
		{
			effect.Parameters["WorldBase"].SetValue(this._basePosition);
			if (fancy)
			{
				if (this._fancyVBs != null && this._fancyVBs.Count > 0)
				{
					for (int i = 0; i < this._fancyVBs.Count; i++)
					{
						VertexBuffer buffer = this._fancyVBs[i].Buffer;
						int numVertexesUsed = this._fancyVBs[i].NumVertexesUsed;
						gd.SetVertexBuffer(buffer);
						effect.CurrentTechnique.Passes[1].Apply();
						gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexesUsed, 0, numVertexesUsed / 2);
					}
					return;
				}
			}
			else if (this._vbs != null && this._vbs.Count > 0)
			{
				for (int j = 0; j < this._vbs.Count; j++)
				{
					VertexBuffer buffer2 = this._vbs[j].Buffer;
					int numVertexesUsed2 = this._vbs[j].NumVertexesUsed;
					gd.SetVertexBuffer(buffer2);
					effect.CurrentTechnique.Passes[0].Apply();
					gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexesUsed2, 0, numVertexesUsed2 / 2);
				}
			}
		}

		public static RenderChunk Alloc()
		{
			RenderChunk renderChunk = RenderChunk._cache.Get();
			renderChunk._refcount = 1;
			return renderChunk;
		}

		public void Release()
		{
			if (Interlocked.Decrement(ref this._refcount) == 0)
			{
				if (this._vbs != null)
				{
					for (int i = 0; i < this._vbs.Count<VertexBufferKeeper>(); i++)
					{
						this._vbs[i].Release();
					}
					this._vbs.Clear();
				}
				if (this._fancyVBs != null)
				{
					for (int j = 0; j < this._fancyVBs.Count<VertexBufferKeeper>(); j++)
					{
						this._fancyVBs[j].Release();
					}
					this._fancyVBs.Clear();
				}
				RenderChunk._cache.Put(this);
			}
		}

		public ILinkedListNode NextNode
		{
			get
			{
				return this._nextNode;
			}
			set
			{
				this._nextNode = value;
			}
		}

		private static readonly int[] _lightNeighborIndexes = new int[]
		{
			4, 1, 3, 0, 4, 1, 5, 2, 4, 3,
			7, 6, 4, 5, 7, 8
		};

		public Vector3 _basePosition;

		public IntVector3 _worldMin;

		public Vector3[] _boundingCorners = new Vector3[8];

		public int _refcount = 1;

		private List<VertexBufferKeeper> _vbs;

		private List<VertexBufferKeeper> _fancyVBs;

		private BlockBuildData _pendingBuildData;

		private static readonly float[] faceBrightness = new float[] { 0.92f, 0.8367f, 0.92f, 0.8367f, 1f, 0.7071f, 0.92f, 0.8367f, 0.92f, 0.8367f };

		private static readonly float[] AOFACTOR = new float[]
		{
			0.4f, 0.4f, 0.4f, 0.4f, 0.3f, 0.4f, 0f, 0f, 0f, 0f,
			0f
		};

		private static ObjectCache<RenderChunk> _cache = new ObjectCache<RenderChunk>();

		private ILinkedListNode _nextNode;
	}
}
