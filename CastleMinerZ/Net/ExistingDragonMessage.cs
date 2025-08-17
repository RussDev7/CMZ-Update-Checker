using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class ExistingDragonMessage : CastleMinerZMessage
	{
		private ExistingDragonMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, byte newClientID, DragonTypeEnum enemyType, bool forBiome, float currentHealth)
		{
			ExistingDragonMessage sendInstance = Message.GetSendInstance<ExistingDragonMessage>();
			sendInstance.EnemyTypeID = enemyType;
			sendInstance.NewClientID = newClientID;
			sendInstance.ForBiome = forBiome;
			sendInstance.CurrentHealth = currentHealth;
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
			this.NewClientID = reader.ReadByte();
			this.ForBiome = reader.ReadBoolean();
			this.CurrentHealth = reader.ReadSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write((byte)this.EnemyTypeID);
			writer.Write(this.NewClientID);
			writer.Write(this.ForBiome);
			writer.Write(this.CurrentHealth);
		}

		public DragonTypeEnum EnemyTypeID;

		public byte NewClientID;

		public bool ForBiome;

		public float CurrentHealth;
	}
}
