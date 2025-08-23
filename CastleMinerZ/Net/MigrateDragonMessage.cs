using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class MigrateDragonMessage : CastleMinerZMessage
	{
		private MigrateDragonMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, byte targetID, DragonHostMigrationInfo miginfo)
		{
			MigrateDragonMessage Instance = Message.GetSendInstance<MigrateDragonMessage>();
			Instance.MigrationInfo = miginfo;
			Instance.TargetID = targetID;
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
			this.MigrationInfo = DragonHostMigrationInfo.Read(reader);
			this.TargetID = reader.ReadByte();
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.MigrationInfo.Write(writer);
			writer.Write(this.TargetID);
		}

		public DragonHostMigrationInfo MigrationInfo;

		public byte TargetID;
	}
}
