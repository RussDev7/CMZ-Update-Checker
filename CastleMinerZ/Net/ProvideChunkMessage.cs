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
			ProvideChunkMessage sendInstance = Message.GetSendInstance<ProvideChunkMessage>();
			sendInstance.BlockLocation = blockLocaion;
			sendInstance.Priority = (byte)priority;
			sendInstance.Delta = delta;
			if (recipient == null)
			{
				sendInstance.DoSend(from);
				return;
			}
			sendInstance.DoSend(from, recipient);
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
			int num = reader.ReadInt32();
			if (num > 0)
			{
				this.Delta = new int[num];
				for (int i = 0; i < num; i++)
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
			int num = ((this.Delta == null) ? 0 : this.Delta.Length);
			writer.Write(num);
			if (num != 0)
			{
				for (int i = 0; i < num; i++)
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
