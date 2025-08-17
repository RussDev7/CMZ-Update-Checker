using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Net
{
	public class DragonAttackMessage : CastleMinerZMessage
	{
		private DragonAttackMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, BaseDragonWaypoint wpt, Vector3 target, int fbindex, bool animatedAttack)
		{
			DragonAttackMessage sendInstance = Message.GetSendInstance<DragonAttackMessage>();
			sendInstance.Waypoint = wpt;
			sendInstance.Target = target;
			sendInstance.FireballIndex = fbindex;
			sendInstance.AnimatedAttack = animatedAttack;
			sendInstance.DoSend(from);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.EnemyMessage;
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
			this.Waypoint = BaseDragonWaypoint.ReadBaseWaypoint(reader);
			this.Target = reader.ReadVector3();
			this.AnimatedAttack = reader.ReadBoolean();
			this.FireballIndex = reader.ReadInt32();
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Waypoint.Write(writer);
			writer.Write(this.Target);
			writer.Write(this.AnimatedAttack);
			writer.Write(this.FireballIndex);
		}

		public BaseDragonWaypoint Waypoint;

		public Vector3 Target;

		public int FireballIndex;

		public bool AnimatedAttack;
	}
}
