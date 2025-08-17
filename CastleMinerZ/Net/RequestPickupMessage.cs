using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class RequestPickupMessage : CastleMinerZMessage
	{
		private RequestPickupMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, int spawnerID, int pickupID)
		{
			RequestPickupMessage sendInstance = Message.GetSendInstance<RequestPickupMessage>();
			sendInstance.PickupID = pickupID;
			sendInstance.SpawnerID = spawnerID;
			sendInstance.DoSend(from);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.PickupMessage;
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
			this.PickupID = reader.ReadInt32();
			this.SpawnerID = reader.ReadInt32();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.PickupID);
			writer.Write(this.SpawnerID);
		}

		public int PickupID;

		public int SpawnerID;
	}
}
