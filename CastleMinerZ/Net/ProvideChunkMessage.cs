using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	internal class ProvideChunkMessage : CastleMinerZMessage
	{
		private ProvideChunkMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, NetworkGamer recipient, IntVector3 blockLocaion, int[] delta, int priority)
		{
			ProvideChunkMessage Instance = Message.GetSendInstance<ProvideChunkMessage>();
			Instance.BlockLocation = blockLocaion;
			Instance.Priority = (byte)priority;
			Instance.Delta = delta;
			if (recipient == null)
			{
				Instance.DoSend(from);
				return;
			}
			Instance.DoSend(from, recipient);
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.BlockLocation = IntVector3.Read(reader);
			this.Priority = reader.ReadByte();
			int dwordCount = reader.ReadInt32();
			if (dwordCount > 0)
			{
				this.Delta = new int[dwordCount];
				for (int i = 0; i < dwordCount; i++)
				{
					this.Delta[i] = reader.ReadInt32();
				}
				return;
			}
			this.Delta = null;
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.BlockLocation.Write(writer);
			writer.Write(this.Priority);
			int dwordCount = ((this.Delta == null) ? 0 : this.Delta.Length);
			writer.Write(dwordCount);
			if (dwordCount != 0)
			{
				for (int i = 0; i < dwordCount; i++)
				{
					writer.Write(this.Delta[i]);
				}
			}
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public IntVector3 BlockLocation;

		public byte Priority;

		public int[] Delta;
	}
}
