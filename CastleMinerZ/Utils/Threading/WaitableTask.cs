using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public sealed class WaitableTask : BaseTask
	{
		public WaitableTask()
		{
			WaitableTask root;
			do
			{
				root = WaitableTask._waitables;
				this._nextWaitableTask = root;
			}
			while (root != Interlocked.CompareExchange<WaitableTask>(ref WaitableTask._waitables, this, root));
		}

		public override void Init(TaskDelegate work, object context)
		{
			base.Init(work, context);
			this._interrupted = false;
			this._finished.Reset();
		}

		public override void DoWork(TaskThread thread)
		{
			base.DoWork(thread);
			this._finished.Set();
		}

		public override void Interrupt()
		{
			base.Interrupt();
			this._finished.Set();
		}

		public bool Wait()
		{
			this._finished.WaitOne();
			return !this._interrupted;
		}

		public bool Done
		{
			get
			{
				return this._finished.WaitOne(0);
			}
		}

		public static void WakeAll()
		{
			for (WaitableTask walker = WaitableTask._waitables; walker != null; walker = walker._nextWaitableTask)
			{
				walker.Interrupt();
			}
		}

		public static WaitableTask Alloc()
		{
			return WaitableTask._cache.Get();
		}

		public override void Release()
		{
			base.Release();
			WaitableTask._cache.Put(this);
		}

		private ManualResetEvent _finished = new ManualResetEvent(true);

		public WaitableTask _nextWaitableTask;

		private volatile bool _interrupted;

		private static ObjectCache<WaitableTask> _cache = new ObjectCache<WaitableTask>();

		private static WaitableTask _waitables = null;
	}
}
