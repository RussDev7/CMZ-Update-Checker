using System;
using DNA.CastleMinerZ.UI;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeItem : InventoryItem
	{
		private Player _localPlayer
		{
			get
			{
				return CastleMinerZGame.Instance.LocalPlayer;
			}
		}

		public GrenadeItem(InventoryItem.InventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			hud.LocalPlayer.UsingTool = false;
			if (controller.Use.Pressed && !this._localPlayer.GrenadeAnimPlaying)
			{
				this._localPlayer.ReadyToThrowGrenade = false;
				this._localPlayer.grenadeCookTime = TimeSpan.Zero;
				this._localPlayer.PlayGrenadeAnim = true;
				return;
			}
			if (controller.Use.Released && this._localPlayer.PlayGrenadeAnim && !this._localPlayer.ReadyToThrowGrenade)
			{
				this._localPlayer.ReadyToThrowGrenade = true;
			}
		}
	}
}
