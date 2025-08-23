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
			ProvideDeltaListMessage Instance = Message.GetSendInstance<ProvideDeltaListMessage>();
			Instance.Delta = delta;
			if (recipient == null)
			{
				Instance.DoSend(from);
				return;
			}
			Instance.DoSend(from, recipient);
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
			int dwordCount = reader.ReadInt32();
			if (dwordCount > 0)
			{
				this.Delta = new int[dwordCount];
				for (int i = 0; i < dwordCount; i++)
				{
					this.Delta[i] = reader.ReadInt32();
				}
				return;
			}
			this.Delta = null;
		}

		protected override void SendData(BinaryWriter writer)
		{
			int dwordCount = ((this.Delta == null) ? 0 : this.Delta.Length);
			writer.Write(dwordCount);
			if (dwordCount != 0)
			{
				for (int i = 0; i < dwordCount; i++)
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
