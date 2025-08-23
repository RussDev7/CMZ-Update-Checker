using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class ItemCrateMessage : CastleMinerZMessage
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

		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		private ItemCrateMessage()
		{
		}

		public void Apply(WorldInfo info)
		{
			Crate crate = info.GetCrate(this.Location, true);
			crate.Inventory[this.Index] = this.Item;
		}

		public static void Send(LocalNetworkGamer from, InventoryItem item, Crate crate, int index)
		{
			crate.Inventory[index] = item;
			ItemCrateMessage Instance = Message.GetSendInstance<ItemCrateMessage>();
			Instance.Location = crate.Location;
			Instance.Index = index;
			Instance.Item = item;
			Instance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Location.Write(writer);
			writer.Write(this.Index);
			if (this.Item != null)
			{
				writer.Write(true);
				this.Item.Write(writer);
				return;
			}
			writer.Write(false);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = IntVector3.Read(reader);
			this.Index = reader.ReadInt32();
			if (reader.ReadBoolean())
			{
				this.Item = InventoryItem.Create(reader);
				return;
			}
			this.Item = null;
		}

		public IntVector3 Location;

		public int Index;

		public InventoryItem Item;
	}
}
