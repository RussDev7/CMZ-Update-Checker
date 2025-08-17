using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserARInventoryItemClass : LaserGunInventoryItemClass
	{
		public LaserARInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\AR\\Model"), name, description, TimeSpan.FromSeconds(0.05999999865889549), material, bulletdamage, durabilitydamage, ammotype, "LaserGun3", "AssaultReload")
		{
			this._playerMode = PlayerMode.SpaceAssault;
			this.ReloadTime = TimeSpan.FromSeconds(2.5);
			this.Automatic = true;
			this.RoundsPerReload = (this.ClipCapacity = 30);
			this.ShoulderMagnification = 1.35f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.625f);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(1.4);
			this.MinInnaccuracy = Angle.FromDegrees(2f);
			this.MaxInnaccuracy = Angle.FromDegrees(4.625);
			this.Recoil = Angle.FromDegrees(3f);
		}
	}
}
