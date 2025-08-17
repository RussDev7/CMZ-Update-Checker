using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class TimeConnectedMessage : CastleMinerZMessage
	{
		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		private TimeConnectedMessage()
		{
		}

		public void Apply(Player player)
		{
			player.TimeConnected = this.TimeConnected;
		}

		public static void Send(LocalNetworkGamer from, Player player)
		{
			TimeConnectedMessage sendInstance = Message.GetSendInstance<TimeConnectedMessage>();
			sendInstance.TimeConnected = player.TimeConnected;
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
			this.TimeConnected = new TimeSpan(reader.ReadInt64());
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.TimeConnected.Ticks);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.PlayerUpdate;
			}
		}

		public TimeSpan TimeConnected;
	}
}
