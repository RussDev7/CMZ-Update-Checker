using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class ConsumePickupMessage : CastleMinerZMessage
	{
		private ConsumePickupMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, byte pickerupper, Vector3 pos, int spawnerID, int pickupID, InventoryItem item, bool displayOnPickup)
		{
			ConsumePickupMessage sendInstance = Message.GetSendInstance<ConsumePickupMessage>();
			sendInstance.PickupPosition = pos;
			sendInstance.Item = item;
			sendInstance.PickupID = pickupID;
			sendInstance.SpawnerID = spawnerID;
			sendInstance.PickerUpper = pickerupper;
			sendInstance.DisplayOnPickup = displayOnPickup;
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
			this.PickupPosition = reader.ReadVector3();
			this.PickupID = reader.ReadInt32();
			this.SpawnerID = reader.ReadInt32();
			this.PickerUpper = reader.ReadByte();
			this.DisplayOnPickup = reader.ReadBoolean();
			this.Item = InventoryItem.Create(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.PickupPosition);
			writer.Write(this.PickupID);
			writer.Write(this.SpawnerID);
			writer.Write(this.PickerUpper);
			writer.Write(this.DisplayOnPickup);
			this.Item.Write(writer);
		}

		public Vector3 PickupPosition;

		public InventoryItem Item;

		public byte PickerUpper;

		public int PickupID;

		public int SpawnerID;

		public bool DisplayOnPickup;
	}
}
