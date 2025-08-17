using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserRifleClass : LaserGunInventoryItemClass
	{
		public LaserRifleClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\Rifle\\Model"), name, description, TimeSpan.FromSeconds(0.25999999046325684), material, bulletdamage, durabilitydamage, ammotype, "LaserGun1", "AssaultReload")
		{
			this._playerMode = PlayerMode.SpaceBoltRifle;
			this.ReloadTime = TimeSpan.FromSeconds(2.950000047683716);
			this.Automatic = false;
			this.RoundsPerReload = (this.ClipCapacity = 10);
			this.ShoulderMagnification = 4.3f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.25f);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(0.36);
			this.MinInnaccuracy = Angle.FromDegrees(6.875);
			this.MaxInnaccuracy = Angle.FromDegrees(10f);
			this.Recoil = Angle.FromDegrees(12f);
			this.Scoped = true;
		}
	}
}
