using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	internal class DoorOpenCloseMessage : CastleMinerZMessage
	{
		private DoorOpenCloseMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 blockLocaion, bool opened)
		{
			DoorOpenCloseMessage sendInstance = Message.GetSendInstance<DoorOpenCloseMessage>();
			sendInstance.Location = blockLocaion;
			sendInstance.Opened = opened;
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
			this.Location = IntVector3.Read(reader);
			this.Opened = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Location.Write(writer);
			writer.Write(this.Opened);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public IntVector3 Location;

		public bool Opened;
	}
}
