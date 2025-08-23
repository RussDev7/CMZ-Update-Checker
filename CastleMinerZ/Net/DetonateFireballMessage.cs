using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class DetonateFireballMessage : CastleMinerZMessage
	{
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

		private DetonateFireballMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 location, int index, int numblocks, IntVector3[] blocks, DragonTypeEnum dragonType)
		{
			DetonateFireballMessage Instance = Message.GetSendInstance<DetonateFireballMessage>();
			Instance.Location = location;
			Instance.Index = index;
			Instance.NumBlocks = (byte)numblocks;
			Instance.BlocksToRemove = blocks;
			Instance.EType = dragonType;
			Instance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Location);
			writer.Write(this.Index);
			writer.Write((byte)this.EType);
			writer.Write(this.NumBlocks);
			for (int i = 0; i < (int)this.NumBlocks; i++)
			{
				this.BlocksToRemove[i].Write(writer);
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = reader.ReadVector3();
			this.Index = reader.ReadInt32();
			this.EType = (DragonTypeEnum)reader.ReadByte();
			this.NumBlocks = reader.ReadByte();
			this.BlocksToRemove = new IntVector3[(int)this.NumBlocks];
			for (int i = 0; i < (int)this.NumBlocks; i++)
			{
				this.BlocksToRemove[i] = IntVector3.Read(reader);
			}
		}

		public Vector3 Location;

		public int Index;

		public byte NumBlocks;

		public DragonTypeEnum EType;

		public IntVector3[] BlocksToRemove;
	}
}
