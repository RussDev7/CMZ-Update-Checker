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
			ShotgunShotMessage Instance = Message.GetSendInstance<ShotgunShotMessage>();
			for (int i = 0; i < 5; i++)
			{
				Vector3 shot = m.Forward;
				if (addDropCompensation)
				{
					shot += m.Up * 0.015f;
				}
				Matrix mat = Matrix.CreateRotationX(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
				mat *= Matrix.CreateRotationY(MathTools.RandomFloat(-innacuracy.Radians, innacuracy.Radians));
				shot = Vector3.TransformNormal(shot, mat);
				Instance.Directions[i] = Vector3.Normalize(shot);
			}
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
