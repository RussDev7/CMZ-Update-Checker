using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	internal class RequestInventoryMessage : CastleMinerZMessage
	{
		private RequestInventoryMessage()
		{
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public static void Send(LocalNetworkGamer from)
		{
			RequestInventoryMessage Instance = Message.GetSendInstance<RequestInventoryMessage>();
			Instance.DoSend(from);
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
	}
}
