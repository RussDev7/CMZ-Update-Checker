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
			GunshotMessage Instance = Message.GetSendInstance<GunshotMessage>();
			Vector3 shot = m.Forward;
			if (addDropCompensation)
			{
				shot += m.Up * 0.015f;
			}
			Matrix mat = Matrix.CreateRotationX(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
			mat *= Matrix.CreateRotationY(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
			shot = Vector3.TransformNormal(shot, mat);
			Instance.Direction = Vector3.Normalize(shot);
			Instance.ItemID = item;
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
