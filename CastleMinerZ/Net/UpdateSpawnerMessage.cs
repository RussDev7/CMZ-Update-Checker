using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class UpdateSpawnerMessage : CastleMinerZMessage
	{
		private UpdateSpawnerMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 pos, bool isStarted)
		{
			UpdateSpawnerMessage sendInstance = Message.GetSendInstance<UpdateSpawnerMessage>();
			sendInstance.SpawnerPosition = pos;
			sendInstance.IsStarted = isStarted;
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
			this.SpawnerPosition = reader.ReadVector3();
			this.IsStarted = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.SpawnerPosition);
			writer.Write(this.IsStarted);
		}

		public Vector3 SpawnerPosition;

		public EnemyTypeEnum EnemyTypeID;

		public int EnemyID;

		public int RandomSeed;

		public bool IsStarted;

		public EnemyType.InitPackage InitPkg;
	}
}
