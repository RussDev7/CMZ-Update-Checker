using System;
using DNA.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Net
{
	public abstract class CastleMinerZMessage : Message
	{
		public abstract CastleMinerZMessage.MessageTypes MessageType { get; }

		protected bool SendToHost(LocalNetworkGamer sender)
		{
			bool flag = false;
			if (CastleMinerZGame.Instance != null)
			{
				NetworkGamer gamerFromID = CastleMinerZGame.Instance.GetGamerFromID(CastleMinerZGame.Instance.TerrainServerID);
				if (gamerFromID != null && !gamerFromID.HasLeftSession)
				{
					base.DoSend(sender, gamerFromID);
					flag = true;
				}
			}
			return flag;
		}

		public enum MessageTypes
		{
			System,
			Broadcast,
			PlayerUpdate,
			EnemyMessage,
			PickupMessage
		}
	}
}
