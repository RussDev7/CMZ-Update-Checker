using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class KillDragonMessage : CastleMinerZMessage
	{
		private KillDragonMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 location, byte killerid, InventoryItemIDs itemid)
		{
			KillDragonMessage Instance = Message.GetSendInstance<KillDragonMessage>();
			Instance.Location = location;
			Instance.WeaponID = itemid;
			Instance.KillerID = killerid;
			Instance.DoSend(from);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.EnemyMessage;
			}
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
			this.Location = reader.ReadVector3();
			this.WeaponID = (InventoryItemIDs)reader.ReadInt16();
			this.KillerID = reader.ReadByte();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Location);
			writer.Write((short)this.WeaponID);
			writer.Write(this.KillerID);
		}

		public Vector3 Location;

		public InventoryItemIDs WeaponID;

		public byte KillerID;
	}
}
