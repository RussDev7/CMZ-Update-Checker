using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class RemoveDragonMessage : CastleMinerZMessage
	{
		private RemoveDragonMessage()
		{
		}

		public static void Send(LocalNetworkGamer from)
		{
			RemoveDragonMessage sendInstance = Message.GetSendInstance<RemoveDragonMessage>();
			sendInstance.DoSend(from);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.EnemyMessage;
			}
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
		}

		protected override void SendData(BinaryWriter writer)
		{
		}
	}
}
