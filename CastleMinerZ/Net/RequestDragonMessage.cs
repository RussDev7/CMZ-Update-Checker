using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class RequestDragonMessage : CastleMinerZMessage
	{
		private RequestDragonMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, DragonTypeEnum enemyType, bool forBiome)
		{
			RequestDragonMessage Instance = Message.GetSendInstance<RequestDragonMessage>();
			Instance.EnemyTypeID = enemyType;
			Instance.ForBiome = forBiome;
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
			this.EnemyTypeID = (DragonTypeEnum)reader.ReadByte();
			this.ForBiome = reader.ReadBoolean();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write((byte)this.EnemyTypeID);
			writer.Write(this.ForBiome);
		}

		public DragonTypeEnum EnemyTypeID;

		public bool ForBiome;
	}
}
