using System;
using DNA.CastleMinerZ.Net;

namespace DNA.CastleMinerZ.Utils
{
	public class DebugUtils
	{
		public static void Log(string message)
		{
			if (DebugUtils.broadLogMessages)
			{
				BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, "[" + CastleMinerZGame.Instance.LocalPlayer.Gamer.Gamertag + "] " + message);
			}
		}

		private static bool broadLogMessages;
	}
}
