using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils.Threading
{
	public class TaskDispatcher
	{
		private string TheadName(int i)
		{
			return this._threadNames[i];
		}

		public bool Stopped
		{
			get
			{
				return this._stopped;
			}
		}

		public static TaskDispatcher Create()
		{
			if (TaskDispatcher._theInstance == null)
			{
				TaskDispatcher._theInstance = new TaskDispatcher();
			}
			return TaskDispatcher._theInstance;
		}

		public static TaskDispatcher Instance
		{
			get
			{
				return TaskDispatcher._theInstance;
			}
		}

		private void CreateThreads(CountdownLatch cdown)
		{
			cdown.Value = this._numThreads;
			this._taskThreads = new TaskThread[this._numThreads];
			this._systemThreads = new Thread[this._numThreads];
			for (int i = 0; i < this._numThreads; i++)
			{
				this._taskThreads[i] = new TaskThread(cdown, (TaskThreadEnum)i);
			}
			this._threadNames = new string[this._numThreads];
			for (int j = 0; j < this._numThreads; j++)
			{
				this._threadNames[j] = "TASK_THREAD_" + (j + 1).ToString();
			}
		}

		private void InitThreads(CountdownLatch numThreadsToInitialize)
		{
			this._numThreads = Math.Max(Environment.ProcessorCount - 1, 2);
			this.CreateThreads(numThreadsToInitialize);
			for (int num = 0; num != this._numThreads; num++)
			{
				TaskThread taskThread = this._taskThreads[num];
				this._systemThreads[num] = new Thread(new ThreadStart(taskThread.ThreadStart), 262144);
			}
		}

		private TaskDispatcher()
		{
			TaskDispatcher._theInstance = this;
			this._stopped = false;
			CountdownLatch countdownLatch = default(CountdownLatch);
			this.InitThreads(countdownLatch);
			for (int num = 0; num != this._numThreads; num++)
			{
				this._systemThreads[num].Name = this._threadNames[num];
				this._systemThreads[num].Start();
			}
			while (countdownLatch.Value != 0)
			{
			}
		}

		public bool IsIdle(TaskThreadEnum skipIndex)
		{
			for (int i = 0; i < this._numThreads; i++)
			{
				if (skipIndex != (TaskThreadEnum)i && !this._taskThreads[i].Idle)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsIdle(TaskThread thread)
		{
			return this.IsIdle(thread._threadIndex);
		}

		public void Stop()
		{
			if (this._stopped)
			{
				return;
			}
			this._stopped = true;
			WaitableTask.WakeAll();
			for (int i = 0; i < this._numThreads; i++)
			{
				this._taskThreads[i].Abort();
			}
			for (int j = 0; j < this._numThreads; j++)
			{
				if (!this._systemThreads[j].Join(1000))
				{
					this._systemThreads[j].Abort();
				}
			}
		}

		private void WakeThreads()
		{
			for (int i = 0; i < this._numThreads; i++)
			{
				if (!this._taskThreads[i].Dormant)
				{
					this._taskThreads[i].Wake();
				}
			}
		}

		private void InsertTask(BaseTask task)
		{
			this._tasks.Queue(task);
			this.WakeThreads();
		}

		private void AppendTask(BaseTask task)
		{
			this._tasks.Undequeue(task);
			this.WakeThreads();
		}

		private void InsertTask(TaskThreadEnum thread, BaseTask task)
		{
			if (thread == TaskThreadEnum.THREAD_ANY)
			{
				this.InsertTask(task);
				return;
			}
			this._taskThreads[(int)thread].AddTask(task);
		}

		public void AddTask(BaseTask task)
		{
			if (this._stopped)
			{
				task.Interrupt();
				return;
			}
			this.InsertTask(task);
		}

		public void AddRushTask(BaseTask task)
		{
			if (this._stopped)
			{
				task.Interrupt();
				return;
			}
			this.AppendTask(task);
		}

		public void AddTask(TaskThreadEnum thread, BaseTask task)
		{
			if (thread == TaskThreadEnum.THREAD_ANY)
			{
				this.AddTask(task);
				return;
			}
			if (this._stopped)
			{
				task.Interrupt();
				return;
			}
			this.InsertTask(thread, task);
		}

		public BaseTask GetTask()
		{
			return this._tasks.Dequeue();
		}

		public void AddTask(TaskDelegate work, object context)
		{
			if (!this._stopped)
			{
				Task task = Task.Alloc();
				task.Init(work, context);
				this.InsertTask(task);
			}
		}

		public void AddRushTask(TaskDelegate work, object context)
		{
			if (!this._stopped)
			{
				Task task = Task.Alloc();
				task.Init(work, context);
				this.AppendTask(task);
			}
		}

		public void AddTask(TaskThreadEnum thread, TaskDelegate work, object context)
		{
			if (!this._stopped)
			{
				if (thread == TaskThreadEnum.THREAD_ANY)
				{
					this.AddTask(work, context);
					return;
				}
				Task task = Task.Alloc();
				task.Init(work, context);
				this.InsertTask(thread, task);
			}
		}

		public void AddTaskForMainThread(TaskDelegate work, object context)
		{
			Task task = Task.Alloc();
			task.Init(work, context);
			this._mainThreadTasks.Queue(task);
		}

		public void AddTaskForMainThread(WaitCallback callback, object context)
		{
			LambdaProxy lambdaProxy = LambdaProxy.Alloc(callback);
			this.AddTaskForMainThread(new TaskDelegate(lambdaProxy.Execute), context);
		}

		public void AddTaskForMainThread(ThreadStart callback)
		{
			LambdaProxy lambdaProxy = LambdaProxy.Alloc(callback);
			this.AddTaskForMainThread(new TaskDelegate(lambdaProxy.Execute), null);
		}

		public void RunMainThreadTasks()
		{
			while (!this._mainThreadTasks.Empty)
			{
				BaseTask baseTask = this._mainThreadTasks.Dequeue();
				baseTask.DoWork(null);
			}
		}

		public WaitableTask AddWaitableTask(TaskThreadEnum thread, TaskDelegate work, object context)
		{
			WaitableTask waitableTask;
			if (thread == TaskThreadEnum.THREAD_ANY)
			{
				waitableTask = this.AddWaitableTask(work, context);
			}
			else
			{
				waitableTask = WaitableTask.Alloc();
				waitableTask.Init(work, context);
				if (this._stopped)
				{
					waitableTask.Interrupt();
				}
				else
				{
					this.InsertTask(thread, waitableTask);
				}
			}
			return waitableTask;
		}

		public WaitableTask AddWaitableTask(TaskDelegate work, object context)
		{
			WaitableTask waitableTask = WaitableTask.Alloc();
			waitableTask.Init(work, context);
			if (this._stopped)
			{
				waitableTask.Interrupt();
			}
			else
			{
				this.InsertTask(waitableTask);
			}
			return waitableTask;
		}

		public WaitableTask AddWaitableRushTask(TaskDelegate work, object context)
		{
			WaitableTask waitableTask = WaitableTask.Alloc();
			waitableTask.Init(work, context);
			if (this._stopped)
			{
				waitableTask.Interrupt();
			}
			else
			{
				this.AppendTask(waitableTask);
			}
			return waitableTask;
		}

		public GatherTask AddGatherTask(TaskThreadEnum thread, TaskDelegate work, object context)
		{
			GatherTask gatherTask = this.AddGatherTask(work, context);
			gatherTask.DesiredThreadIndex = thread;
			return gatherTask;
		}

		public GatherTask AddGatherTask(TaskDelegate work, object context)
		{
			GatherTask gatherTask = GatherTask.Alloc();
			gatherTask.Init(work, context);
			return gatherTask;
		}

		private string[] _threadNames;

		private static TaskDispatcher _theInstance;

		private TaskThread[] _taskThreads;

		private Thread[] _systemThreads;

		private int _numThreads;

		private volatile bool _stopped;

		private SynchronizedQueue<BaseTask> _tasks = new SynchronizedQueue<BaseTask>();

		private SynchronizedQueue<BaseTask> _mainThreadTasks = new SynchronizedQueue<BaseTask>();
	}
}
