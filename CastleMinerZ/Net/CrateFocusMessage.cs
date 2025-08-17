using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class CrateFocusMessage : CastleMinerZMessage
	{
		private CrateFocusMessage()
		{
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
				return SendDataOptions.ReliableInOrder;
			}
		}

		public static void Send(LocalNetworkGamer from, IntVector3 location, Point index)
		{
			CrateFocusMessage sendInstance = Message.GetSendInstance<CrateFocusMessage>();
			sendInstance.Location = location;
			sendInstance.ItemIndex = index;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Location.Write(writer);
			writer.Write(this.ItemIndex.X);
			writer.Write(this.ItemIndex.Y);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = IntVector3.Read(reader);
			this.ItemIndex.X = reader.ReadInt32();
			this.ItemIndex.Y = reader.ReadInt32();
		}

		public IntVector3 Location;

		public Point ItemIndex;
	}
}
