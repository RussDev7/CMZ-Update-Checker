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
					BlockVertex[] array = new BlockVertex[this._fancyVXBufferSize + 100];
					this._fancyVXList.CopyTo(array, 0);
					this._fancyVXList = array;
					this._fancyVXBufferSize += 100;
				}
				this._fancyVXList[this._fancyVXCount++] = bv;
				return;
			}
			if (this._vxCount == this._vxBufferSize)
			{
				BlockVertex[] array2 = new BlockVertex[this._vxBufferSize + 100];
				this._vxList.CopyTo(array2, 0);
				this._vxList = array2;
				this._vxBufferSize += 100;
			}
			this._vxList[this._vxCount++] = bv;
		}

		public void BuildVBs(GraphicsDevice gd, ref List<VertexBufferKeeper> vbs, ref List<VertexBufferKeeper> fancy)
		{
			if (this._vxCount > 0)
			{
				int num = 0;
				int num2 = this._vxCount;
				int num3 = 0;
				while (num2 != 0)
				{
					int num4 = ((num2 > 16384) ? 16384 : num2);
					num2 -= num4;
					if (gd.IsDisposed)
					{
						return;
					}
					VertexBufferKeeper vertexBufferKeeper = VertexBufferKeeper.Alloc(num4);
					if (vertexBufferKeeper.Buffer != null && vertexBufferKeeper.Buffer.VertexCount < num4)
					{
						vertexBufferKeeper.Buffer.Dispose();
						vertexBufferKeeper.Buffer = null;
					}
					bool flag = false;
					do
					{
						if (GraphicsDeviceLocker.Instance.TryLockDevice())
						{
							try
							{
								if (vertexBufferKeeper.Buffer == null)
								{
									vertexBufferKeeper.Buffer = new VertexBuffer(gd, typeof(BlockVertex), num4, BufferUsage.WriteOnly);
								}
								VertexBuffer buffer = vertexBufferKeeper.Buffer;
								buffer.SetData<BlockVertex>(this._vxList, num, num4);
							}
							finally
							{
								GraphicsDeviceLocker.Instance.UnlockDevice();
							}
							flag = true;
						}
						if (!flag)
						{
							Thread.Sleep(10);
						}
					}
					while (!flag);
					vertexBufferKeeper.NumVertexesUsed = num4;
					num += num4;
					vbs.Add(vertexBufferKeeper);
					num3++;
				}
			}
			if (this._fancyVXCount > 0)
			{
				int num5 = 0;
				int num6 = this._fancyVXCount;
				int num7 = 0;
				while (num6 != 0)
				{
					int num8 = ((num6 > 16384) ? 16384 : num6);
					num6 -= num8;
					if (gd.IsDisposed)
					{
						return;
					}
					VertexBufferKeeper vertexBufferKeeper2 = VertexBufferKeeper.Alloc(num8);
					if (vertexBufferKeeper2.Buffer != null && vertexBufferKeeper2.Buffer.VertexCount < num8)
					{
						vertexBufferKeeper2.Buffer.Dispose();
						vertexBufferKeeper2.Buffer = null;
					}
					bool flag2 = false;
					do
					{
						if (GraphicsDeviceLocker.Instance.TryLockDevice())
						{
							try
							{
								if (vertexBufferKeeper2.Buffer == null)
								{
									vertexBufferKeeper2.Buffer = new VertexBuffer(gd, typeof(BlockVertex), num8, BufferUsage.WriteOnly);
								}
								VertexBuffer buffer2 = vertexBufferKeeper2.Buffer;
								buffer2.SetData<BlockVertex>(this._fancyVXList, num5, num8);
							}
							finally
							{
								GraphicsDeviceLocker.Instance.UnlockDevice();
							}
							flag2 = true;
						}
						if (!flag2)
						{
							Thread.Sleep(10);
						}
					}
					while (!flag2);
					vertexBufferKeeper2.NumVertexesUsed = num8;
					fancy.Add(vertexBufferKeeper2);
					num7++;
				}
			}
		}

		public static BlockBuildData Alloc()
		{
			BlockBuildData blockBuildData = BlockBuildData._cache.Get();
			blockBuildData._min.SetValues(int.MaxValue, int.MaxValue, int.MaxValue);
			blockBuildData._max.SetValues(int.MinValue, int.MinValue, int.MinValue);
			blockBuildData._vxCount = 0;
			blockBuildData._fancyVXCount = 0;
			return blockBuildData;
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
