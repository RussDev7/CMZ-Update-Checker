using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class DestroyCrateMessage : CastleMinerZMessage
	{
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

		private DestroyCrateMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 location)
		{
			DestroyCrateMessage sendInstance = Message.GetSendInstance<DestroyCrateMessage>();
			sendInstance.Location = location;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Location.Write(writer);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = IntVector3.Read(reader);
		}

		public IntVector3 Location;
	}
}
