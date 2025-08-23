using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	internal class InventoryStoreOnServerMessage : CastleMinerZMessage
	{
		private InventoryStoreOnServerMessage()
		{
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public static void Send(LocalNetworkGamer from, PlayerInventory playerInventory, bool final)
		{
			InventoryStoreOnServerMessage Instance = Message.GetSendInstance<InventoryStoreOnServerMessage>();
			Instance.Inventory = playerInventory;
			Instance.FinalSave = final;
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
			this.Inventory = new PlayerInventory((Player)base.Sender.Tag, false);
			this.Inventory.Load(reader);
			this.FinalSave = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Inventory.Save(writer);
			writer.Write(this.FinalSave);
		}

		public PlayerInventory Inventory;

		public bool FinalSave;
	}
}
