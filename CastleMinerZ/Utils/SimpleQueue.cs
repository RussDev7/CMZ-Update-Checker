using System;

namespace DNA.CastleMinerZ.Utils
{
	public class SimpleQueue<T> where T : class, ILinkedListNode
	{
		public virtual void Queue(T obj)
		{
			if (this._front == null)
			{
				this._back = obj;
				this._front = obj;
				obj.NextNode = null;
			}
			else
			{
				this._back.NextNode = obj;
				this._back = obj;
				obj.NextNode = null;
			}
			this._count++;
		}

		private void InnerReplace(SimpleQueue<T> q)
		{
			this._front = q._front;
			this._back = q._back;
			this._count = q._count;
			q._front = default(T);
			q._back = default(T);
			q._count = 0;
		}

		public virtual void ReplaceFromList(T root)
		{
			this._front = root;
			this._back = default(T);
			this._count = 0;
			while (root != null)
			{
				this._back = root;
				this._count++;
				root = (T)((object)root.NextNode);
			}
		}

		public virtual void ReplaceContentsWith(SimpleQueue<T> q)
		{
			if (q is SynchronizedQueue<T>)
			{
				lock (q)
				{
					this.InnerReplace(q);
					return;
				}
			}
			this.InnerReplace(q);
		}

		public virtual T Clear()
		{
			T front = this._front;
			this._back = default(T);
			this._front = default(T);
			this._count = 0;
			return front;
		}

		public T Front
		{
			get
			{
				return this._front;
			}
		}

		public T Back
		{
			get
			{
				return this._back;
			}
		}

		public bool Empty
		{
			get
			{
				return this._front == null;
			}
		}

		public int Count
		{
			get
			{
				return this._count;
			}
		}

		public int IncrementCountAfterInsertion()
		{
			return ++this._count;
		}

		public int DecrementCountAfterDeletion()
		{
			return --this._count;
		}

		public virtual T Dequeue()
		{
			T front = this._front;
			if (this._front != null)
			{
				this._front = (T)((object)this._front.NextNode);
				if (this._front == null)
				{
					this._back = default(T);
				}
				this._count--;
			}
			if (front != null)
			{
				front.NextNode = null;
			}
			return front;
		}

		public virtual void Undequeue(T obj)
		{
			obj.NextNode = this._front;
			this._front = obj;
			if (this._back == null)
			{
				this._back = this._front;
			}
			this._count++;
		}

		public virtual void Remove(T obj)
		{
			if (this._front == obj)
			{
				this._front = (T)((object)this._front.NextNode);
				if (this._front == null)
				{
					this._back = default(T);
				}
				this._count--;
			}
			else
			{
				T t = this._front;
				while (t.NextNode != null)
				{
					if (t.NextNode == obj)
					{
						t.NextNode = obj.NextNode;
						if (this._back == obj)
						{
							this._back = t;
						}
						this._count--;
						break;
					}
					t = (T)((object)t.NextNode);
				}
			}
			obj.NextNode = null;
		}

		private T _back = default(T);

		private T _front = default(T);

		private int _count;
	}
}
