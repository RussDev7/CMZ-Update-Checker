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
			int num = 32768;
			int num2 = 0;
			lock (this._queue)
			{
				while (!this._queue.Empty && num2 < num)
				{
					MainThreadMessageSender.MessageCommand messageCommand = this._queue.Dequeue();
					if (messageCommand._type == MainThreadMessageSender.MessageType.SENDCHUNK && messageCommand._delta != null)
					{
						num2 += messageCommand._delta.Length * 4;
					}
					this._commandsToSend.Queue(messageCommand);
				}
				goto IL_00BD;
			}
			IL_0071:
			MainThreadMessageSender.MessageCommand messageCommand2 = this._commandsToSend.Dequeue();
			if (!messageCommand2.Execute())
			{
				if (messageCommand2.CanRetry())
				{
					messageCommand2._retryCount++;
					this._queue.Queue(messageCommand2);
				}
				else
				{
					if (Debugger.IsAttached)
					{
						messageCommand2.WrongFrame();
					}
					messageCommand2.Release();
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
				for (MainThreadMessageSender.MessageCommand messageCommand = this._queue.Front; messageCommand != null; messageCommand = (MainThreadMessageSender.MessageCommand)messageCommand.NextNode)
				{
					if (messageCommand._type == MainThreadMessageSender.MessageType.GETCHUNK && pos.Equals(messageCommand._position))
					{
						if (messageCommand._priority < priority)
						{
							messageCommand._priority = priority;
							this._queue.Remove(messageCommand);
							this.InsertInQueue(messageCommand);
						}
						return;
					}
				}
			}
			MainThreadMessageSender.MessageCommand messageCommand2 = MainThreadMessageSender.MessageCommand.Alloc();
			messageCommand2._type = MainThreadMessageSender.MessageType.GETCHUNK;
			messageCommand2._position = pos;
			messageCommand2._priority = priority;
			this.InsertInQueue(messageCommand2);
		}

		public void ClientReadyForChunks()
		{
			MainThreadMessageSender.MessageCommand messageCommand = MainThreadMessageSender.MessageCommand.Alloc();
			messageCommand._type = MainThreadMessageSender.MessageType.BROADCASTREADY;
			messageCommand._priority = 1;
			this.InsertInQueue(messageCommand);
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
					MainThreadMessageSender.MessageCommand messageCommand = null;
					MainThreadMessageSender.MessageCommand messageCommand2 = this._queue.Front;
					while (messageCommand2._priority != 0)
					{
						messageCommand = messageCommand2;
						messageCommand2 = (MainThreadMessageSender.MessageCommand)messageCommand2.NextNode;
						if (messageCommand2 == null)
						{
							this._queue.Queue(c);
							goto IL_00A7;
						}
					}
					if (messageCommand != null)
					{
						messageCommand.NextNode = c;
						c.NextNode = messageCommand2;
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
				MainThreadMessageSender.MessageCommand messageCommand = this._queue.Front;
				while (messageCommand != null)
				{
					if (messageCommand._type == MainThreadMessageSender.MessageType.SENDCHUNK && pos.Equals(messageCommand._position))
					{
						messageCommand._delta = delta;
						if (messageCommand._priority < priority)
						{
							messageCommand._priority = priority;
							this._queue.Remove(messageCommand);
							this.InsertInQueue(messageCommand);
						}
						if (messageCommand._sendToAll)
						{
							return;
						}
						messageCommand.CopyReceiversToMe(receiverid, numReceivers, (byte)priority);
						return;
					}
					else
					{
						messageCommand = (MainThreadMessageSender.MessageCommand)messageCommand.NextNode;
					}
				}
			}
			MainThreadMessageSender.MessageCommand messageCommand2 = MainThreadMessageSender.MessageCommand.Alloc();
			messageCommand2._type = MainThreadMessageSender.MessageType.SENDCHUNK;
			messageCommand2._position = pos;
			messageCommand2._delta = delta;
			messageCommand2._priority = priority;
			messageCommand2._numRecipients = 0;
			messageCommand2.CopyReceiversToMe(receiverid, numReceivers, (byte)priority);
			this.InsertInQueue(messageCommand2);
		}

		public void SendDeltaListMessage(int[] deltaList, byte receiverid, bool toall)
		{
			lock (this._queue)
			{
				MainThreadMessageSender.MessageCommand messageCommand = this._queue.Front;
				while (messageCommand != null)
				{
					if (messageCommand._type == MainThreadMessageSender.MessageType.SENDDELTALIST)
					{
						messageCommand._delta = deltaList;
						if (messageCommand._sendToAll)
						{
							return;
						}
						if (toall)
						{
							messageCommand._sendToAll = true;
							messageCommand._numRecipients = 0;
						}
						else
						{
							for (int i = 0; i < messageCommand._numRecipients; i++)
							{
								if (messageCommand._recipients[i] == receiverid)
								{
									return;
								}
							}
							messageCommand._recipients[messageCommand._numRecipients++] = receiverid;
						}
						return;
					}
					else
					{
						messageCommand = (MainThreadMessageSender.MessageCommand)messageCommand.NextNode;
					}
				}
			}
			MainThreadMessageSender.MessageCommand messageCommand2 = MainThreadMessageSender.MessageCommand.Alloc();
			messageCommand2._type = MainThreadMessageSender.MessageType.SENDDELTALIST;
			messageCommand2._delta = deltaList;
			messageCommand2._priority = 1;
			if (toall)
			{
				messageCommand2._sendToAll = true;
			}
			else
			{
				messageCommand2._numRecipients = 1;
				messageCommand2._recipients[0] = receiverid;
			}
			this.InsertInQueue(messageCommand2);
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
				bool flag = false;
				for (int i = 0; i < numRecipients; i++)
				{
					bool flag2 = false;
					int j = 0;
					while (j < this._numRecipients)
					{
						if (this._recipients[j] == recipients[i])
						{
							flag2 = true;
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
					if (!flag2)
					{
						flag = true;
						this._recipients[this._numRecipients] = recipients[i];
						this._priorities[this._numRecipients++] = priority;
					}
				}
				return flag;
			}

			public bool Execute()
			{
				bool flag = false;
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
									NetworkGamer gamerFromID = CastleMinerZGame.Instance.GetGamerFromID(this._recipients[i]);
									if (gamerFromID != null && !gamerFromID.HasLeftSession)
									{
										DNA.CastleMinerZ.Net.ProvideChunkMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, gamerFromID, this._position, this._delta, (int)this._priorities[i]);
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
									NetworkGamer gamerFromID2 = CastleMinerZGame.Instance.GetGamerFromID(this._recipients[j]);
									if (gamerFromID2 != null && !gamerFromID2.HasLeftSession)
									{
										ProvideDeltaListMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, gamerFromID2, this._delta);
									}
								}
							}
							break;
						}
					}
					this.Release();
					flag = true;
				}
				catch (Exception)
				{
				}
				return flag;
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
				MainThreadMessageSender.MessageCommand messageCommand = MainThreadMessageSender.MessageCommand._cache.Get();
				messageCommand._retryCount = 0;
				messageCommand._retryFrame = MainThreadMessageSender.MessageCommand.retryFrame;
				messageCommand._priority = 0;
				messageCommand._numRecipients = 0;
				messageCommand._sendToAll = false;
				return messageCommand;
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
