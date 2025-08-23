using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public class TaskThread
	{
		public TaskThread(CountdownLatch latch, TaskThreadEnum idx)
		{
			this._initializedCountdown = latch;
			this._threadIndex = idx;
		}

		public bool Busy
		{
			get
			{
				return this._busy;
			}
		}

		public bool Idle
		{
			get
			{
				return this._dormant || !this._busy;
			}
		}

		public void Wake()
		{
			this._tasksWaiting.Set();
		}

		public bool Dormant
		{
			get
			{
				return this._dormant;
			}
			set
			{
				this._dormant = value;
			}
		}

		public void AddTask(BaseTask task)
		{
			this._taskList.Queue(task);
			this.Wake();
		}

		public void Abort()
		{
			this._timeToQuit = true;
			this._tasksWaiting.Set();
		}

		public BaseTask GetTask()
		{
			BaseTask result = this._taskList.Dequeue();
			if (result == null && !this._dormant)
			{
				result = TaskDispatcher.Instance.GetTask();
			}
			return result;
		}

		public void DoTask(BaseTask task)
		{
			try
			{
				task.DoWork(this);
			}
			catch (Exception e)
			{
				CastleMinerZGame.Instance.CrashGame(e);
			}
		}

		public void DrainTaskList()
		{
			BaseTask currentTask = this.GetTask();
			while (!this._timeToQuit && currentTask != null)
			{
				this.DoTask(currentTask);
				currentTask = this.GetTask();
			}
		}

		public void ThreadStart()
		{
			Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
			this._initializedCountdown.Decrement();
			while (!this._timeToQuit)
			{
				this.DrainTaskList();
				if (!this._timeToQuit)
				{
					this._busy = false;
					this._tasksWaiting.WaitOne();
					this._busy = true;
				}
			}
		}

		private SynchronizedQueue<BaseTask> _taskList = new SynchronizedQueue<BaseTask>();

		private AutoResetEvent _tasksWaiting = new AutoResetEvent(false);

		private volatile bool _timeToQuit;

		private volatile bool _busy;

		private volatile bool _dormant;

		public int _tasksPending;

		public TaskThreadEnum _threadIndex;

		private CountdownLatch _initializedCountdown;
	}
}
