using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class CreatePickupMessage : CastleMinerZMessage
	{
		private CreatePickupMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 pos, Vector3 vec, int pickupID, InventoryItem item, bool dropped, bool displayOnPickup)
		{
			CreatePickupMessage sendInstance = Message.GetSendInstance<CreatePickupMessage>();
			sendInstance.SpawnPosition = pos;
			sendInstance.SpawnVector = vec;
			sendInstance.Item = item;
			sendInstance.Dropped = dropped;
			sendInstance.DisplayOnPickup = displayOnPickup;
			sendInstance.PickupID = pickupID;
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
			this.SpawnPosition = reader.ReadVector3();
			this.SpawnVector = reader.ReadVector3();
			this.PickupID = reader.ReadInt32();
			this.Item = InventoryItem.Create(reader);
			this.Dropped = reader.ReadBoolean();
			this.DisplayOnPickup = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.SpawnPosition);
			writer.Write(this.SpawnVector);
			writer.Write(this.PickupID);
			this.Item.Write(writer);
			writer.Write(this.Dropped);
			writer.Write(this.DisplayOnPickup);
		}

		public Vector3 SpawnPosition;

		public Vector3 SpawnVector;

		public InventoryItem Item;

		public int PickupID;

		public bool Dropped;

		public bool DisplayOnPickup;
	}
}
