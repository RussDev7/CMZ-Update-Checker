using System;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class ShotgunShotMessage : CastleMinerZMessage
	{
		private ShotgunShotMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix m, Angle innacuracy, InventoryItemIDs item, bool addDropCompensation)
		{
			ShotgunShotMessage sendInstance = Message.GetSendInstance<ShotgunShotMessage>();
			for (int i = 0; i < 5; i++)
			{
				Vector3 vector = m.Forward;
				if (addDropCompensation)
				{
					vector += m.Up * 0.015f;
				}
				Matrix matrix = Matrix.CreateRotationX(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
				matrix *= Matrix.CreateRotationY(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
				vector = Vector3.TransformNormal(vector, matrix);
				sendInstance.Directions[i] = Vector3.Normalize(vector);
			}
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
			for (int i = 0; i < 5; i++)
			{
				this.Directions[i] = reader.ReadVector3();
			}
			this.ItemID = (InventoryItemIDs)reader.ReadInt16();
		}

		protected override void SendData(BinaryWriter writer)
		{
			for (int i = 0; i < 5; i++)
			{
				writer.Write(this.Directions[i]);
			}
			writer.Write((short)this.ItemID);
		}

		public Vector3[] Directions = new Vector3[5];

		public InventoryItemIDs ItemID;
	}
}
