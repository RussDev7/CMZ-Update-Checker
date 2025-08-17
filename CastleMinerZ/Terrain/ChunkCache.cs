using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DNA.CastleMinerZ.Utils;
using DNA.Drawing.UI;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain
{
	public class ChunkCache
	{
		public bool IsStorageEnabled
		{
			get
			{
				return this._worldInfo != null && this._worldInfo.SavePath != null;
			}
		}

		public string RootPath
		{
			get
			{
				return this._worldInfo.SavePath;
			}
		}

		private void TestCallback(ChunkCacheCommand cmd)
		{
			cmd.Release();
		}

		private ChunkCache()
		{
			this._internalChunkLoadedDelegate = new ChunkCacheCommandDelegate(this.InternalRetrieveChunkCallback);
		}

		private void DrainCommandQueue()
		{
			while (!this._quit && !this._commandQueue.Empty)
			{
				ChunkCacheCommand chunkCacheCommand = this._commandQueue.Dequeue();
				try
				{
					this.RunCommand(chunkCacheCommand);
				}
				catch (Exception ex)
				{
					CastleMinerZGame.Instance.CrashGame(ex);
				}
			}
		}

		private void StartThread()
		{
			Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
			this._commandsWaiting.Reset();
			this._running = true;
			while (!this._quit)
			{
				this.DrainCommandQueue();
				if (!this._quit)
				{
					this._commandsWaiting.WaitOne();
				}
			}
			lock (this)
			{
				this._running = false;
				while (!this._commandQueue.Empty)
				{
					this._commandQueue.Dequeue().Release();
				}
			}
		}

		public void Flush(bool wait)
		{
			ChunkCacheCommand chunkCacheCommand = null;
			lock (this)
			{
				if (this.Running)
				{
					chunkCacheCommand = ChunkCacheCommand.Alloc();
					chunkCacheCommand._command = ChunkCacheCommandEnum.FLUSH;
					this.AddCommand(chunkCacheCommand);
				}
			}
			if (chunkCacheCommand != null)
			{
				while (wait && chunkCacheCommand._status != ChunkCacheCommandStatus.DONE)
				{
				}
			}
		}

		public void Start(bool wait)
		{
			if (!this._running)
			{
				this._thread = new Thread(delegate
				{
					this.StartThread();
				});
				this._thread.Name = "ChunkCacheThread";
				this._quit = false;
				this._numPri0Waiting = 0;
				this._numPri1Waiting = 0;
				this._timeTilHeartbeat = 0f;
				this._timeTilPushChunks = 3f;
				this._thread.Start();
				while (!this._running)
				{
				}
			}
		}

		public void Stop(bool wait)
		{
			lock (this)
			{
				if (this.Running)
				{
					ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
					chunkCacheCommand._command = ChunkCacheCommandEnum.SHUTDOWN;
					this.AddCommand(chunkCacheCommand);
				}
			}
			while (wait && this._running)
			{
			}
		}

		public void ResetWaitingChunks()
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.RESETWAITINGCHUNKS;
			this.AddCommand(chunkCacheCommand);
		}

		public bool Running
		{
			get
			{
				return this._running && !this._quit;
			}
		}

		protected bool ConsolidateNewCommand(ChunkCacheCommand command)
		{
			ChunkCacheCommand chunkCacheCommand = this._commandQueue.Front;
			while (chunkCacheCommand != null)
			{
				if (chunkCacheCommand._command == command._command)
				{
					if (chunkCacheCommand._priority >= command._priority)
					{
						command.Release();
						return true;
					}
					this._commandQueue.Remove(chunkCacheCommand);
					chunkCacheCommand.Release();
					return false;
				}
				else
				{
					chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand.NextNode;
				}
			}
			return false;
		}

		protected bool ConsolidateNewChunkRequest(ChunkCacheCommand command)
		{
			ChunkCacheCommand chunkCacheCommand = this._commandQueue.Front;
			while (chunkCacheCommand != null)
			{
				if (chunkCacheCommand._command == command._command && chunkCacheCommand._worldPosition.Equals(command._worldPosition))
				{
					if (chunkCacheCommand._priority >= command._priority)
					{
						chunkCacheCommand.CopyRequestersToMe(command);
						command.Release();
						return true;
					}
					command.CopyRequestersToMe(chunkCacheCommand);
					this._commandQueue.Remove(chunkCacheCommand);
					chunkCacheCommand.Release();
					return false;
				}
				else
				{
					chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand.NextNode;
				}
			}
			return false;
		}

		public void AddCommand(ChunkCacheCommand command)
		{
			lock (this)
			{
				if (this.Running)
				{
					command._submittedTime = this._queueTimer.ElapsedMilliseconds;
					lock (this._commandQueue)
					{
						if (command._command == ChunkCacheCommandEnum.FETCHDELTAFORCLIENT)
						{
							if (this.ConsolidateNewChunkRequest(command))
							{
								return;
							}
						}
						else if (command._consolidate && this.ConsolidateNewCommand(command))
						{
							return;
						}
						if (command._priority == 0 || this._commandQueue.Empty)
						{
							this._commandQueue.Queue(command);
						}
						else if (this._commandQueue.Back._priority == 1)
						{
							this._commandQueue.Queue(command);
						}
						else if (this._commandQueue.Front._priority == 0)
						{
							this._commandQueue.Undequeue(command);
						}
						else
						{
							ChunkCacheCommand chunkCacheCommand = this._commandQueue.Front;
							ChunkCacheCommand chunkCacheCommand2 = null;
							bool flag3 = false;
							while (chunkCacheCommand != null)
							{
								if (chunkCacheCommand._priority == 0)
								{
									chunkCacheCommand2.NextNode = command;
									command.NextNode = chunkCacheCommand;
									this._commandQueue.IncrementCountAfterInsertion();
									flag3 = true;
									break;
								}
								chunkCacheCommand2 = chunkCacheCommand;
								chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand.NextNode;
							}
							if (!flag3)
							{
								this._commandQueue.Queue(command);
							}
						}
					}
					command._status = ChunkCacheCommandStatus.QUEUED;
					this._commandsWaiting.Set();
				}
				else
				{
					command.Release();
				}
			}
		}

		public CachedChunk FindChunk(IntVector3 v, SimpleQueue<CachedChunk> queue)
		{
			CachedChunk cachedChunk = null;
			for (CachedChunk cachedChunk2 = queue.Front; cachedChunk2 != null; cachedChunk2 = (CachedChunk)cachedChunk2.NextNode)
			{
				if (cachedChunk2._worldMin.Equals(v))
				{
					cachedChunk = cachedChunk2;
					queue.Remove(cachedChunk2);
					queue.Queue(cachedChunk2);
					break;
				}
			}
			return cachedChunk;
		}

		private void ReduceMemory()
		{
			if (ChunkCache.Instance.IsStorageEnabled)
			{
				CachedChunk cachedChunk = this._cachedChunks.Front;
				if (cachedChunk == null)
				{
					return;
				}
				int num = 0;
				while (cachedChunk != null)
				{
					num += cachedChunk._numEntries * 4 + 100;
					cachedChunk = (CachedChunk)cachedChunk.NextNode;
				}
				if (num > 524288)
				{
					CachedChunk cachedChunk2 = null;
					CachedChunk cachedChunk3 = null;
					cachedChunk = this._cachedChunks.Front;
					while (cachedChunk != null && num > 524288)
					{
						CachedChunk cachedChunk4 = (CachedChunk)cachedChunk.NextNode;
						cachedChunk.NextNode = null;
						if (cachedChunk.SameAsDisk)
						{
							num -= cachedChunk._numEntries * 4 + 100;
							cachedChunk.Save();
							cachedChunk.Release();
							cachedChunk = cachedChunk4;
						}
						else
						{
							if (cachedChunk2 == null)
							{
								cachedChunk2 = cachedChunk;
							}
							else
							{
								cachedChunk3.NextNode = cachedChunk;
							}
							cachedChunk3 = cachedChunk;
							cachedChunk = cachedChunk4;
						}
					}
					if (cachedChunk != null)
					{
						if (cachedChunk2 == null)
						{
							cachedChunk2 = cachedChunk;
						}
						else
						{
							cachedChunk3.NextNode = cachedChunk;
						}
					}
					this._cachedChunks.ReplaceFromList(cachedChunk2);
				}
			}
		}

		private void MoveFromAToB(CachedChunk v, SimpleQueue<CachedChunk> a, SimpleQueue<CachedChunk> b)
		{
			for (CachedChunk cachedChunk = a.Front; cachedChunk != null; cachedChunk = (CachedChunk)cachedChunk.NextNode)
			{
				if (cachedChunk == v)
				{
					a.Remove(cachedChunk);
					b.Queue(cachedChunk);
					return;
				}
			}
		}

		private CachedChunk GetCachedChunk(IntVector3 v)
		{
			return this.FindChunk(v, this._cachedChunks);
		}

		private CachedChunk GetWaitingChunk(IntVector3 v)
		{
			return this.FindChunk(v, this._waitingChunks);
		}

		private CachedChunk CreateChunk(IntVector3 v, SimpleQueue<CachedChunk> queue)
		{
			CachedChunk cachedChunk = CachedChunk.Alloc();
			cachedChunk.Init(v);
			queue.Queue(cachedChunk);
			this.AddChunkToLocalList(v);
			return cachedChunk;
		}

		private CachedChunk CreateCachedChunk(IntVector3 v)
		{
			return this.CreateChunk(v, this._cachedChunks);
		}

		private CachedChunk CreateWaitingChunk(IntVector3 v)
		{
			return this.CreateChunk(v, this._waitingChunks);
		}

		private void FlushCachedChunks()
		{
			CachedChunk cachedChunk2;
			for (CachedChunk cachedChunk = this._cachedChunks.Front; cachedChunk != null; cachedChunk = cachedChunk2)
			{
				cachedChunk2 = (CachedChunk)cachedChunk.NextNode;
				cachedChunk.Save();
			}
		}

		private void ForceReadWaitingChunks()
		{
			while (!this._waitingChunks.Empty)
			{
				CachedChunk cachedChunk = this._waitingChunks.Dequeue();
				cachedChunk.RetroReadFromDisk();
				if (cachedChunk._numEntries == 0)
				{
					this.RemoveChunkFromLocalList(cachedChunk._worldMin);
					cachedChunk.Release();
				}
				else
				{
					this._cachedChunks.Queue(cachedChunk);
				}
			}
		}

		private void InternalResetWaitingChunks()
		{
			for (CachedChunk cachedChunk = this._waitingChunks.Front; cachedChunk != null; cachedChunk = cachedChunk.NextNode as CachedChunk)
			{
				cachedChunk.StripFetchCommands();
			}
		}

		private void Heartbeat()
		{
			CachedChunk cachedChunk = this._waitingChunks.Front;
			this._numPri0Waiting = 0;
			this._numPri1Waiting = 0;
			while (cachedChunk != null)
			{
				CachedChunk cachedChunk2 = (CachedChunk)cachedChunk.NextNode;
				if (cachedChunk._loadingPriority == 0)
				{
					this._numPri0Waiting++;
				}
				else
				{
					this._numPri1Waiting++;
				}
				cachedChunk = cachedChunk2;
			}
		}

		private void ChangeChunkHosts()
		{
			CachedChunk cachedChunk2;
			for (CachedChunk cachedChunk = this._waitingChunks.Front; cachedChunk != null; cachedChunk = cachedChunk2)
			{
				cachedChunk2 = (CachedChunk)cachedChunk.NextNode;
				cachedChunk.HostChanged();
			}
		}

		private string MakeFilename()
		{
			return Path.Combine(ChunkCache.Instance.RootPath, "chunklist.lst");
		}

		public bool CIDInRemoteList(int cid)
		{
			if (this._numRemoteChunks == 0)
			{
				return false;
			}
			for (int i = 0; i < this._numRemoteChunks; i++)
			{
				if (this._remoteChunks[i] == cid)
				{
					return true;
				}
			}
			return false;
		}

		public bool ChunkInRemoteList(IntVector3 chunkCorner)
		{
			return this._numRemoteChunks != 0 && this.CIDInRemoteList(CachedChunk.MakeCIDFromChunkCorner(chunkCorner));
		}

		public bool CIDInLocalList(int cid)
		{
			if (this._numLocalChunks == 0)
			{
				return false;
			}
			for (int i = 0; i < this._numLocalChunks; i++)
			{
				if (this._localChunks[i] == cid)
				{
					return true;
				}
			}
			return false;
		}

		public bool ChunkInLocalList(IntVector3 chunkCorner)
		{
			return this._numLocalChunks != 0 && this.CIDInLocalList(CachedChunk.MakeCIDFromChunkCorner(chunkCorner));
		}

		private void AddChunkToLocalList(int cid)
		{
			if (this._localChunks == null)
			{
				this._localChunks = new int[100];
			}
			else if (this._numLocalChunks == this._localChunks.Length)
			{
				int[] array = new int[this._numLocalChunks + 100];
				Buffer.BlockCopy(this._localChunks, 0, array, 0, this._numLocalChunks * 4);
				this._localChunks = array;
			}
			this._localChunks[this._numLocalChunks++] = cid;
		}

		public void AddChunkToLocalList(IntVector3 chunkCorner)
		{
			int num = CachedChunk.MakeCIDFromChunkCorner(chunkCorner);
			if (this._localChunks == null)
			{
				this.AddChunkToLocalList(num);
				return;
			}
			for (int i = 0; i < this._numLocalChunks; i++)
			{
				if (this._localChunks[i] == num)
				{
					return;
				}
			}
			this.AddChunkToLocalList(num);
		}

		private void RemoveChunkFromLocalList(int cid)
		{
			if (this._localChunks != null)
			{
				for (int i = 0; i < this._numLocalChunks; i++)
				{
					if (this._localChunks[i] == cid)
					{
						this._numLocalChunks--;
						if (this._numLocalChunks != i || this._numLocalChunks != 0)
						{
							this._localChunks[i] = this._localChunks[this._numLocalChunks];
						}
						return;
					}
				}
			}
		}

		private void RemoveChunkFromLocalList(IntVector3 chunkCorner)
		{
			this.RemoveChunkFromLocalList(CachedChunk.MakeCIDFromChunkCorner(chunkCorner));
		}

		private void LoadChunkList()
		{
			if (this._weAreHosting && ChunkCache.Instance.IsStorageEnabled && this._numLocalChunks == 0)
			{
				SignedInGamer currentGamer = Screen.CurrentGamer;
				this.MakeFilename();
				try
				{
					string[] files = CastleMinerZGame.Instance.SaveDevice.GetFiles(Path.Combine(ChunkCache.Instance.RootPath, "*.*"));
					IntVector3 zero = IntVector3.Zero;
					zero.Y = -64;
					for (int i = 0; i < files.Length; i++)
					{
						string fileName = Path.GetFileName(files[i]);
						if (fileName[0] == 'X' && fileName.EndsWith(".dat"))
						{
							int num = fileName.IndexOf('X');
							int num2 = fileName.IndexOf('Y');
							int num3 = fileName.IndexOf('Z');
							int num4 = fileName.IndexOf('.');
							if (num4 > num3 && num3 > num2 && num2 > num)
							{
								zero.X = int.Parse(fileName.Substring(num + 1, num2 - (num + 1)));
								zero.Z = int.Parse(fileName.Substring(num3 + 1, num4 - (num3 + 1)));
								this.AddChunkToLocalList(zero);
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private void RunCommand(ChunkCacheCommand command)
		{
			bool flag = true;
			long num = 0L;
			this.CurrentQueueDelay = (float)(this._queueTimer.ElapsedMilliseconds - command._submittedTime) / 1000f;
			command._status = ChunkCacheCommandStatus.PROCESSING;
			if (command._trackingString != null)
			{
				num = this._queueTimer.ElapsedMilliseconds;
			}
			switch (command._command)
			{
			case ChunkCacheCommandEnum.MOD:
			case ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN:
			case ChunkCacheCommandEnum.FETCHDELTAFORCLIENT:
			{
				flag = false;
				IntVector3 intVector = CachedChunk.MakeChunkCorner(command._worldPosition);
				int num2 = CachedChunk.MakeCIDFromChunkCorner(intVector);
				if (this.CIDInLocalList(num2))
				{
					CachedChunk cachedChunk = this.GetCachedChunk(intVector);
					if (cachedChunk != null)
					{
						cachedChunk.RunCommand(command);
					}
					else
					{
						cachedChunk = this.GetWaitingChunk(intVector);
						if (cachedChunk != null)
						{
							cachedChunk.QueueCommand(command);
						}
						else
						{
							cachedChunk = this.CreateCachedChunk(intVector);
							cachedChunk.GetChunkFromDisk();
							cachedChunk.RunCommand(command);
						}
					}
				}
				else if (this.CIDInRemoteList(num2) || (!this._weAreHosting && this._remoteChunks == null))
				{
					CachedChunk cachedChunk = this.CreateWaitingChunk(intVector);
					cachedChunk.GetChunkFromHost(command._priority);
					cachedChunk.QueueCommand(command);
				}
				else if (command._command != ChunkCacheCommandEnum.MOD)
				{
					command._delta = null;
					command._callback(command);
				}
				else
				{
					CachedChunk cachedChunk = this.CreateCachedChunk(intVector);
					cachedChunk.RunCommand(command);
				}
				break;
			}
			case ChunkCacheCommandEnum.DELTAARRIVED:
			{
				IntVector3 intVector2 = CachedChunk.MakeChunkCorner(command._worldPosition);
				CachedChunk waitingChunk = this.GetWaitingChunk(intVector2);
				if (waitingChunk != null)
				{
					waitingChunk.SetDelta(command._delta, false);
					waitingChunk.ExecuteCommands();
					if (waitingChunk._numEntries == 0)
					{
						this._waitingChunks.Remove(waitingChunk);
						this.RemoveChunkFromLocalList(intVector2);
						waitingChunk.Release();
					}
					else
					{
						this.MoveFromAToB(waitingChunk, this._waitingChunks, this._cachedChunks);
					}
				}
				else
				{
					flag = false;
					command.Release();
					command = null;
				}
				break;
			}
			case ChunkCacheCommandEnum.BECOMEHOST:
				this._weAreHosting = true;
				if (command._context != null)
				{
					this._worldInfo = (WorldInfo)command._context;
				}
				this.LoadChunkList();
				this._remoteChunks = null;
				this._numRemoteChunks = 0;
				this.ForceReadWaitingChunks();
				break;
			case ChunkCacheCommandEnum.BECOMECLIENT:
				this._weAreHosting = false;
				if (command._context != null)
				{
					if (this._worldInfo == null)
					{
						this.BroadcastThatWereReady();
					}
					this._worldInfo = (WorldInfo)command._context;
				}
				break;
			case ChunkCacheCommandEnum.FLUSH:
				this.FlushCachedChunks();
				break;
			case ChunkCacheCommandEnum.HOSTCHANGED:
				if (!this._weAreHosting)
				{
					this.ChangeChunkHosts();
				}
				break;
			case ChunkCacheCommandEnum.HEARTBEAT:
				this.Heartbeat();
				this.ReduceMemory();
				break;
			case ChunkCacheCommandEnum.SHUTDOWN:
				this.InternalResetWaitingChunks();
				this.FlushCachedChunks();
				this._quit = true;
				while (!this._cachedChunks.Empty)
				{
					this._cachedChunks.Dequeue().Release();
				}
				while (!this._waitingChunks.Empty)
				{
					this._waitingChunks.Dequeue().Release();
				}
				this._numLocalChunks = 0;
				this._copyOfLocalChunks = null;
				this._worldInfo = null;
				this.AlreadyForcedRestart = false;
				this._numRemoteChunks = 0;
				this._remoteChunks = null;
				this._numPri0Waiting = 0;
				this._numPri1Waiting = 0;
				break;
			case ChunkCacheCommandEnum.RESETWAITINGCHUNKS:
				this.InternalResetWaitingChunks();
				break;
			case ChunkCacheCommandEnum.ASKHOSTFORREMOTECHUNKS:
				this.InternalAskHostForSomeRemoteChunks();
				break;
			case ChunkCacheCommandEnum.SENDREMOTECHUNKLIST:
				this.InternalSendRemoteChunkList(command._requesterID, false);
				break;
			case ChunkCacheCommandEnum.REMOTECHUNKLISTARRIVED:
				if (this._worldInfo != null && this._remoteChunks == null)
				{
					if (command._delta == null)
					{
						this._remoteChunks = new int[1];
						this._numRemoteChunks = 0;
					}
					else
					{
						this._remoteChunks = command._delta;
						this._numRemoteChunks = command._delta.Length;
					}
				}
				break;
			case ChunkCacheCommandEnum.SENDREMOTECHUNKLISTTOALL:
				this.InternalSendRemoteChunkList(0, true);
				break;
			}
			if (command != null && command._trackingString != null)
			{
				float num3 = (float)(this._queueTimer.ElapsedMilliseconds - num) / 1000f;
			}
			if (flag)
			{
				command.Release();
			}
		}

		public void Update(GameTime time)
		{
			if (this.Running)
			{
				this._timeTilHeartbeat -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._timeTilHeartbeat <= 0f)
				{
					ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
					chunkCacheCommand._command = ChunkCacheCommandEnum.HEARTBEAT;
					chunkCacheCommand._consolidate = true;
					this.AddCommand(chunkCacheCommand);
					this._timeTilHeartbeat = 0.5f;
				}
				this._timeTilPushChunks -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._timeTilPushChunks <= 0f)
				{
					this.AskHostForSomeRemoteChunks();
					this._timeTilPushChunks = 2f;
				}
			}
		}

		private void BroadcastThatWereReady()
		{
			MainThreadMessageSender.Instance.ClientReadyForChunks();
			this._remoteChunks = null;
			this._numRemoteChunks = 0;
		}

		private void InternalRetrieveChunkCallback(ChunkCacheCommand cmd)
		{
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				MainThreadMessageSender.Instance.ProvideChunkMessage(cmd._worldPosition, cmd._delta, cmd._priority, cmd._numRequesters, cmd._requesterIDs);
			}
			cmd.Release();
		}

		public void GetChunkFromServer(IntVector3 worldmin, int priority)
		{
			MainThreadMessageSender.Instance.RequestChunkMessage(worldmin, priority);
		}

		public int NumPendingRequests
		{
			get
			{
				return this._commandQueue.Count;
			}
		}

		private void InternalAskHostForSomeRemoteChunks()
		{
			if (this._numRemoteChunks == 0)
			{
				return;
			}
			if (this._numPri0Waiting > 5)
			{
				return;
			}
			if (this._numPri1Waiting > 2)
			{
				return;
			}
			int num = 0;
			while (num < 2 && this._numRemoteChunks > 0)
			{
				this._numRemoteChunks--;
				if (!this.CIDInLocalList(this._remoteChunks[this._numRemoteChunks]))
				{
					IntVector3 intVector = CachedChunk.MakeChunkCornerFromCID(this._remoteChunks[this._numRemoteChunks]);
					CachedChunk cachedChunk = this.CreateWaitingChunk(intVector);
					cachedChunk.GetChunkFromHost(0);
					num++;
				}
			}
		}

		private void InternalSendRemoteChunkList(byte requesterId, bool toall)
		{
			if (this._numLocalChunks != 0)
			{
				if (this._copyOfLocalChunks == null || this._copyOfLocalChunks.Length != this._numLocalChunks)
				{
					this._copyOfLocalChunks = new int[this._numLocalChunks];
					Buffer.BlockCopy(this._localChunks, 0, this._copyOfLocalChunks, 0, this._numLocalChunks * 4);
				}
				MainThreadMessageSender.Instance.SendDeltaListMessage(this._copyOfLocalChunks, requesterId, toall);
			}
		}

		public void AskHostForSomeRemoteChunks()
		{
			if (!this._weAreHosting && this._numRemoteChunks != 0)
			{
				ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
				chunkCacheCommand._command = ChunkCacheCommandEnum.ASKHOSTFORREMOTECHUNKS;
				chunkCacheCommand._priority = 0;
				chunkCacheCommand._consolidate = true;
				this.AddCommand(chunkCacheCommand);
			}
		}

		public void SendRemoteChunkList(byte requesterId, bool toall)
		{
			if (!this.Running || CastleMinerZGame.Instance.CurrentNetworkSession == null || CastleMinerZGame.Instance.CurrentNetworkSession.SessionType == NetworkSessionType.Local)
			{
				return;
			}
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = (toall ? ChunkCacheCommandEnum.SENDREMOTECHUNKLISTTOALL : ChunkCacheCommandEnum.SENDREMOTECHUNKLIST);
			chunkCacheCommand._requesterID = requesterId;
			this.AddCommand(chunkCacheCommand);
		}

		public void RemoteChunkListArrived(int[] deltaList)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.REMOTECHUNKLISTARRIVED;
			chunkCacheCommand._delta = deltaList;
			this.AddCommand(chunkCacheCommand);
		}

		public void AddModifiedBlock(IntVector3 worldIndex, BlockTypeEnum blockType)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.MOD;
			chunkCacheCommand._worldPosition = worldIndex;
			chunkCacheCommand._blockType = blockType;
			chunkCacheCommand._priority = 1;
			ChunkCache.Instance.AddCommand(chunkCacheCommand);
		}

		public void RetrieveChunkForNetwork(byte requesterID, IntVector3 worldmin, int priority, object context)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._callback = this._internalChunkLoadedDelegate;
			chunkCacheCommand._worldPosition = CachedChunk.MakeChunkCorner(worldmin);
			chunkCacheCommand._command = ChunkCacheCommandEnum.FETCHDELTAFORCLIENT;
			chunkCacheCommand._context = context;
			chunkCacheCommand._priority = priority;
			chunkCacheCommand._requesterIDs[0] = requesterID;
			chunkCacheCommand._numRequesters = 1;
			this.AddCommand(chunkCacheCommand);
		}

		public void ChunkDeltaArrived(IntVector3 worldmin, int[] delta, byte priority)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.DELTAARRIVED;
			chunkCacheCommand._worldPosition = worldmin;
			chunkCacheCommand._delta = delta;
			chunkCacheCommand._priority = (int)priority;
			this.AddCommand(chunkCacheCommand);
		}

		public void HostChanged()
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.HOSTCHANGED;
			this.AddCommand(chunkCacheCommand);
		}

		public void MakeHost(WorldInfo worldinfo, bool value)
		{
			if (this.Running)
			{
				ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
				chunkCacheCommand._command = (value ? ChunkCacheCommandEnum.BECOMEHOST : ChunkCacheCommandEnum.BECOMECLIENT);
				chunkCacheCommand._context = worldinfo;
				this.AddCommand(chunkCacheCommand);
			}
		}

		private const int MAX_CHUNK_MEMORY = 524288;

		private const float ASK_HOST_FOR_CHUNKS_INTERVAL = 2f;

		private const int MAX_CHUNKS_TO_ASK_HOST_FOR = 2;

		private const float HEARTBEAT_INTERVAL = 0.5f;

		private const byte CHUNKLIST_VERSION = 0;

		private const int MAX_PR0_WAITING = 5;

		private const int MAX_PR1_WAITING = 2;

		public static ChunkCache Instance = new ChunkCache();

		private SynchronizedQueue<ChunkCacheCommand> _commandQueue = new SynchronizedQueue<ChunkCacheCommand>();

		private SimpleQueue<CachedChunk> _cachedChunks = new SimpleQueue<CachedChunk>();

		private SimpleQueue<CachedChunk> _waitingChunks = new SimpleQueue<CachedChunk>();

		public int[] _localChunks;

		public int _numLocalChunks;

		public int[] _remoteChunks;

		public int _numRemoteChunks;

		public int[] _copyOfLocalChunks;

		private AutoResetEvent _commandsWaiting = new AutoResetEvent(false);

		private Thread _thread;

		private Stopwatch _queueTimer = Stopwatch.StartNew();

		private float _timeTilHeartbeat = 0.5f;

		private float _timeTilPushChunks = 2f;

		private volatile bool _quit;

		private volatile bool _running;

		private ChunkCacheCommandDelegate _internalChunkLoadedDelegate;

		private bool _weAreHosting;

		public WorldInfo _worldInfo;

		public bool AlreadyForcedRestart;

		public float CurrentQueueDelay;

		private int _numPri1Waiting;

		private int _numPri0Waiting;
	}
}
