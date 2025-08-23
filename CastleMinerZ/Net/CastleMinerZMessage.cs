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
			bool result = false;
			if (CastleMinerZGame.Instance != null)
			{
				NetworkGamer host = CastleMinerZGame.Instance.GetGamerFromID(CastleMinerZGame.Instance.TerrainServerID);
				if (host != null && !host.HasLeftSession)
				{
					base.DoSend(sender, host);
					result = true;
				}
			}
			return result;
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
