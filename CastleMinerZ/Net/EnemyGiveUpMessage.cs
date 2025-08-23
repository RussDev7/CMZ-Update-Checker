using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class EnemyGiveUpMessage : CastleMinerZMessage
	{
		private EnemyGiveUpMessage()
		{
		}

		public static void Send(int enemyid, int targetid)
		{
			EnemyGiveUpMessage Instance = Message.GetSendInstance<EnemyGiveUpMessage>();
			Instance.EnemyID = enemyid;
			Instance.TargetID = targetid;
			Instance.DoSend(CastleMinerZGame.Instance.LocalPlayer.Gamer as LocalNetworkGamer);
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
