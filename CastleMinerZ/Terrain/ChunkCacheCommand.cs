using System;
using DNA.CastleMinerZ.Utils;

namespace DNA.CastleMinerZ.Terrain
{
	public class ChunkCacheCommand : IReleaseable, ILinkedListNode
	{
		public ChunkCacheCommand()
		{
			this._objID = ChunkCacheCommand._nextObjID++;
		}

		public static ChunkCacheCommand Alloc()
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand._cache.Get();
			chunkCacheCommand._status = ChunkCacheCommandStatus.NEW;
			chunkCacheCommand._trackingString = null;
			return chunkCacheCommand;
		}

		public bool CopyRequestersToMe(ChunkCacheCommand src)
		{
			bool flag = false;
			for (int i = 0; i < src._numRequesters; i++)
			{
				bool flag2 = false;
				for (int j = 0; j < this._numRequesters; j++)
				{
					if (this._requesterIDs[j] == src._requesterIDs[i])
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = true;
					this._requesterIDs[this._numRequesters++] = src._requesterIDs[i];
				}
			}
			return flag;
		}

		public void Release()
		{
			this._delta = null;
			this._callback = null;
			this._context = null;
			this._data1 = null;
			this._data2 = null;
			this._priority = 1;
			this._consolidate = false;
			this._status = ChunkCacheCommandStatus.DONE;
			this._numRequesters = 0;
			ChunkCacheCommand._cache.Put(this);
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

		public ChunkCacheCommandEnum _command;

		public IntVector3 _worldPosition = IntVector3.Zero;

		public BlockTypeEnum _blockType;

		public ChunkCacheCommandDelegate _callback;

		public object _context;

		public byte[] _data1;

		public byte[] _data2;

		public string _trackingString;

		public long _submittedTime;

		public int _priority = 1;

		public byte _requesterID;

		public bool _consolidate;

		public byte[] _requesterIDs = new byte[16];

		public int _numRequesters;

		public volatile ChunkCacheCommandStatus _status;

		private static int _nextObjID = 0;

		public int _objID;

		public int[] _delta;

		private static ObjectCache<ChunkCacheCommand> _cache = new ObjectCache<ChunkCacheCommand>();

		private ILinkedListNode _nextNode;
	}
}
