using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class RemoveBlocksMessage : CastleMinerZMessage
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

		private RemoveBlocksMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, int numblocks, IntVector3[] blocks, bool doEffects)
		{
			RemoveBlocksMessage Instance = Message.GetSendInstance<RemoveBlocksMessage>();
			Instance.NumBlocks = numblocks;
			Instance.BlocksToRemove = blocks;
			Instance.DoDigEffects = doEffects;
			Instance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.NumBlocks);
			for (int i = 0; i < this.NumBlocks; i++)
			{
				this.BlocksToRemove[i].Write(writer);
			}
			writer.Write(this.DoDigEffects);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.NumBlocks = reader.ReadInt32();
			this.BlocksToRemove = new IntVector3[this.NumBlocks];
			for (int i = 0; i < this.NumBlocks; i++)
			{
				this.BlocksToRemove[i] = IntVector3.Read(reader);
			}
			this.DoDigEffects = reader.ReadBoolean();
		}

		public bool DoDigEffects;

		public int NumBlocks;

		public IntVector3[] BlocksToRemove;
	}
}
