using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class DetonateExplosiveMessage : CastleMinerZMessage
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
				return SendDataOptions.ReliableInOrder;
			}
		}

		private DetonateExplosiveMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 location, bool originalExplosion, ExplosiveTypes explosiveType)
		{
			DetonateExplosiveMessage sendInstance = Message.GetSendInstance<DetonateExplosiveMessage>();
			sendInstance.Location = location;
			sendInstance.OriginalExplosion = originalExplosion;
			sendInstance.ExplosiveType = explosiveType;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Location);
			writer.Write(this.OriginalExplosion);
			writer.Write((byte)this.ExplosiveType);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Location = reader.ReadIntVector3();
			this.OriginalExplosion = reader.ReadBoolean();
			this.ExplosiveType = (ExplosiveTypes)reader.ReadByte();
		}

		public IntVector3 Location;

		public bool OriginalExplosion;

		public ExplosiveTypes ExplosiveType;
	}
}
