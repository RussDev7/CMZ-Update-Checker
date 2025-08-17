using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserPrecisionInventoryItemClass : LaserGunInventoryItemClass
	{
		public LaserPrecisionInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Tools\\Drill\\Model"), name, description, TimeSpan.FromSeconds(0.05000000074505806), material, bulletdamage, durabilitydamage, ammotype, "LaserGun3", "AssaultReload")
		{
			this._playerMode = PlayerMode.SpaceAssault;
			this.ReloadTime = TimeSpan.FromSeconds(3.0);
			this.Automatic = false;
			this.RoundsPerReload = (this.ClipCapacity = 10);
			this.ShoulderMagnification = 3.45f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.1f);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(0.2f);
			this.MinInnaccuracy = Angle.FromDegrees(0.1);
			this.MaxInnaccuracy = Angle.FromDegrees(0.2);
			this.Recoil = Angle.FromDegrees(0.1);
		}

		internal override bool IsHarvestWeapon()
		{
			return true;
		}
	}
}
