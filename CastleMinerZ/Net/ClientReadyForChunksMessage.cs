using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class ClientReadyForChunksMessage : CastleMinerZMessage
	{
		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		private ClientReadyForChunksMessage()
		{
		}

		public static void Send(LocalNetworkGamer from)
		{
			ClientReadyForChunksMessage sendInstance = Message.GetSendInstance<ClientReadyForChunksMessage>();
			sendInstance.SendToHost(from);
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		protected override void SendData(BinaryWriter writer)
		{
		}

		protected override void RecieveData(BinaryReader reader)
		{
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}
	}
}
