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
				ChunkCacheCommand command = this._commandQueue.Dequeue();
				try
				{
					this.RunCommand(command);
				}
				catch (Exception e)
				{
					CastleMinerZGame.Instance.CrashGame(e);
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
			ChunkCacheCommand cmd = null;
			lock (this)
			{
				if (this.Running)
				{
					cmd = ChunkCacheCommand.Alloc();
					cmd._command = ChunkCacheCommandEnum.FLUSH;
					this.AddCommand(cmd);
				}
			}
			if (cmd != null)
			{
				while (wait && cmd._status != ChunkCacheCommandStatus.DONE)
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
					ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
					cmd._command = ChunkCacheCommandEnum.SHUTDOWN;
					this.AddCommand(cmd);
				}
			}
			while (wait && this._running)
			{
			}
		}

		public void ResetWaitingChunks()
		{
			ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
			cmd._command = ChunkCacheCommandEnum.RESETWAITINGCHUNKS;
			this.AddCommand(cmd);
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
			ChunkCacheCommand c = this._commandQueue.Front;
			while (c != null)
			{
				if (c._command == command._command)
				{
					if (c._priority >= command._priority)
					{
						command.Release();
						return true;
					}
					this._commandQueue.Remove(c);
					c.Release();
					return false;
				}
				else
				{
					c = (ChunkCacheCommand)c.NextNode;
				}
			}
			return false;
		}

		protected bool ConsolidateNewChunkRequest(ChunkCacheCommand command)
		{
			ChunkCacheCommand c = this._commandQueue.Front;
			while (c != null)
			{
				if (c._command == command._command && c._worldPosition.Equals(command._worldPosition))
				{
					if (c._priority >= command._priority)
					{
						c.CopyRequestersToMe(command);
						command.Release();
						return true;
					}
					command.CopyRequestersToMe(c);
					this._commandQueue.Remove(c);
					c.Release();
					return false;
				}
				else
				{
					c = (ChunkCacheCommand)c.NextNode;
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
							ChunkCacheCommand c = this._commandQueue.Front;
							ChunkCacheCommand lastc = null;
							bool queued = false;
							while (c != null)
							{
								if (c._priority == 0)
								{
									lastc.NextNode = command;
									command.NextNode = c;
									this._commandQueue.IncrementCountAfterInsertion();
									queued = true;
									break;
								}
								lastc = c;
								c = (ChunkCacheCommand)c.NextNode;
							}
							if (!queued)
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
			CachedChunk result = null;
			for (CachedChunk c = queue.Front; c != null; c = (CachedChunk)c.NextNode)
			{
				if (c._worldMin.Equals(v))
				{
					result = c;
					queue.Remove(c);
					queue.Queue(c);
					break;
				}
			}
			return result;
		}

		private void ReduceMemory()
		{
			if (ChunkCache.Instance.IsStorageEnabled)
			{
				CachedChunk chunk = this._cachedChunks.Front;
				if (chunk == null)
				{
					return;
				}
				int memsize = 0;
				while (chunk != null)
				{
					memsize += chunk._numEntries * 4 + 100;
					chunk = (CachedChunk)chunk.NextNode;
				}
				if (memsize > 524288)
				{
					CachedChunk newroot = null;
					CachedChunk newtail = null;
					chunk = this._cachedChunks.Front;
					while (chunk != null && memsize > 524288)
					{
						CachedChunk next = (CachedChunk)chunk.NextNode;
						chunk.NextNode = null;
						if (chunk.SameAsDisk)
						{
							memsize -= chunk._numEntries * 4 + 100;
							chunk.Save();
							chunk.Release();
							chunk = next;
						}
						else
						{
							if (newroot == null)
							{
								newroot = chunk;
							}
							else
							{
								newtail.NextNode = chunk;
							}
							newtail = chunk;
							chunk = next;
						}
					}
					if (chunk != null)
					{
						if (newroot == null)
						{
							newroot = chunk;
						}
						else
						{
							newtail.NextNode = chunk;
						}
					}
					this._cachedChunks.ReplaceFromList(newroot);
				}
			}
		}

		private void MoveFromAToB(CachedChunk v, SimpleQueue<CachedChunk> a, SimpleQueue<CachedChunk> b)
		{
			for (CachedChunk c = a.Front; c != null; c = (CachedChunk)c.NextNode)
			{
				if (c == v)
				{
					a.Remove(c);
					b.Queue(c);
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
			CachedChunk result = CachedChunk.Alloc();
			result.Init(v);
			queue.Queue(result);
			this.AddChunkToLocalList(v);
			return result;
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
			CachedChunk nextChunk;
			for (CachedChunk chunk = this._cachedChunks.Front; chunk != null; chunk = nextChunk)
			{
				nextChunk = (CachedChunk)chunk.NextNode;
				chunk.Save();
			}
		}

		private void ForceReadWaitingChunks()
		{
			while (!this._waitingChunks.Empty)
			{
				CachedChunk c = this._waitingChunks.Dequeue();
				c.RetroReadFromDisk();
				if (c._numEntries == 0)
				{
					this.RemoveChunkFromLocalList(c._worldMin);
					c.Release();
				}
				else
				{
					this._cachedChunks.Queue(c);
				}
			}
		}

		private void InternalResetWaitingChunks()
		{
			for (CachedChunk c = this._waitingChunks.Front; c != null; c = c.NextNode as CachedChunk)
			{
				c.StripFetchCommands();
			}
		}

		private void Heartbeat()
		{
			CachedChunk chunk = this._waitingChunks.Front;
			this._numPri0Waiting = 0;
			this._numPri1Waiting = 0;
			while (chunk != null)
			{
				CachedChunk nextChunk = (CachedChunk)chunk.NextNode;
				if (chunk._loadingPriority == 0)
				{
					this._numPri0Waiting++;
				}
				else
				{
					this._numPri1Waiting++;
				}
				chunk = nextChunk;
			}
		}

		private void ChangeChunkHosts()
		{
			CachedChunk nextChunk;
			for (CachedChunk chunk = this._waitingChunks.Front; chunk != null; chunk = nextChunk)
			{
				nextChunk = (CachedChunk)chunk.NextNode;
				chunk.HostChanged();
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
				int[] newchunks = new int[this._numLocalChunks + 100];
				Buffer.BlockCopy(this._localChunks, 0, newchunks, 0, this._numLocalChunks * 4);
				this._localChunks = newchunks;
			}
			this._localChunks[this._numLocalChunks++] = cid;
		}

		public void AddChunkToLocalList(IntVector3 chunkCorner)
		{
			int cid = CachedChunk.MakeCIDFromChunkCorner(chunkCorner);
			if (this._localChunks == null)
			{
				this.AddChunkToLocalList(cid);
				return;
			}
			for (int i = 0; i < this._numLocalChunks; i++)
			{
				if (this._localChunks[i] == cid)
				{
					return;
				}
			}
			this.AddChunkToLocalList(cid);
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
					IntVector3 chunkCorner = IntVector3.Zero;
					chunkCorner.Y = -64;
					for (int i = 0; i < files.Length; i++)
					{
						string p = Path.GetFileName(files[i]);
						if (p[0] == 'X' && p.EndsWith(".dat"))
						{
							int x = p.IndexOf('X');
							int y = p.IndexOf('Y');
							int z = p.IndexOf('Z');
							int dot = p.IndexOf('.');
							if (dot > z && z > y && y > x)
							{
								chunkCorner.X = int.Parse(p.Substring(x + 1, y - (x + 1)));
								chunkCorner.Z = int.Parse(p.Substring(z + 1, dot - (z + 1)));
								this.AddChunkToLocalList(chunkCorner);
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
			bool releaseCommand = true;
			long commandTimer = 0L;
			this.CurrentQueueDelay = (float)(this._queueTimer.ElapsedMilliseconds - command._submittedTime) / 1000f;
			command._status = ChunkCacheCommandStatus.PROCESSING;
			if (command._trackingString != null)
			{
				commandTimer = this._queueTimer.ElapsedMilliseconds;
			}
			switch (command._command)
			{
			case ChunkCacheCommandEnum.MOD:
			case ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN:
			case ChunkCacheCommandEnum.FETCHDELTAFORCLIENT:
			{
				releaseCommand = false;
				IntVector3 chunkCorner = CachedChunk.MakeChunkCorner(command._worldPosition);
				int cid = CachedChunk.MakeCIDFromChunkCorner(chunkCorner);
				if (this.CIDInLocalList(cid))
				{
					CachedChunk c = this.GetCachedChunk(chunkCorner);
					if (c != null)
					{
						c.RunCommand(command);
					}
					else
					{
						c = this.GetWaitingChunk(chunkCorner);
						if (c != null)
						{
							c.QueueCommand(command);
						}
						else
						{
							c = this.CreateCachedChunk(chunkCorner);
							c.GetChunkFromDisk();
							c.RunCommand(command);
						}
					}
				}
				else if (this.CIDInRemoteList(cid) || (!this._weAreHosting && this._remoteChunks == null))
				{
					CachedChunk c = this.CreateWaitingChunk(chunkCorner);
					c.GetChunkFromHost(command._priority);
					c.QueueCommand(command);
				}
				else if (command._command != ChunkCacheCommandEnum.MOD)
				{
					command._delta = null;
					command._callback(command);
				}
				else
				{
					CachedChunk c = this.CreateCachedChunk(chunkCorner);
					c.RunCommand(command);
				}
				break;
			}
			case ChunkCacheCommandEnum.DELTAARRIVED:
			{
				IntVector3 chunkCorner2 = CachedChunk.MakeChunkCorner(command._worldPosition);
				CachedChunk c2 = this.GetWaitingChunk(chunkCorner2);
				if (c2 != null)
				{
					c2.SetDelta(command._delta, false);
					c2.ExecuteCommands();
					if (c2._numEntries == 0)
					{
						this._waitingChunks.Remove(c2);
						this.RemoveChunkFromLocalList(chunkCorner2);
						c2.Release();
					}
					else
					{
						this.MoveFromAToB(c2, this._waitingChunks, this._cachedChunks);
					}
				}
				else
				{
					releaseCommand = false;
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
				float num = (float)(this._queueTimer.ElapsedMilliseconds - commandTimer) / 1000f;
			}
			if (releaseCommand)
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
					ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
					cmd._command = ChunkCacheCommandEnum.HEARTBEAT;
					cmd._consolidate = true;
					this.AddCommand(cmd);
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
			int i = 0;
			while (i < 2 && this._numRemoteChunks > 0)
			{
				this._numRemoteChunks--;
				if (!this.CIDInLocalList(this._remoteChunks[this._numRemoteChunks]))
				{
					IntVector3 chunkCorner = CachedChunk.MakeChunkCornerFromCID(this._remoteChunks[this._numRemoteChunks]);
					CachedChunk c = this.CreateWaitingChunk(chunkCorner);
					c.GetChunkFromHost(0);
					i++;
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
				ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
				cmd._command = ChunkCacheCommandEnum.ASKHOSTFORREMOTECHUNKS;
				cmd._priority = 0;
				cmd._consolidate = true;
				this.AddCommand(cmd);
			}
		}

		public void SendRemoteChunkList(byte requesterId, bool toall)
		{
			if (!this.Running || CastleMinerZGame.Instance.CurrentNetworkSession == null || CastleMinerZGame.Instance.CurrentNetworkSession.SessionType == NetworkSessionType.Local)
			{
				return;
			}
			ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
			cmd._command = (toall ? ChunkCacheCommandEnum.SENDREMOTECHUNKLISTTOALL : ChunkCacheCommandEnum.SENDREMOTECHUNKLIST);
			cmd._requesterID = requesterId;
			this.AddCommand(cmd);
		}

		public void RemoteChunkListArrived(int[] deltaList)
		{
			ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
			cmd._command = ChunkCacheCommandEnum.REMOTECHUNKLISTARRIVED;
			cmd._delta = deltaList;
			this.AddCommand(cmd);
		}

		public void AddModifiedBlock(IntVector3 worldIndex, BlockTypeEnum blockType)
		{
			ChunkCacheCommand command = ChunkCacheCommand.Alloc();
			command._command = ChunkCacheCommandEnum.MOD;
			command._worldPosition = worldIndex;
			command._blockType = blockType;
			command._priority = 1;
			ChunkCache.Instance.AddCommand(command);
		}

		public void RetrieveChunkForNetwork(byte requesterID, IntVector3 worldmin, int priority, object context)
		{
			ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
			cmd._callback = this._internalChunkLoadedDelegate;
			cmd._worldPosition = CachedChunk.MakeChunkCorner(worldmin);
			cmd._command = ChunkCacheCommandEnum.FETCHDELTAFORCLIENT;
			cmd._context = context;
			cmd._priority = priority;
			cmd._requesterIDs[0] = requesterID;
			cmd._numRequesters = 1;
			this.AddCommand(cmd);
		}

		public void ChunkDeltaArrived(IntVector3 worldmin, int[] delta, byte priority)
		{
			ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
			cmd._command = ChunkCacheCommandEnum.DELTAARRIVED;
			cmd._worldPosition = worldmin;
			cmd._delta = delta;
			cmd._priority = (int)priority;
			this.AddCommand(cmd);
		}

		public void HostChanged()
		{
			ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
			cmd._command = ChunkCacheCommandEnum.HOSTCHANGED;
			this.AddCommand(cmd);
		}

		public void MakeHost(WorldInfo worldinfo, bool value)
		{
			if (this.Running)
			{
				ChunkCacheCommand cmd = ChunkCacheCommand.Alloc();
				cmd._command = (value ? ChunkCacheCommandEnum.BECOMEHOST : ChunkCacheCommandEnum.BECOMECLIENT);
				cmd._context = worldinfo;
				this.AddCommand(cmd);
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
