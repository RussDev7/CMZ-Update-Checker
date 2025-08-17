using System;
using DNA.CastleMinerZ.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherGuidedItem : RocketLauncherBaseItem
	{
		public RocketLauncherGuidedItem(RocketLauncherBaseInventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override bool InflictDamage()
		{
			this._deleteMe = true;
			ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, InventoryItemIDs.RocketLauncherGuidedShotFired);
			return false;
		}
	}
}
