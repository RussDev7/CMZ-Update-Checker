using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserDrillInventoryItemClass : LaserGunInventoryItemClass
	{
		public LaserDrillInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Tools\\Drill\\Model"), name, description, TimeSpan.FromSeconds(0.05000000074505806), material, bulletdamage, durabilitydamage, ammotype, "LaserGun3", "AssaultReload")
		{
			this._playerMode = PlayerMode.SpaceAssault;
			this.ReloadTime = TimeSpan.FromSeconds(3.0);
			this.Automatic = true;
			this.RoundsPerReload = (this.ClipCapacity = 200);
			this.ShoulderMagnification = 1.15f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.625f);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(1.4);
			this.MinInnaccuracy = Angle.FromDegrees(2f);
			this.MaxInnaccuracy = Angle.FromDegrees(4f);
			this.Recoil = Angle.FromDegrees(2f);
		}

		internal override bool IsHarvestWeapon()
		{
			return true;
		}
	}
}
