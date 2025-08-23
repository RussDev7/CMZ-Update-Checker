using System;
using System.IO;
using DNA.CastleMinerZ.Terrain;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class AlterBlockMessage : CastleMinerZMessage
	{
		private AlterBlockMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 blockLocaion, BlockTypeEnum blockType)
		{
			AlterBlockMessage Instance = Message.GetSendInstance<AlterBlockMessage>();
			Instance.BlockLocation = blockLocaion;
			Instance.BlockType = blockType;
			Instance.DoSend(from);
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
			this.BlockLocation = IntVector3.Read(reader);
			this.BlockType = (BlockTypeEnum)reader.ReadInt32();
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.BlockLocation.Write(writer);
			writer.Write((int)this.BlockType);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public IntVector3 BlockLocation;

		public BlockTypeEnum BlockType;
	}
}
