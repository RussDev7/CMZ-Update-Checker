using System;
using DNA.CastleMinerZ.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeLauncherGuidedItem : GrenadeLauncherBaseItem
	{
		public GrenadeLauncherGuidedItem(GrenadeLauncherBaseInventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override bool InflictDamage()
		{
			ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, InventoryItemIDs.RocketLauncherGuidedShotFired);
			return false;
		}
	}
}
