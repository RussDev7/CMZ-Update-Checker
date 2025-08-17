using System;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public class BaseTask : IReleaseable, ILinkedListNode
	{
		public virtual void Init(TaskDelegate work, object context, GatherTask parent)
		{
			this.Init(work, context);
			this._parent = parent;
			this._thread = null;
		}

		public virtual void Init(TaskDelegate work, object context)
		{
			if (work == null)
			{
				throw new ArgumentNullException("work", "Task initialized with null work delegate");
			}
			this._parent = null;
			this._workDelegate = work;
			this._context = context;
			this._thread = null;
		}

		public virtual void DoWork(TaskThread thread)
		{
			this._thread = thread;
			this._workDelegate(this, this._context);
			if (this._parent != null)
			{
				this._parent.ChildFinished();
			}
		}

		public virtual void YieldUntilIdle()
		{
			if (this._thread != null)
			{
				this._thread.DrainTaskList();
			}
		}

		public virtual void YieldCount(int count)
		{
			if (this._thread != null)
			{
				while (count != 0)
				{
					BaseTask task = this._thread.GetTask();
					if (task != null)
					{
						this._thread.DoTask(task);
						count--;
					}
					else
					{
						count = 0;
					}
				}
			}
		}

		public virtual void YieldOnce()
		{
			this.YieldCount(1);
		}

		public virtual void Interrupt()
		{
			this._interrupted = true;
		}

		public virtual bool Interrupted
		{
			get
			{
				return this._interrupted;
			}
		}

		public virtual void Release()
		{
			this._thread = null;
			this._context = null;
			this._workDelegate = null;
			this._parent = null;
			this._interrupted = false;
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

		public TaskDelegate _workDelegate;

		public GatherTask _parent;

		public TaskThread _thread;

		public object _context;

		private volatile bool _interrupted;

		public ILinkedListNode _nextNode;
	}
}
