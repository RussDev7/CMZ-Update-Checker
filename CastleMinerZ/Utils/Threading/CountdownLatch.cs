using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public struct CountdownLatch
	{
		public static implicit operator bool(CountdownLatch latch)
		{
			return latch.Value != 0;
		}

		public int Increment()
		{
			return Interlocked.Increment(ref this._count);
		}

		public int Value
		{
			get
			{
				return this._count;
			}
			set
			{
				Interlocked.Exchange(ref this._count, value);
			}
		}

		public int Decrement()
		{
			return Interlocked.Decrement(ref this._count);
		}

		private int _count;
	}
}
