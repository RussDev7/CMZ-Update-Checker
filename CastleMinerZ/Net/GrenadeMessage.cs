using System;
using System.IO;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class GrenadeMessage : CastleMinerZMessage
	{
		private GrenadeMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix orientation, GrenadeTypeEnum grenadeType, float secondsLeft)
		{
			GrenadeMessage sendInstance = Message.GetSendInstance<GrenadeMessage>();
			sendInstance.Direction = orientation.Forward;
			sendInstance.Position = orientation.Translation + sendInstance.Direction;
			sendInstance.GrenadeType = grenadeType;
			sendInstance.SecondsLeft = secondsLeft;
			sendInstance.DoSend(from);
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
				return SendDataOptions.Reliable;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Direction = reader.ReadVector3();
			this.Position = reader.ReadVector3();
			this.GrenadeType = (GrenadeTypeEnum)reader.ReadByte();
			this.SecondsLeft = reader.ReadSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Direction);
			writer.Write(this.Position);
			writer.Write((byte)this.GrenadeType);
			writer.Write(this.SecondsLeft);
		}

		public Vector3 Position;

		public Vector3 Direction;

		public GrenadeTypeEnum GrenadeType;

		public float SecondsLeft;
	}
}
