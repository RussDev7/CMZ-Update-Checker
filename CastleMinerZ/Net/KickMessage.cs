using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class KickMessage : Message
	{
		private KickMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, NetworkGamer kickedPlayer, bool banned)
		{
			KickMessage Instance = Message.GetSendInstance<KickMessage>();
			Instance.Banned = banned;
			Instance.PlayerID = kickedPlayer.Id;
			Instance.DoSend(from, kickedPlayer);
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
			this.Banned = reader.ReadBoolean();
			this.PlayerID = reader.ReadByte();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Banned);
			writer.Write(this.PlayerID);
		}

		public bool Banned;

		public byte PlayerID;
	}
}
