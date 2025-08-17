using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	internal class ProvideDeltaListMessage : CastleMinerZMessage
	{
		private ProvideDeltaListMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, NetworkGamer recipient, int[] delta)
		{
			ProvideDeltaListMessage sendInstance = Message.GetSendInstance<ProvideDeltaListMessage>();
			sendInstance.Delta = delta;
			if (recipient == null)
			{
				sendInstance.DoSend(from);
				return;
			}
			sendInstance.DoSend(from, recipient);
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
			int num = reader.ReadInt32();
			if (num > 0)
			{
				this.Delta = new int[num];
				for (int i = 0; i < num; i++)
				{
					this.Delta[i] = reader.ReadInt32();
				}
				return;
			}
			this.Delta = null;
		}

		protected override void SendData(BinaryWriter writer)
		{
			int num = ((this.Delta == null) ? 0 : this.Delta.Length);
			writer.Write(num);
			if (num != 0)
			{
				for (int i = 0; i < num; i++)
				{
					writer.Write(this.Delta[i]);
				}
			}
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.System;
			}
		}

		public int[] Delta;
	}
}
