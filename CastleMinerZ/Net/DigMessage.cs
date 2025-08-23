using System;
using System.IO;
using DNA.CastleMinerZ.Terrain;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class DigMessage : CastleMinerZMessage
	{
		private DigMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, bool placing, Vector3 location, Vector3 direction, BlockTypeEnum blockDug)
		{
			DigMessage Instance = Message.GetSendInstance<DigMessage>();
			Instance.Placing = placing;
			Instance.Location = location;
			Instance.Direction = direction;
			Instance.BlockType = blockDug;
			Instance.DoSend(from);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.PlayerUpdate;
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
			this.Placing = reader.ReadBoolean();
			this.Location = reader.ReadVector3();
			this.Direction = reader.ReadVector3();
			this.BlockType = (BlockTypeEnum)reader.ReadInt32();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Placing);
			writer.Write(this.Location);
			writer.Write(this.Direction);
			writer.Write((int)this.BlockType);
		}

		public bool Placing;

		public Vector3 Location;

		public Vector3 Direction;

		public BlockTypeEnum BlockType;
	}
}
