using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class SpawnEnemyMessage : CastleMinerZMessage
	{
		private SpawnEnemyMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 pos, EnemyTypeEnum enemyType, float midnight, int id, int seed, Vector3 spawnerPos, int spawnValue = 0, string playerName = null)
		{
			SpawnEnemyMessage Instance = Message.GetSendInstance<SpawnEnemyMessage>();
			Instance.SpawnPosition = pos;
			Instance.SpawnerPosition = spawnerPos;
			Instance.EnemyTypeID = enemyType;
			Instance.EnemyID = id;
			Instance.SpawnValue = spawnValue;
			Instance.RandomSeed = seed;
			if (playerName == null)
			{
				playerName = "";
			}
			Instance.PlayerName = playerName;
			Instance.InitPkg = EnemyType.Types[(int)enemyType].CreateInitPackage(midnight);
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
			this.SpawnPosition = reader.ReadVector3();
			this.SpawnerPosition = reader.ReadVector3();
			this.EnemyTypeID = (EnemyTypeEnum)reader.ReadByte();
			this.EnemyID = reader.ReadInt32();
			this.RandomSeed = reader.ReadInt32();
			this.SpawnValue = reader.ReadInt32();
			this.PlayerName = reader.ReadString();
			this.InitPkg = EnemyType.InitPackage.Read(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.SpawnPosition);
			writer.Write(this.SpawnerPosition);
			writer.Write((byte)this.EnemyTypeID);
			writer.Write(this.EnemyID);
			writer.Write(this.RandomSeed);
			writer.Write(this.SpawnValue);
			writer.Write(this.PlayerName);
			this.InitPkg.Write(writer);
		}

		public Vector3 SpawnPosition;

		public Vector3 SpawnerPosition;

		public EnemyTypeEnum EnemyTypeID;

		public string PlayerName;

		public int EnemyID;

		public int RandomSeed;

		public EnemyType.InitPackage InitPkg;

		public int SpawnValue;
	}
}
