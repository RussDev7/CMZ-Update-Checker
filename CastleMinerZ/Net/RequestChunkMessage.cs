using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	internal class RequestChunkMessage : CastleMinerZMessage
	{
		private RequestChunkMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 blockLocation, int priority)
		{
			RequestChunkMessage Instance = Message.GetSendInstance<RequestChunkMessage>();
			Instance.BlockLocation = blockLocation;
			Instance.Priority = priority;
			Instance.SendToHost(from);
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
			this.Priority = (int)reader.ReadByte();
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.BlockLocation.Write(writer);
			writer.Write((byte)this.Priority);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public IntVector3 BlockLocation;

		public int Priority;
	}
}
