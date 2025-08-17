using System;

namespace DNA.CastleMinerZ.Utils
{
	public class SynchronizedQueue<T> : SimpleQueue<T> where T : class, ILinkedListNode
	{
		public override void Queue(T obj)
		{
			lock (this)
			{
				base.Queue(obj);
			}
		}

		public override T Clear()
		{
			T t;
			lock (this)
			{
				t = base.Clear();
			}
			return t;
		}

		public override T Dequeue()
		{
			T t;
			lock (this)
			{
				t = base.Dequeue();
			}
			return t;
		}

		public override void Undequeue(T obj)
		{
			lock (this)
			{
				base.Undequeue(obj);
			}
		}

		public override void Remove(T obj)
		{
			lock (this)
			{
				base.Remove(obj);
			}
		}

		public override void ReplaceContentsWith(SimpleQueue<T> q)
		{
			lock (this)
			{
				base.ReplaceContentsWith(q);
			}
		}

		public override void ReplaceFromList(T root)
		{
			lock (this)
			{
				base.ReplaceFromList(root);
			}
		}
	}
}
