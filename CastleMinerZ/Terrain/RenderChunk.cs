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
			int nindex = 0;
			for (int i = 0; i < 4; i++)
			{
				float count = 0f;
				float sunlight = 0f;
				float torchlight = 0f;
				int cornerCount = 0;
				for (int j = 0; j < 4; j++)
				{
					int cur = RenderChunk._lightNeighborIndexes[nindex++];
					if (j != 3 || cornerCount != 2)
					{
						if (sun[cur] >= 0f)
						{
							count += 1f;
							sunlight += sun[cur];
							torchlight += torch[cur];
						}
					}
					else
					{
						cornerCount++;
					}
				}
				if (count == 0f)
				{
					vxsun[i] = 0;
					vxtorch[i] = 0;
				}
				else
				{
					float k = sunlight / (count * 15f);
					vxsun[i] = (int)Math.Floor((double)(k * 255f + 0.5f));
					k = torchlight / (count * 15f);
					vxtorch[i] = (int)Math.Floor((double)(k * 255f + 0.5f));
				}
			}
		}

		public void MakeFace(IntVector3 iv, IntVector3 chunkLocal, IntVector3 local, BlockFace face, BlockType t, BlockBuildData bd, int block)
		{
			bool fancy = t.NeedsFancyLighting;
			int aoface = 0;
			bd._min.SetToMin(iv);
			bd._max.SetToMax(iv);
			BlockTerrain.Instance.FillFaceLightTable(local, face, ref bd._sun, ref bd._torch);
			if (t.LightAsTranslucent)
			{
				float sl = (float)Block.GetSunLightLevel(block);
				float tl = (float)Block.GetTorchLightLevel(block);
				for (int i = 0; i < 9; i++)
				{
					bd._sun[i] = Math.Max(bd._sun[i], sl);
					bd._torch[i] = Math.Max(bd._torch[i], tl);
				}
			}
			for (int j = 0; j < 9; j++)
			{
				if (bd._sun[j] < 0f)
				{
					if (j < 4)
					{
						aoface |= 1 << j;
					}
					else if (j > 4)
					{
						aoface |= 1 << j - 1;
					}
				}
			}
			this.FillFaceColors(ref bd._vxsun, ref bd._vxtorch, ref bd._sun, ref bd._torch);
			for (int k = 0; k < 4; k++)
			{
				bd.AddVertex(new BlockVertex(chunkLocal, face, k, t, bd._vxsun[k], bd._vxtorch[k], aoface), fancy);
			}
		}

		public void AddBlock(IntVector3 iv, BlockBuildData bd)
		{
			int b = BlockTerrain.Instance.GetSafeBlockAtABS(iv);
			BlockType t = Block.GetType(b);
			bool interiorFaces = t.InteriorFaces;
			if (t[BlockFace.NEGY] == -1)
			{
				return;
			}
			IntVector3 localIV = IntVector3.Subtract(iv, BlockTerrain.Instance._worldMin);
			IntVector3 chunkLocalIV = IntVector3.Subtract(iv, this._worldMin);
			if (!Block.HasAlpha(b))
			{
				for (BlockFace i = BlockFace.POSX; i < BlockFace.NUM_FACES; i++)
				{
					IntVector3 ni = IntVector3.Add(localIV, BlockTerrain._faceNeighbors[(int)i]);
					int blockIndex = ni.Y.Clamp(0, 127) + ni.X.Clamp(0, 383) * 128 + ni.Z.Clamp(0, 383) * 128 * 384;
					if (Block.HasAlpha(BlockTerrain.Instance._blocks[blockIndex]))
					{
						this.MakeFace(iv, chunkLocalIV, localIV, i, t, bd, b);
					}
				}
				return;
			}
			if (interiorFaces)
			{
				for (BlockFace j = BlockFace.POSX; j < BlockFace.NUM_FACES; j++)
				{
					this.MakeFace(iv, chunkLocalIV, localIV, j, t, bd, b);
					IntVector3 ni2 = IntVector3.Add(localIV, BlockTerrain._faceNeighbors[(int)j]);
					int blockIndex2 = ni2.Y.Clamp(0, 127) + ni2.X.Clamp(0, 383) * 128 + ni2.Z.Clamp(0, 383) * 128 * 384;
					int k = BlockTerrain.Instance._blocks[blockIndex2];
					BlockType nt = Block.GetType(k);
					if (nt[BlockFace.NEGY] == -1)
					{
						this.MakeFace(IntVector3.Add(iv, BlockTerrain._faceNeighbors[(int)j]), IntVector3.Add(chunkLocalIV, BlockTerrain._faceNeighbors[(int)j]), ni2, Block.OpposingFace[(int)j], t, bd, k);
					}
				}
				return;
			}
			for (BlockFace l = BlockFace.POSX; l < BlockFace.NUM_FACES; l++)
			{
				IntVector3 ni3 = IntVector3.Add(localIV, BlockTerrain._faceNeighbors[(int)l]);
				int blockIndex3 = ni3.Y.Clamp(0, 127) + ni3.X.Clamp(0, 383) * 128 + ni3.Z.Clamp(0, 383) * 128 * 384;
				if (Block.GetType(BlockTerrain.Instance._blocks[blockIndex3]) != t)
				{
					this.MakeFace(iv, chunkLocalIV, localIV, l, t, bd, b);
				}
			}
		}

		public bool BuildFaces(GraphicsDevice gd)
		{
			BlockBuildData bd = BlockBuildData.Alloc();
			IntVector3 offset = this._worldMin;
			IntVector3 lasts = IntVector3.Add(new IntVector3(16, 128, 16), this._worldMin);
			offset.Y = lasts.Y - 1;
			while (offset.Y >= this._worldMin.Y)
			{
				if (!BlockTerrain.Instance.IsReady)
				{
					bd.Release();
					return false;
				}
				offset.Z = this._worldMin.Z;
				while (offset.Z < lasts.Z)
				{
					offset.X = this._worldMin.X;
					while (offset.X < lasts.X)
					{
						this.AddBlock(offset, bd);
						offset.X++;
					}
					offset.Z++;
				}
				offset.Y--;
			}
			if (bd.HasVertexes)
			{
				if (this._vbs == null)
				{
					this._vbs = new List<VertexBufferKeeper>();
				}
				if (this._fancyVBs == null)
				{
					this._fancyVBs = new List<VertexBufferKeeper>();
				}
				bd._max = IntVector3.Add(new IntVector3(1, 1, 1), bd._max);
				IntVector3.FillBoundingCorners(bd._min, bd._max, ref this._boundingCorners);
				this._pendingBuildData = bd;
				return true;
			}
			bd.Release();
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
					VertexBuffer vb = this._vbs[i].Buffer;
					int numVertexes = this._vbs[i].NumVertexesUsed;
					gd.SetVertexBuffer(vb);
					effect.CurrentTechnique.Passes[2].Apply();
					gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexes, 0, numVertexes / 2);
				}
			}
			if (this._fancyVBs != null && this._fancyVBs.Count > 0)
			{
				for (int j = 0; j < this._fancyVBs.Count; j++)
				{
					VertexBuffer vb2 = this._fancyVBs[j].Buffer;
					int numVertexes2 = this._fancyVBs[j].NumVertexesUsed;
					gd.SetVertexBuffer(vb2);
					effect.CurrentTechnique.Passes[2].Apply();
					gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexes2, 0, numVertexes2 / 2);
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
						VertexBuffer vb = this._fancyVBs[i].Buffer;
						int numVertexes = this._fancyVBs[i].NumVertexesUsed;
						gd.SetVertexBuffer(vb);
						effect.CurrentTechnique.Passes[1].Apply();
						gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexes, 0, numVertexes / 2);
					}
					return;
				}
			}
			else if (this._vbs != null && this._vbs.Count > 0)
			{
				for (int j = 0; j < this._vbs.Count; j++)
				{
					VertexBuffer vb2 = this._vbs[j].Buffer;
					int numVertexes2 = this._vbs[j].NumVertexesUsed;
					gd.SetVertexBuffer(vb2);
					effect.CurrentTechnique.Passes[0].Apply();
					gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertexes2, 0, numVertexes2 / 2);
				}
			}
		}

		public static RenderChunk Alloc()
		{
			RenderChunk result = RenderChunk._cache.Get();
			result._refcount = 1;
			return result;
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
