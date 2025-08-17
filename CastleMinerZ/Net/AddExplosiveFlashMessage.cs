using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class AddExplosiveFlashMessage : CastleMinerZMessage
	{
		private AddExplosiveFlashMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 position)
		{
			AddExplosiveFlashMessage sendInstance = Message.GetSendInstance<AddExplosiveFlashMessage>();
			sendInstance.Position = position;
			sendInstance.DoSend(from);
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Position = IntVector3.Read(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Position.Write(writer);
		}

		public IntVector3 Position;
	}
}
