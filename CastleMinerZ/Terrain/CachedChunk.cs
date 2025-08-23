using System;
using System.Diagnostics;
using System.IO;
using DNA.CastleMinerZ.Utils;
using DNA.Drawing.UI;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Terrain
{
	public class CachedChunk : IReleaseable, ILinkedListNode
	{
		public static IntVector3 MakeChunkCorner(IntVector3 position)
		{
			IntVector3 chunkCorner;
			chunkCorner.X = (int)(Math.Floor((double)position.X / 16.0) * 16.0);
			chunkCorner.Y = -64;
			chunkCorner.Z = (int)(Math.Floor((double)position.Z / 16.0) * 16.0);
			return chunkCorner;
		}

		public static IntVector3 MakeChunkCornerFromCID(int cid)
		{
			IntVector3 chunkCorner = IntVector3.Zero;
			short chunkz = (short)((int)((long)cid & (long)((ulong)(-65536))) >> 16);
			short chunkx = (short)((long)cid & 65535L);
			chunkCorner.X = (int)(chunkx * 16);
			chunkCorner.Y = -64;
			chunkCorner.Z = (int)(chunkz * 16);
			return chunkCorner;
		}

		public static int MakeCIDFromChunkCorner(IntVector3 chunkCorner)
		{
			uint chunkz = (uint)((chunkCorner.Z / 16) & 65535);
			uint chunkx = (uint)((chunkCorner.X / 16) & 65535);
			return (int)((chunkz << 16) | chunkx);
		}

		public bool SameAsDisk
		{
			get
			{
				return this._sameAsDisk;
			}
			set
			{
				if (this._sameAsDisk != value)
				{
					this._sameAsDisk = value;
					if (!this._sameAsDisk)
					{
						this._saved = false;
					}
				}
			}
		}

		private string MakeFilename()
		{
			return Path.Combine(ChunkCache.Instance.RootPath, string.Concat(new string[]
			{
				"X",
				this._worldMin.X.ToString(),
				"Y",
				this._worldMin.Y.ToString(),
				"Z",
				this._worldMin.Z.ToString(),
				".dat"
			}));
		}

		public void Init(IntVector3 worldMin)
		{
			this._worldMin = worldMin;
			this._saved = true;
			this._sameAsDisk = true;
			this._loadingPriority = 0;
			this._numEntries = 0;
			this._copy = null;
		}

		public void SetDelta(int[] delta, bool cameFromDisk)
		{
			this._saved = cameFromDisk;
			this._sameAsDisk = true;
			this._delta = delta;
			if (this._delta != null)
			{
				this._numEntries = delta.Length;
				return;
			}
			this._numEntries = 0;
		}

		public int[] GetDeltaCopy()
		{
			if (this._numEntries == 0)
			{
				return null;
			}
			if (this._numEntries == this._delta.Length)
			{
				return this._delta;
			}
			if (this._copy == null)
			{
				this._copy = new int[this._numEntries];
				Buffer.BlockCopy(this._delta, 0, this._copy, 0, this._numEntries * 4);
			}
			return this._copy;
		}

		private void Embiggen()
		{
			int newsize;
			if (this._delta == null)
			{
				newsize = 32;
			}
			else
			{
				newsize = this._delta.Length + 32;
			}
			this.Resize(newsize);
		}

		private void Resize(int newsize)
		{
			int[] newd = new int[newsize];
			if (this._numEntries > 0)
			{
				Buffer.BlockCopy(this._delta, 0, newd, 0, this._numEntries * 4);
			}
			this._delta = newd;
		}

		public void AddWorldVector(IntVector3 entry, BlockTypeEnum type)
		{
			this.Add(IntVector3.Subtract(entry, this._worldMin), type);
		}

		public void Add(IntVector3 entry, BlockTypeEnum type)
		{
			int newEntry = DeltaEntry.Create(entry, type);
			for (int i = 0; i < this._numEntries; i++)
			{
				if (DeltaEntry.SameLocation(this._delta[i], newEntry))
				{
					if (this._delta[i] != newEntry)
					{
						this._delta[i] = newEntry;
						this._copy = null;
						this.SameAsDisk = false;
					}
					return;
				}
			}
			if (this._delta == null || this._numEntries >= this._delta.Length)
			{
				this.Embiggen();
			}
			this._delta[this._numEntries++] = newEntry;
			this._copy = null;
			this.SameAsDisk = false;
		}

		public void QueueCommand(ChunkCacheCommand command)
		{
			if (command._priority > this._loadingPriority)
			{
				this.GetChunkFromHost(command._priority);
			}
			if (command._command == ChunkCacheCommandEnum.MOD)
			{
				for (ChunkCacheCommand cmd = this._commandQueue.Front; cmd != null; cmd = (ChunkCacheCommand)cmd.NextNode)
				{
					if (cmd._command == ChunkCacheCommandEnum.MOD && cmd._worldPosition.Equals(command._worldPosition))
					{
						cmd._blockType = command._blockType;
						command.Release();
						return;
					}
				}
			}
			this._commandQueue.Queue(command);
			command._status = ChunkCacheCommandStatus.BLOCKED;
		}

		public void RunCommand(ChunkCacheCommand command)
		{
			bool releaseCommand = true;
			ChunkCacheCommandEnum command2 = command._command;
			switch (command2)
			{
			case ChunkCacheCommandEnum.MOD:
			{
				IntVector3 location = IntVector3.Subtract(command._worldPosition, this._worldMin);
				this.Add(location, command._blockType);
				goto IL_0059;
			}
			case ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN:
				break;
			default:
				if (command2 != ChunkCacheCommandEnum.FETCHDELTAFORCLIENT)
				{
					goto IL_0059;
				}
				break;
			}
			command._delta = this.GetDeltaCopy();
			command._callback(command);
			releaseCommand = false;
			IL_0059:
			if (releaseCommand)
			{
				command.Release();
			}
		}

		public void StripFetchCommands()
		{
			ChunkCacheCommand newHead = null;
			while (!this._commandQueue.Empty)
			{
				ChunkCacheCommand c = this._commandQueue.Dequeue();
				if (c._command == ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN)
				{
					c._command = ChunkCacheCommandEnum.RESETWAITINGCHUNKS;
					c._callback(c);
					if (this._loadingPriority > 0)
					{
						this._loadingPriority = 0;
					}
				}
				else
				{
					c.NextNode = newHead;
					newHead = c;
				}
			}
			while (newHead != null)
			{
				ChunkCacheCommand c = newHead;
				newHead = (ChunkCacheCommand)c.NextNode;
				c.NextNode = null;
				this._commandQueue.Undequeue(c);
			}
		}

		public void ExecuteCommands()
		{
			while (!this._commandQueue.Empty)
			{
				this.RunCommand(this._commandQueue.Dequeue());
			}
		}

		public void GetChunkFromDisk()
		{
			if (ChunkCache.Instance.IsStorageEnabled && ChunkCache.Instance.ChunkInLocalList(this._worldMin))
			{
				SignedInGamer currentGamer = Screen.CurrentGamer;
				string filename = this.MakeFilename();
				int[] data = null;
				try
				{
					CastleMinerZGame.Instance.SaveDevice.Load(filename, delegate(Stream stream)
					{
						BinaryReader reader = new BinaryReader(stream);
						uint version = reader.ReadUInt32();
						int modsToRead;
						if (version == 3203334144U)
						{
							modsToRead = reader.ReadInt32();
							data = new int[modsToRead];
							for (int i = 0; i < modsToRead; i++)
							{
								data[i] = reader.ReadInt32();
							}
							return;
						}
						int totalSize;
						if ((version & 65535U) == 2U)
						{
							stream.Position = 0L;
							totalSize = (int)stream.Length;
						}
						else
						{
							totalSize = (int)version;
						}
						int skipSize = (int)reader.ReadByte();
						for (int j = 0; j < skipSize - 1; j++)
						{
							reader.ReadByte();
						}
						modsToRead = (totalSize - skipSize) / 4;
						data = new int[modsToRead];
						for (int k = 0; k < modsToRead; k++)
						{
							uint s = reader.ReadUInt32();
							uint f = (s & 4278190080U) >> 24;
							f |= (s & 16711680U) >> 8;
							f |= (s & 65280U) << 8;
							f |= (s & 255U) << 24;
							data[k] = (int)f;
						}
					});
				}
				catch (FileNotFoundException)
				{
					data = null;
				}
				catch (Exception)
				{
					data = null;
				}
				this.SetDelta(data, true);
			}
		}

		public void GetChunkFromHost(int priority)
		{
			if (this._loadingPriority < priority)
			{
				this._loadingPriority = priority;
			}
			ChunkCache.Instance.GetChunkFromServer(this._worldMin, this._loadingPriority);
			this._timeOfRequest = CachedChunk.TimeoutTimer.ElapsedMilliseconds;
		}

		public void Save()
		{
			if (!this._saved && ChunkCache.Instance.IsStorageEnabled)
			{
				SignedInGamer currentGamer = Screen.CurrentGamer;
				string fname = this.MakeFilename();
				bool saveWorked = true;
				if (this._numEntries != 0)
				{
					try
					{
						CastleMinerZGame.Instance.SaveDevice.Save(fname, true, true, delegate(Stream stream)
						{
							BinaryWriter writer = new BinaryWriter(stream);
							writer.Write(3203334144U);
							writer.Write(this._numEntries);
							for (int i = 0; i < this._numEntries; i++)
							{
								writer.Write(this._delta[i]);
							}
							writer.Flush();
						});
						saveWorked = true;
					}
					catch (Exception)
					{
						saveWorked = false;
					}
				}
				if (saveWorked)
				{
					this._saved = true;
					this.SameAsDisk = true;
				}
			}
		}

		public void RetroReadFromDisk()
		{
			this.GetChunkFromDisk();
			this.ExecuteCommands();
		}

		public void HostChanged()
		{
			this.GetChunkFromHost(this._loadingPriority);
		}

		public void MaybeRetryFetch()
		{
			double timeSinceRequest = (double)(CachedChunk.TimeoutTimer.ElapsedMilliseconds - this._timeOfRequest) / 1000.0;
			double timeout;
			if (this._loadingPriority > 0)
			{
				if (this._retries < 2)
				{
					timeout = 2.0;
				}
				else
				{
					timeout = 0.5;
				}
			}
			else
			{
				timeout = 20.0;
			}
			if (timeSinceRequest > timeout)
			{
				this._retries++;
				this.GetChunkFromHost(this._loadingPriority);
			}
		}

		public static CachedChunk Alloc()
		{
			return CachedChunk._cache.Get();
		}

		public void Release()
		{
			this._delta = null;
			this._numEntries = 0;
			this._retries = 0;
			this._copy = null;
			CachedChunk._cache.Put(this);
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

		private const double CHUNK_HIGH_PRI_TIMEOUT = 0.5;

		private const double CHUNK_LOW_PRI_TIMEOUT = 20.0;

		private const double CHUNK_HI_PRI_INITIAL_TIMEOUT = 2.0;

		private const int RETRIES_BEFORE_SHORT_TIMEOUT = 2;

		private static Stopwatch TimeoutTimer = Stopwatch.StartNew();

		public IntVector3 _worldMin;

		public int _loadingPriority;

		public long _timeOfRequest;

		public int _retries;

		public int _numEntries;

		public int[] _delta;

		public int[] _copy;

		public bool _sameAsDisk;

		public bool _saved = true;

		public SynchronizedQueue<ChunkCacheCommand> _commandQueue = new SynchronizedQueue<ChunkCacheCommand>();

		private static ObjectCache<CachedChunk> _cache = new ObjectCache<CachedChunk>();

		private ILinkedListNode _nextNode;

		private enum DISK_VERSION : uint
		{
			FIRST_FIXED_VERSION = 3203334144U
		}
	}
}
