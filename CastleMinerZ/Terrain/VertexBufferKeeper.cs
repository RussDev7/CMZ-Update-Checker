using System;
using DNA.CastleMinerZ.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Terrain
{
	public class VertexBufferKeeper : ILinkedListNode, IReleaseable
	{
		public static VertexBufferKeeper Alloc(int numNeeded)
		{
			VertexBufferKeeper vertexBufferKeeper;
			if (numNeeded < 2048)
			{
				vertexBufferKeeper = VertexBufferKeeper._smallBufferCache.Get();
			}
			else
			{
				vertexBufferKeeper = VertexBufferKeeper._largeBufferCache.Get();
			}
			return vertexBufferKeeper;
		}

		public void Release()
		{
			if (this.Buffer == null || this.Buffer.VertexCount < 2048)
			{
				VertexBufferKeeper._smallBufferCache.Put(this);
				return;
			}
			VertexBufferKeeper._largeBufferCache.Put(this);
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

		public VertexBuffer Buffer;

		public int NumVertexesUsed;

		private static ObjectCache<VertexBufferKeeper> _smallBufferCache = new ObjectCache<VertexBufferKeeper>();

		private static ObjectCache<VertexBufferKeeper> _largeBufferCache = new ObjectCache<VertexBufferKeeper>();

		private ILinkedListNode _nextNode;
	}
}
