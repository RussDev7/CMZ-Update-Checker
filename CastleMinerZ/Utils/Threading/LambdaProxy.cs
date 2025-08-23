using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public class LambdaProxy : ILinkedListNode, IReleaseable
	{
		public void Execute(BaseTask t, object c)
		{
			if (this._oneArgumentCallback != null)
			{
				this._oneArgumentCallback(c);
			}
			else
			{
				this._zeroArgumentCallback();
			}
			this.Release();
		}

		public static LambdaProxy Alloc(WaitCallback callback)
		{
			LambdaProxy result = LambdaProxy._cache.Get();
			result._oneArgumentCallback = callback;
			return result;
		}

		public static LambdaProxy Alloc(ThreadStart callback)
		{
			LambdaProxy result = LambdaProxy._cache.Get();
			result._zeroArgumentCallback = callback;
			return result;
		}

		public void Release()
		{
			this._oneArgumentCallback = null;
			this._zeroArgumentCallback = null;
			LambdaProxy._cache.Put(this);
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

		private WaitCallback _oneArgumentCallback;

		private ThreadStart _zeroArgumentCallback;

		private static ObjectCache<LambdaProxy> _cache = new ObjectCache<LambdaProxy>();

		private ILinkedListNode _nextNode;
	}
}
