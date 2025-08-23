using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class KillEnemyMessage : CastleMinerZMessage
	{
		private KillEnemyMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, int enemyid, int targetid, byte killerid, InventoryItemIDs itemid)
		{
			KillEnemyMessage Instance = Message.GetSendInstance<KillEnemyMessage>();
			Instance.EnemyID = enemyid;
			Instance.TargetID = targetid;
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
				return SendDataOptions.Reliable;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.EnemyID = reader.ReadInt32();
			this.TargetID = reader.ReadInt32();
			this.WeaponID = (InventoryItemIDs)reader.ReadInt16();
			this.KillerID = reader.ReadByte();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.EnemyID);
			writer.Write(this.TargetID);
			writer.Write((short)this.WeaponID);
			writer.Write(this.KillerID);
		}

		public int EnemyID;

		public int TargetID;

		public InventoryItemIDs WeaponID;

		public byte KillerID;
	}
}
