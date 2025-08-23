using System;
using System.IO;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class DetonateGrenadeMessage : CastleMinerZMessage
	{
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
				return SendDataOptions.Reliable;
			}
		}

		private DetonateGrenadeMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 location, GrenadeTypeEnum grenadeType, bool onGround)
		{
			DetonateGrenadeMessage Instance = Message.GetSendInstance<DetonateGrenadeMessage>();
			Instance.Location = location;
			Instance.GrenadeType = grenadeType;
			Instance.OnGround = onGround;
			Instance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Location);
			writer.Write((byte)this.GrenadeType);
			writer.Write(this.OnGround);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = reader.ReadVector3();
			this.GrenadeType = (GrenadeTypeEnum)reader.ReadByte();
			this.OnGround = reader.ReadBoolean();
		}

		public Vector3 Location;

		public GrenadeTypeEnum GrenadeType;

		public bool OnGround;
	}
}
