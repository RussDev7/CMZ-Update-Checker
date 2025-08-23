using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class RestartLevelMessage : CastleMinerZMessage
	{
		public override bool Echo
		{
			get
			{
				return true;
			}
		}

		private RestartLevelMessage()
		{
		}

		public static void Send(LocalNetworkGamer from)
		{
			RestartLevelMessage Instance = Message.GetSendInstance<RestartLevelMessage>();
			Instance.DoSend(from);
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

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}
	}
}
