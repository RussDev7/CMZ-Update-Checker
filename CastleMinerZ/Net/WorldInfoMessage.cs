using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class WorldInfoMessage : CastleMinerZMessage
	{
		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		private WorldInfoMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, WorldInfo worldInfo)
		{
			WorldInfoMessage sendInstance = Message.GetSendInstance<WorldInfoMessage>();
			sendInstance.WorldInfo = worldInfo;
			sendInstance.DoSend(from);
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
			this.WorldInfo = new WorldInfo(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.WorldInfo.Save(writer);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public WorldInfo WorldInfo;
	}
}
