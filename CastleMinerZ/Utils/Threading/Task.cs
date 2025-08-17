using System;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public sealed class Task : BaseTask
	{
		public override void DoWork(TaskThread thread)
		{
			base.DoWork(thread);
			this.Release();
		}

		public static Task Alloc()
		{
			return Task._cache.Get();
		}

		public override void Release()
		{
			base.Release();
			Task._cache.Put(this);
		}

		private static ObjectCache<Task> _cache = new ObjectCache<Task>();
	}
}
