using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class AppointServerMessage : CastleMinerZMessage
	{
		private AppointServerMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, byte playerID)
		{
			AppointServerMessage sendInstance = Message.GetSendInstance<AppointServerMessage>();
			sendInstance.PlayerID = playerID;
			sendInstance.DoSend(from);
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
			this.PlayerID = reader.ReadByte();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.PlayerID);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public byte PlayerID;
	}
}
