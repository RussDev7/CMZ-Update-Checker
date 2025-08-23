using System;
using System.IO;
using DNA.CastleMinerZ.Terrain;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class DestroyCustomBlockMessage : CastleMinerZMessage
	{
		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		private DestroyCustomBlockMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 location, BlockTypeEnum blockType)
		{
			DestroyCustomBlockMessage Instance = Message.GetSendInstance<DestroyCustomBlockMessage>();
			Instance.Location = location;
			Instance.BlockType = blockType;
			Instance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Location.Write(writer);
			writer.Write((byte)this.BlockType);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = IntVector3.Read(reader);
			this.BlockType = (BlockTypeEnum)reader.ReadByte();
		}

		public IntVector3 Location;

		public BlockTypeEnum BlockType;
	}
}
