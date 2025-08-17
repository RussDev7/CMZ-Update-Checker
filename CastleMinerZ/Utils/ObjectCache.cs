using System;

namespace DNA.CastleMinerZ.Utils
{
	public class ObjectCache<iType> where iType : class, ILinkedListNode, new()
	{
		public int GrowSize
		{
			get
			{
				return this._growSize;
			}
			set
			{
				if (value > 0)
				{
					this._growSize = value;
					return;
				}
				throw new ArgumentException("PartCache.GrowSize must be a positive integer");
			}
		}

		private void GrowList(int size)
		{
			for (int i = 0; i < size; i++)
			{
				this._cache.Push(new iType());
			}
		}

		public iType Get()
		{
			iType iType = default(iType);
			iType = this._cache.Pop();
			if (iType == null)
			{
				this.GrowList(this._growSize);
				for (iType = this._cache.Pop(); iType == null; iType = this._cache.Pop())
				{
					this._cache.Push(new iType());
				}
			}
			return iType;
		}

		public void Put(iType part)
		{
			if (part != null)
			{
				this._cache.Push(part);
			}
		}

		public void PutList(iType list)
		{
			if (list != null)
			{
				this._cache.PushList(list);
			}
		}

		private int _growSize = 5;

		private LockFreeStack<iType> _cache = new LockFreeStack<iType>();
	}
}
