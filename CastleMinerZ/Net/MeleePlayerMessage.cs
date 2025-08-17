using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class MeleePlayerMessage : Message
	{
		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		private MeleePlayerMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, NetworkGamer to, InventoryItemIDs itemID, Vector3 damageSource)
		{
			MeleePlayerMessage sendInstance = Message.GetSendInstance<MeleePlayerMessage>();
			sendInstance.ItemID = itemID;
			sendInstance.DamageSource = damageSource;
			sendInstance.DoSend(from, to);
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write((short)this.ItemID);
			writer.Write(this.DamageSource);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.ItemID = (InventoryItemIDs)reader.ReadInt16();
			this.DamageSource = reader.ReadVector3();
		}

		public InventoryItemIDs ItemID;

		public Vector3 DamageSource;
	}
}
