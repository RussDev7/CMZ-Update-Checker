using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class GunshotMessage : CastleMinerZMessage
	{
		private GunshotMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix m, Angle innacuracy, InventoryItemIDs item, bool addDropCompensation)
		{
			GunshotMessage sendInstance = Message.GetSendInstance<GunshotMessage>();
			Vector3 vector = m.Forward;
			if (addDropCompensation)
			{
				vector += m.Up * 0.015f;
			}
			Matrix matrix = Matrix.CreateRotationX(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
			matrix *= Matrix.CreateRotationY(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
			vector = Vector3.TransformNormal(vector, matrix);
			sendInstance.Direction = Vector3.Normalize(vector);
			sendInstance.ItemID = item;
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
			this.ItemID = (InventoryItemIDs)reader.ReadInt16();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Direction);
			writer.Write((short)this.ItemID);
		}

		public Vector3 Direction;

		public InventoryItemIDs ItemID;
	}
}
