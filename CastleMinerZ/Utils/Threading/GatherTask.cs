using System;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public sealed class GatherTask : BaseTask
	{
		public bool Rush
		{
			get
			{
				return this._rush;
			}
			set
			{
				this._rush = value;
			}
		}

		public override void DoWork(TaskThread thread)
		{
			base.DoWork(thread);
			this.Release();
		}

		public TaskThreadEnum DesiredThreadIndex
		{
			get
			{
				return this._desiredThreadIndex;
			}
			set
			{
				this._desiredThreadIndex = value;
			}
		}

		private void InsertTask(BaseTask task)
		{
			this._children.Queue(task);
		}

		public void SetCount(int c)
		{
			this._count.Value = c;
		}

		public void AddTask(TaskDelegate work, object context)
		{
			Task task = Task.Alloc();
			task.Init(work, context, this);
			this.InsertTask(task);
		}

		public WaitableTask AddWaitableTask(TaskDelegate work, object context)
		{
			WaitableTask waitableTask = WaitableTask.Alloc();
			waitableTask.Init(work, context, this);
			this.InsertTask(waitableTask);
			return waitableTask;
		}

		public void Start()
		{
			this._count.Value = this._children.Count;
			while (!this._children.Empty)
			{
				BaseTask baseTask = this._children.Dequeue();
				if (this._rush)
				{
					TaskDispatcher.Instance.AddRushTask(baseTask);
				}
				else
				{
					TaskDispatcher.Instance.AddTask(baseTask);
				}
			}
		}

		public void StartNow()
		{
			this._rush = true;
			this.Start();
		}

		public void ChildFinished()
		{
			if (this._count.Decrement() == 0)
			{
				if (this._rush)
				{
					TaskDispatcher.Instance.AddRushTask(this);
					return;
				}
				TaskDispatcher.Instance.AddTask(this._desiredThreadIndex, this);
			}
		}

		public static GatherTask Alloc()
		{
			GatherTask gatherTask = GatherTask._cache.Get();
			gatherTask._desiredThreadIndex = TaskThreadEnum.THREAD_ANY;
			gatherTask._count.Value = 0;
			gatherTask._rush = false;
			return gatherTask;
		}

		public override void Release()
		{
			base.Release();
			GatherTask._cache.Put(this);
		}

		private CountdownLatch _count = default(CountdownLatch);

		private TaskThreadEnum _desiredThreadIndex = TaskThreadEnum.THREAD_ANY;

		private bool _rush;

		private SimpleQueue<BaseTask> _children = new SimpleQueue<BaseTask>();

		private static ObjectCache<GatherTask> _cache = new ObjectCache<GatherTask>();
	}
}
