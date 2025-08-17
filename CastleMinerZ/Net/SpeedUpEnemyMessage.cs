using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class SpeedUpEnemyMessage : CastleMinerZMessage
	{
		private SpeedUpEnemyMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, int enemyid, int targetid)
		{
			SpeedUpEnemyMessage sendInstance = Message.GetSendInstance<SpeedUpEnemyMessage>();
			sendInstance.EnemyID = enemyid;
			sendInstance.TargetID = targetid;
			sendInstance.DoSend(from);
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
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.EnemyID);
			writer.Write(this.TargetID);
		}

		public int EnemyID;

		public int TargetID;
	}
}
