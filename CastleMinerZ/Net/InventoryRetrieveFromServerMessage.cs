using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	internal class InventoryRetrieveFromServerMessage : CastleMinerZMessage
	{
		private InventoryRetrieveFromServerMessage()
		{
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public static void Send(LocalNetworkGamer from, Player player, bool isdefault)
		{
			InventoryRetrieveFromServerMessage sendInstance = Message.GetSendInstance<InventoryRetrieveFromServerMessage>();
			sendInstance.Inventory = player.PlayerInventory;
			sendInstance.playerID = player.Gamer.Id;
			sendInstance.Default = isdefault;
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
			this.Inventory = new PlayerInventory((Player)base.Sender.Tag, false);
			this.Inventory.Load(reader);
			this.playerID = reader.ReadByte();
			this.Default = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Inventory.Save(writer);
			writer.Write(this.playerID);
			writer.Write(this.Default);
		}

		public PlayerInventory Inventory;

		public byte playerID;

		public bool Default;
	}
}
