using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class DetonateRocketMessage : CastleMinerZMessage
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
				return SendDataOptions.Reliable;
			}
		}

		private DetonateRocketMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 location, ExplosiveTypes explosiveType, InventoryItemIDs itemType, bool hitDragon)
		{
			DetonateRocketMessage Instance = Message.GetSendInstance<DetonateRocketMessage>();
			Instance.Location = location;
			Instance.HitDragon = hitDragon;
			Instance.ExplosiveType = explosiveType;
			Instance.ItemType = itemType;
			Instance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Location);
			writer.Write(this.HitDragon);
			writer.Write((byte)this.ExplosiveType);
			writer.Write((byte)this.ItemType);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = reader.ReadVector3();
			this.HitDragon = reader.ReadBoolean();
			this.ExplosiveType = (ExplosiveTypes)reader.ReadByte();
			this.ItemType = (InventoryItemIDs)reader.ReadByte();
		}

		public Vector3 Location;

		public ExplosiveTypes ExplosiveType;

		public InventoryItemIDs ItemType;

		public bool HitDragon;
	}
}
