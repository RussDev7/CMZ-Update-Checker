using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class FireRocketMessage : CastleMinerZMessage
	{
		private FireRocketMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix orientation, InventoryItemIDs weaponType, bool guided)
		{
			FireRocketMessage sendInstance = Message.GetSendInstance<FireRocketMessage>();
			sendInstance.Direction = orientation.Forward;
			sendInstance.Position = orientation.Translation + sendInstance.Direction;
			sendInstance.WeaponType = weaponType;
			sendInstance.Guided = guided;
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
			this.Guided = reader.ReadBoolean();
			this.WeaponType = (InventoryItemIDs)reader.ReadInt16();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Direction);
			writer.Write(this.Position);
			writer.Write(this.Guided);
			writer.Write((short)this.WeaponType);
		}

		public Vector3 Position;

		public Vector3 Direction;

		public InventoryItemIDs WeaponType;

		public bool Guided;
	}
}
