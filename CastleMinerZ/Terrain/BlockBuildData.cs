using System;
using System.Collections.Generic;
using System.Threading;
using DNA.CastleMinerZ.Utils;
using DNA.Drawing;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Terrain
{
	public class BlockBuildData : IReleaseable, ILinkedListNode
	{
		public bool HasVertexes
		{
			get
			{
				return this._vxCount != 0 || this._fancyVXCount != 0;
			}
		}

		public void AddVertex(BlockVertex bv, bool fancy)
		{
			if (fancy)
			{
				if (this._fancyVXCount == this._fancyVXBufferSize)
				{
					BlockVertex[] other = new BlockVertex[this._fancyVXBufferSize + 100];
					this._fancyVXList.CopyTo(other, 0);
					this._fancyVXList = other;
					this._fancyVXBufferSize += 100;
				}
				this._fancyVXList[this._fancyVXCount++] = bv;
				return;
			}
			if (this._vxCount == this._vxBufferSize)
			{
				BlockVertex[] other2 = new BlockVertex[this._vxBufferSize + 100];
				this._vxList.CopyTo(other2, 0);
				this._vxList = other2;
				this._vxBufferSize += 100;
			}
			this._vxList[this._vxCount++] = bv;
		}

		public void BuildVBs(GraphicsDevice gd, ref List<VertexBufferKeeper> vbs, ref List<VertexBufferKeeper> fancy)
		{
			if (this._vxCount > 0)
			{
				int baseVx = 0;
				int vxsLeft = this._vxCount;
				int count = 0;
				while (vxsLeft != 0)
				{
					int vxsThisTime = ((vxsLeft > 16384) ? 16384 : vxsLeft);
					vxsLeft -= vxsThisTime;
					if (gd.IsDisposed)
					{
						return;
					}
					VertexBufferKeeper vbk = VertexBufferKeeper.Alloc(vxsThisTime);
					if (vbk.Buffer != null && vbk.Buffer.VertexCount < vxsThisTime)
					{
						vbk.Buffer.Dispose();
						vbk.Buffer = null;
					}
					bool created = false;
					do
					{
						if (GraphicsDeviceLocker.Instance.TryLockDevice())
						{
							try
							{
								if (vbk.Buffer == null)
								{
									vbk.Buffer = new VertexBuffer(gd, typeof(BlockVertex), vxsThisTime, BufferUsage.WriteOnly);
								}
								VertexBuffer vb = vbk.Buffer;
								vb.SetData<BlockVertex>(this._vxList, baseVx, vxsThisTime);
							}
							finally
							{
								GraphicsDeviceLocker.Instance.UnlockDevice();
							}
							created = true;
						}
						if (!created)
						{
							Thread.Sleep(10);
						}
					}
					while (!created);
					vbk.NumVertexesUsed = vxsThisTime;
					baseVx += vxsThisTime;
					vbs.Add(vbk);
					count++;
				}
			}
			if (this._fancyVXCount > 0)
			{
				int baseVx2 = 0;
				int vxsLeft2 = this._fancyVXCount;
				int count2 = 0;
				while (vxsLeft2 != 0)
				{
					int vxsThisTime2 = ((vxsLeft2 > 16384) ? 16384 : vxsLeft2);
					vxsLeft2 -= vxsThisTime2;
					if (gd.IsDisposed)
					{
						return;
					}
					VertexBufferKeeper vbk2 = VertexBufferKeeper.Alloc(vxsThisTime2);
					if (vbk2.Buffer != null && vbk2.Buffer.VertexCount < vxsThisTime2)
					{
						vbk2.Buffer.Dispose();
						vbk2.Buffer = null;
					}
					bool created2 = false;
					do
					{
						if (GraphicsDeviceLocker.Instance.TryLockDevice())
						{
							try
							{
								if (vbk2.Buffer == null)
								{
									vbk2.Buffer = new VertexBuffer(gd, typeof(BlockVertex), vxsThisTime2, BufferUsage.WriteOnly);
								}
								VertexBuffer vb2 = vbk2.Buffer;
								vb2.SetData<BlockVertex>(this._fancyVXList, baseVx2, vxsThisTime2);
							}
							finally
							{
								GraphicsDeviceLocker.Instance.UnlockDevice();
							}
							created2 = true;
						}
						if (!created2)
						{
							Thread.Sleep(10);
						}
					}
					while (!created2);
					vbk2.NumVertexesUsed = vxsThisTime2;
					fancy.Add(vbk2);
					count2++;
				}
			}
		}

		public static BlockBuildData Alloc()
		{
			BlockBuildData result = BlockBuildData._cache.Get();
			result._min.SetValues(int.MaxValue, int.MaxValue, int.MaxValue);
			result._max.SetValues(int.MinValue, int.MinValue, int.MinValue);
			result._vxCount = 0;
			result._fancyVXCount = 0;
			return result;
		}

		public void Release()
		{
			BlockBuildData._cache.Put(this);
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

		private const int NUM_VERTS = 7000;

		public float[] _sun = new float[9];

		public float[] _torch = new float[9];

		public int[] _vxsun = new int[4];

		public int[] _vxtorch = new int[4];

		public IntVector3 _min = default(IntVector3);

		public IntVector3 _max = default(IntVector3);

		public BlockVertex[] _vxList = new BlockVertex[7000];

		private int _vxBufferSize = 7000;

		private int _vxCount;

		public BlockVertex[] _fancyVXList = new BlockVertex[7000];

		private int _fancyVXBufferSize = 7000;

		private int _fancyVXCount;

		private static ObjectCache<BlockBuildData> _cache = new ObjectCache<BlockBuildData>();

		private ILinkedListNode _nextNode;
	}
}
