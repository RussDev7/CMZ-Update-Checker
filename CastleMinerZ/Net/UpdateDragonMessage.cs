using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public class UpdateDragonMessage : CastleMinerZMessage
	{
		private UpdateDragonMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, BaseDragonWaypoint wpt)
		{
			UpdateDragonMessage Instance = Message.GetSendInstance<UpdateDragonMessage>();
			Instance.Waypoint = wpt;
			Instance.DoSend(from);
			UpdateDragonMessage.UpdateCount++;
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
				if (UpdateDragonMessage.UpdateCount < 2)
				{
					return SendDataOptions.ReliableInOrder;
				}
				return SendDataOptions.None;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Waypoint = BaseDragonWaypoint.ReadBaseWaypoint(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			this.Waypoint.Write(writer);
		}

		public static int UpdateCount;

		public BaseDragonWaypoint Waypoint;
	}
}
