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
			IntVector3 intVector;
			intVector.X = (int)(Math.Floor((double)position.X / 16.0) * 16.0);
			intVector.Y = -64;
			intVector.Z = (int)(Math.Floor((double)position.Z / 16.0) * 16.0);
			return intVector;
		}

		public static IntVector3 MakeChunkCornerFromCID(int cid)
		{
			IntVector3 zero = IntVector3.Zero;
			short num = (short)((int)((long)cid & (long)((ulong)(-65536))) >> 16);
			short num2 = (short)((long)cid & 65535L);
			zero.X = (int)(num2 * 16);
			zero.Y = -64;
			zero.Z = (int)(num * 16);
			return zero;
		}

		public static int MakeCIDFromChunkCorner(IntVector3 chunkCorner)
		{
			uint num = (uint)((chunkCorner.Z / 16) & 65535);
			uint num2 = (uint)((chunkCorner.X / 16) & 65535);
			return (int)((num << 16) | num2);
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
			int num;
			if (this._delta == null)
			{
				num = 32;
			}
			else
			{
				num = this._delta.Length + 32;
			}
			this.Resize(num);
		}

		private void Resize(int newsize)
		{
			int[] array = new int[newsize];
			if (this._numEntries > 0)
			{
				Buffer.BlockCopy(this._delta, 0, array, 0, this._numEntries * 4);
			}
			this._delta = array;
		}

		public void AddWorldVector(IntVector3 entry, BlockTypeEnum type)
		{
			this.Add(IntVector3.Subtract(entry, this._worldMin), type);
		}

		public void Add(IntVector3 entry, BlockTypeEnum type)
		{
			int num = DeltaEntry.Create(entry, type);
			for (int i = 0; i < this._numEntries; i++)
			{
				if (DeltaEntry.SameLocation(this._delta[i], num))
				{
					if (this._delta[i] != num)
					{
						this._delta[i] = num;
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
			this._delta[this._numEntries++] = num;
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
				for (ChunkCacheCommand chunkCacheCommand = this._commandQueue.Front; chunkCacheCommand != null; chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand.NextNode)
				{
					if (chunkCacheCommand._command == ChunkCacheCommandEnum.MOD && chunkCacheCommand._worldPosition.Equals(command._worldPosition))
					{
						chunkCacheCommand._blockType = command._blockType;
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
			bool flag = true;
			ChunkCacheCommandEnum command2 = command._command;
			switch (command2)
			{
			case ChunkCacheCommandEnum.MOD:
			{
				IntVector3 intVector = IntVector3.Subtract(command._worldPosition, this._worldMin);
				this.Add(intVector, command._blockType);
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
			flag = false;
			IL_0059:
			if (flag)
			{
				command.Release();
			}
		}

		public void StripFetchCommands()
		{
			ChunkCacheCommand chunkCacheCommand = null;
			while (!this._commandQueue.Empty)
			{
				ChunkCacheCommand chunkCacheCommand2 = this._commandQueue.Dequeue();
				if (chunkCacheCommand2._command == ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN)
				{
					chunkCacheCommand2._command = ChunkCacheCommandEnum.RESETWAITINGCHUNKS;
					chunkCacheCommand2._callback(chunkCacheCommand2);
					if (this._loadingPriority > 0)
					{
						this._loadingPriority = 0;
					}
				}
				else
				{
					chunkCacheCommand2.NextNode = chunkCacheCommand;
					chunkCacheCommand = chunkCacheCommand2;
				}
			}
			while (chunkCacheCommand != null)
			{
				ChunkCacheCommand chunkCacheCommand2 = chunkCacheCommand;
				chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand2.NextNode;
				chunkCacheCommand2.NextNode = null;
				this._commandQueue.Undequeue(chunkCacheCommand2);
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
				string text = this.MakeFilename();
				int[] data = null;
				try
				{
					CastleMinerZGame.Instance.SaveDevice.Load(text, delegate(Stream stream)
					{
						BinaryReader binaryReader = new BinaryReader(stream);
						uint num = binaryReader.ReadUInt32();
						int num2;
						if (num == 3203334144U)
						{
							num2 = binaryReader.ReadInt32();
							data = new int[num2];
							for (int i = 0; i < num2; i++)
							{
								data[i] = binaryReader.ReadInt32();
							}
							return;
						}
						int num3;
						if ((num & 65535U) == 2U)
						{
							stream.Position = 0L;
							num3 = (int)stream.Length;
						}
						else
						{
							num3 = (int)num;
						}
						int num4 = (int)binaryReader.ReadByte();
						for (int j = 0; j < num4 - 1; j++)
						{
							binaryReader.ReadByte();
						}
						num2 = (num3 - num4) / 4;
						data = new int[num2];
						for (int k = 0; k < num2; k++)
						{
							uint num5 = binaryReader.ReadUInt32();
							uint num6 = (num5 & 4278190080U) >> 24;
							num6 |= (num5 & 16711680U) >> 8;
							num6 |= (num5 & 65280U) << 8;
							num6 |= (num5 & 255U) << 24;
							data[k] = (int)num6;
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
				string text = this.MakeFilename();
				bool flag = true;
				if (this._numEntries != 0)
				{
					try
					{
						CastleMinerZGame.Instance.SaveDevice.Save(text, true, true, delegate(Stream stream)
						{
							BinaryWriter binaryWriter = new BinaryWriter(stream);
							binaryWriter.Write(3203334144U);
							binaryWriter.Write(this._numEntries);
							for (int i = 0; i < this._numEntries; i++)
							{
								binaryWriter.Write(this._delta[i]);
							}
							binaryWriter.Flush();
						});
						flag = true;
					}
					catch (Exception)
					{
						flag = false;
					}
				}
				if (flag)
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
			double num = (double)(CachedChunk.TimeoutTimer.ElapsedMilliseconds - this._timeOfRequest) / 1000.0;
			double num2;
			if (this._loadingPriority > 0)
			{
				if (this._retries < 2)
				{
					num2 = 2.0;
				}
				else
				{
					num2 = 0.5;
				}
			}
			else
			{
				num2 = 20.0;
			}
			if (num > num2)
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
