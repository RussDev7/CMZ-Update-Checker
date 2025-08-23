using System;
using System.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class PlayerExistsMessage : CastleMinerZMessage
	{
		private PlayerExistsMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, AvatarDescription description, bool requestResponse)
		{
			PlayerExistsMessage Instance = Message.GetSendInstance<PlayerExistsMessage>();
			Instance.AvatarDescriptionData = description.Description;
			Instance.RequestResponse = requestResponse;
			Instance.Gamer.Gamertag = from.Gamertag;
			Instance.Gamer.PlayerID = from.PlayerID;
			Instance.DoSend(from);
		}

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

		protected override void RecieveData(BinaryReader reader)
		{
			this.RequestResponse = reader.ReadBoolean();
			int length = reader.ReadInt32();
			this.AvatarDescriptionData = reader.ReadBytes(length);
			this.Gamer.Gamertag = reader.ReadString();
			this.Gamer.PlayerID.Read(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.RequestResponse);
			writer.Write(this.AvatarDescriptionData.Length);
			writer.Write(this.AvatarDescriptionData);
			writer.Write(this.Gamer.Gamertag);
			this.Gamer.PlayerID.Write(writer);
		}

		public byte[] AvatarDescriptionData;

		public bool RequestResponse;

		public Gamer Gamer = new SimpleGamer();
	}
}
