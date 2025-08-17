using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class SpawnDragonMessage : CastleMinerZMessage
	{
		private SpawnDragonMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, byte spawnerid, DragonTypeEnum enemyType, bool forBiome, float health)
		{
			SpawnDragonMessage sendInstance = Message.GetSendInstance<SpawnDragonMessage>();
			sendInstance.EnemyTypeID = enemyType;
			sendInstance.SpawnerID = spawnerid;
			sendInstance.ForBiome = forBiome;
			sendInstance.Health = health;
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
				return SendDataOptions.ReliableInOrder;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.EnemyTypeID = (DragonTypeEnum)reader.ReadByte();
			this.SpawnerID = reader.ReadByte();
			this.ForBiome = reader.ReadBoolean();
			this.Health = reader.ReadSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write((byte)this.EnemyTypeID);
			writer.Write(this.SpawnerID);
			writer.Write(this.ForBiome);
			writer.Write(this.Health);
		}

		public DragonTypeEnum EnemyTypeID;

		public byte SpawnerID;

		public bool ForBiome;

		public float Health;
	}
}
