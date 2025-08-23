using System;
using System.Diagnostics;
using System.Threading;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Utils;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ
{
	public class MainThreadMessageSender
	{
		public static void Init()
		{
			if (MainThreadMessageSender.Instance == null)
			{
				MainThreadMessageSender.Instance = new MainThreadMessageSender();
			}
		}

		public void DrainQueue()
		{
			int memtosend = 32768;
			int memcommitted = 0;
			lock (this._queue)
			{
				while (!this._queue.Empty && memcommitted < memtosend)
				{
					MainThreadMessageSender.MessageCommand msg = this._queue.Dequeue();
					if (msg._type == MainThreadMessageSender.MessageType.SENDCHUNK && msg._delta != null)
					{
						memcommitted += msg._delta.Length * 4;
					}
					this._commandsToSend.Queue(msg);
				}
				goto IL_00BD;
			}
			IL_0071:
			MainThreadMessageSender.MessageCommand walker = this._commandsToSend.Dequeue();
			if (!walker.Execute())
			{
				if (walker.CanRetry())
				{
					walker._retryCount++;
					this._queue.Queue(walker);
				}
				else
				{
					if (Debugger.IsAttached)
					{
						walker.WrongFrame();
					}
					walker.Release();
				}
			}
			IL_00BD:
			if (this._commandsToSend.Empty)
			{
				return;
			}
			goto IL_0071;
		}

		public void GameOver()
		{
			MainThreadMessageSender.MessageCommand.GameOver();
		}

		public void RequestChunkMessage(IntVector3 pos, int priority)
		{
			lock (this._queue)
			{
				for (MainThreadMessageSender.MessageCommand walker = this._queue.Front; walker != null; walker = (MainThreadMessageSender.MessageCommand)walker.NextNode)
				{
					if (walker._type == MainThreadMessageSender.MessageType.GETCHUNK && pos.Equals(walker._position))
					{
						if (walker._priority < priority)
						{
							walker._priority = priority;
							this._queue.Remove(walker);
							this.InsertInQueue(walker);
						}
						return;
					}
				}
			}
			MainThreadMessageSender.MessageCommand mc = MainThreadMessageSender.MessageCommand.Alloc();
			mc._type = MainThreadMessageSender.MessageType.GETCHUNK;
			mc._position = pos;
			mc._priority = priority;
			this.InsertInQueue(mc);
		}

		public void ClientReadyForChunks()
		{
			MainThreadMessageSender.MessageCommand mc = MainThreadMessageSender.MessageCommand.Alloc();
			mc._type = MainThreadMessageSender.MessageType.BROADCASTREADY;
			mc._priority = 1;
			this.InsertInQueue(mc);
		}

		private void InsertInQueue(MainThreadMessageSender.MessageCommand c)
		{
			lock (this._queue)
			{
				if (c._priority == 0 || this._queue.Empty || this._queue.Back._priority == 1)
				{
					this._queue.Queue(c);
				}
				else
				{
					MainThreadMessageSender.MessageCommand prev = null;
					MainThreadMessageSender.MessageCommand walker = this._queue.Front;
					while (walker._priority != 0)
					{
						prev = walker;
						walker = (MainThreadMessageSender.MessageCommand)walker.NextNode;
						if (walker == null)
						{
							this._queue.Queue(c);
							goto IL_00A7;
						}
					}
					if (prev != null)
					{
						prev.NextNode = c;
						c.NextNode = walker;
						this._queue.IncrementCountAfterInsertion();
					}
					else
					{
						this._queue.Undequeue(c);
					}
				}
				IL_00A7:;
			}
		}

		public void ProvideChunkMessage(IntVector3 pos, int[] delta, int priority, int numReceivers, byte[] receiverid)
		{
			lock (this._queue)
			{
				MainThreadMessageSender.MessageCommand walker = this._queue.Front;
				while (walker != null)
				{
					if (walker._type == MainThreadMessageSender.MessageType.SENDCHUNK && pos.Equals(walker._position))
					{
						walker._delta = delta;
						if (walker._priority < priority)
						{
							walker._priority = priority;
							this._queue.Remove(walker);
							this.InsertInQueue(walker);
						}
						if (walker._sendToAll)
						{
							return;
						}
						walker.CopyReceiversToMe(receiverid, numReceivers, (byte)priority);
						return;
					}
					else
					{
						walker = (MainThreadMessageSender.MessageCommand)walker.NextNode;
					}
				}
			}
			MainThreadMessageSender.MessageCommand mc = MainThreadMessageSender.MessageCommand.Alloc();
			mc._type = MainThreadMessageSender.MessageType.SENDCHUNK;
			mc._position = pos;
			mc._delta = delta;
			mc._priority = priority;
			mc._numRecipients = 0;
			mc.CopyReceiversToMe(receiverid, numReceivers, (byte)priority);
			this.InsertInQueue(mc);
		}

		public void SendDeltaListMessage(int[] deltaList, byte receiverid, bool toall)
		{
			lock (this._queue)
			{
				MainThreadMessageSender.MessageCommand walker = this._queue.Front;
				while (walker != null)
				{
					if (walker._type == MainThreadMessageSender.MessageType.SENDDELTALIST)
					{
						walker._delta = deltaList;
						if (walker._sendToAll)
						{
							return;
						}
						if (toall)
						{
							walker._sendToAll = true;
							walker._numRecipients = 0;
						}
						else
						{
							for (int i = 0; i < walker._numRecipients; i++)
							{
								if (walker._recipients[i] == receiverid)
								{
									return;
								}
							}
							walker._recipients[walker._numRecipients++] = receiverid;
						}
						return;
					}
					else
					{
						walker = (MainThreadMessageSender.MessageCommand)walker.NextNode;
					}
				}
			}
			MainThreadMessageSender.MessageCommand mc = MainThreadMessageSender.MessageCommand.Alloc();
			mc._type = MainThreadMessageSender.MessageType.SENDDELTALIST;
			mc._delta = deltaList;
			mc._priority = 1;
			if (toall)
			{
				mc._sendToAll = true;
			}
			else
			{
				mc._numRecipients = 1;
				mc._recipients[0] = receiverid;
			}
			this.InsertInQueue(mc);
		}

		public static MainThreadMessageSender Instance;

		private SynchronizedQueue<MainThreadMessageSender.MessageCommand> _queue = new SynchronizedQueue<MainThreadMessageSender.MessageCommand>();

		private SimpleQueue<MainThreadMessageSender.MessageCommand> _commandsToSend = new SimpleQueue<MainThreadMessageSender.MessageCommand>();

		private enum MessageType
		{
			SENDCHUNK,
			BROADCASTREADY,
			GETCHUNK,
			SENDDELTALIST
		}

		private class MessageCommand : IReleaseable, ILinkedListNode
		{
			private bool Contains(byte recipient)
			{
				if (this._sendToAll)
				{
					return true;
				}
				for (int i = 0; i < this._numRecipients; i++)
				{
					if (this._recipients[i] == recipient)
					{
						return true;
					}
				}
				return false;
			}

			public bool CopyReceiversToMe(byte[] recipients, int numRecipients, byte priority)
			{
				if (this._sendToAll)
				{
					return false;
				}
				bool copiedOne = false;
				for (int i = 0; i < numRecipients; i++)
				{
					bool foundIt = false;
					int j = 0;
					while (j < this._numRecipients)
					{
						if (this._recipients[j] == recipients[i])
						{
							foundIt = true;
							if (this._priority > (int)this._priorities[j])
							{
								this._priorities[j] = priority;
								break;
							}
							break;
						}
						else
						{
							j++;
						}
					}
					if (!foundIt)
					{
						copiedOne = true;
						this._recipients[this._numRecipients] = recipients[i];
						this._priorities[this._numRecipients++] = priority;
					}
				}
				return copiedOne;
			}

			public bool Execute()
			{
				bool result = false;
				try
				{
					if (CastleMinerZGame.Instance != null)
					{
						switch (this._type)
						{
						case MainThreadMessageSender.MessageType.SENDCHUNK:
							if (this._sendToAll)
							{
								DNA.CastleMinerZ.Net.ProvideChunkMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, null, this._position, this._delta, this._priority);
							}
							else
							{
								for (int i = 0; i < this._numRecipients; i++)
								{
									NetworkGamer recipient = CastleMinerZGame.Instance.GetGamerFromID(this._recipients[i]);
									if (recipient != null && !recipient.HasLeftSession)
									{
										DNA.CastleMinerZ.Net.ProvideChunkMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, recipient, this._position, this._delta, (int)this._priorities[i]);
									}
								}
							}
							break;
						case MainThreadMessageSender.MessageType.BROADCASTREADY:
							ClientReadyForChunksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer);
							break;
						case MainThreadMessageSender.MessageType.GETCHUNK:
							DNA.CastleMinerZ.Net.RequestChunkMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, this._position, this._priority);
							break;
						case MainThreadMessageSender.MessageType.SENDDELTALIST:
							if (this._sendToAll)
							{
								ProvideDeltaListMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, null, this._delta);
							}
							else
							{
								for (int j = 0; j < this._numRecipients; j++)
								{
									NetworkGamer recipient2 = CastleMinerZGame.Instance.GetGamerFromID(this._recipients[j]);
									if (recipient2 != null && !recipient2.HasLeftSession)
									{
										ProvideDeltaListMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, recipient2, this._delta);
									}
								}
							}
							break;
						}
					}
					this.Release();
					result = true;
				}
				catch (Exception)
				{
				}
				return result;
			}

			public static void GameOver()
			{
				Interlocked.Increment(ref MainThreadMessageSender.MessageCommand.retryFrame);
			}

			public bool CanRetry()
			{
				return this._retryCount < 10 && !this.WrongFrame();
			}

			public bool WrongFrame()
			{
				return this._retryFrame != MainThreadMessageSender.MessageCommand.retryFrame;
			}

			public static MainThreadMessageSender.MessageCommand Alloc()
			{
				MainThreadMessageSender.MessageCommand result = MainThreadMessageSender.MessageCommand._cache.Get();
				result._retryCount = 0;
				result._retryFrame = MainThreadMessageSender.MessageCommand.retryFrame;
				result._priority = 0;
				result._numRecipients = 0;
				result._sendToAll = false;
				return result;
			}

			public void Release()
			{
				this._delta = null;
				this._nextNode = null;
				MainThreadMessageSender.MessageCommand._cache.Put(this);
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

			private static int retryFrame = 0;

			public MainThreadMessageSender.MessageType _type;

			public IntVector3 _position;

			public int[] _delta;

			public int _retryCount;

			public int _retryFrame;

			public int _priority;

			public byte[] _recipients = new byte[16];

			public byte[] _priorities = new byte[16];

			public int _numRecipients;

			public bool _sendToAll;

			private static ObjectCache<MainThreadMessageSender.MessageCommand> _cache = new ObjectCache<MainThreadMessageSender.MessageCommand>();

			private ILinkedListNode _nextNode;
		}
	}
}
