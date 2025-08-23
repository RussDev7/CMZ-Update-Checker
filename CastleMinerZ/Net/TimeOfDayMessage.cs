using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class TimeOfDayMessage : CastleMinerZMessage
	{
		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		private TimeOfDayMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, float timeOfDay)
		{
			TimeOfDayMessage Instance = Message.GetSendInstance<TimeOfDayMessage>();
			Instance.TimeOfDay = timeOfDay;
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
			this.TimeOfDay = reader.ReadSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.TimeOfDay);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public float TimeOfDay;
	}
}
