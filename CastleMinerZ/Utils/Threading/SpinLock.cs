using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public struct SpinLock
	{
		public bool Locked
		{
			get
			{
				return this._lock != 0;
			}
		}

		public void Lock()
		{
			while (Interlocked.CompareExchange(ref this._lock, 1, 0) != 0)
			{
			}
		}

		public void Unlock()
		{
			this._lock = 0;
		}

		public bool TryLock()
		{
			return Interlocked.CompareExchange(ref this._lock, 1, 0) == 0;
		}

		private int _lock;
	}
}
