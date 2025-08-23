using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class RequestWorldInfoMessage : CastleMinerZMessage
	{
		private RequestWorldInfoMessage()
		{
		}

		public static void Send(LocalNetworkGamer from)
		{
			RequestWorldInfoMessage Instance = Message.GetSendInstance<RequestWorldInfoMessage>();
			Instance.DoSend(from, CastleMinerZGame.Instance.CurrentNetworkSession.Host);
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
		}

		protected override void SendData(BinaryWriter writer)
		{
		}
	}
}
