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
			GrenadeMessage Instance = Message.GetSendInstance<GrenadeMessage>();
			Instance.Direction = orientation.Forward;
			Instance.Position = orientation.Translation + Instance.Direction;
			Instance.GrenadeType = grenadeType;
			Instance.SecondsLeft = secondsLeft;
			Instance.DoSend(from);
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
