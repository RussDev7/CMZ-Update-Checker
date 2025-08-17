using System;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeLauncherBaseItem : GunInventoryItem
	{
		public GrenadeLauncherBaseItem(GrenadeLauncherBaseInventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override bool InflictDamage()
		{
			ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, InventoryItemIDs.RocketLauncherShotFired);
			return false;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (this._deleteMe && !hud.LocalPlayer.UsingAnimationPlaying)
			{
				hud.PlayerInventory.Remove(this);
				return;
			}
			base.ProcessInput(hud, controller);
		}

		protected bool _deleteMe;
	}
}
