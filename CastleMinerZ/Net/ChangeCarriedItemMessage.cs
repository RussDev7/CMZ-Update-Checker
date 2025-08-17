using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class ChangeCarriedItemMessage : CastleMinerZMessage
	{
		private ChangeCarriedItemMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, InventoryItemIDs id)
		{
			ChangeCarriedItemMessage sendInstance = Message.GetSendInstance<ChangeCarriedItemMessage>();
			sendInstance.ItemID = id;
			sendInstance.DoSend(from);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.PlayerUpdate;
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
			this.ItemID = (InventoryItemIDs)reader.ReadInt16();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write((short)this.ItemID);
		}

		public InventoryItemIDs ItemID;
	}
}
