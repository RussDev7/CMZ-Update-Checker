using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class BroadcastTextMessage : CastleMinerZMessage
	{
		private BroadcastTextMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, string message)
		{
			BroadcastTextMessage Instance = DNA.Net.Message.GetSendInstance<BroadcastTextMessage>();
			Instance.Message = message;
			Instance.DoSend(from);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Message = reader.ReadString();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Message);
		}

		public string Message;
	}
}
