using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserSMGClass : LaserGunInventoryItemClass
	{
		public LaserSMGClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\SMG\\Model"), name, description, TimeSpan.FromSeconds(0.05999999865889549), material, bulletdamage, durabilitydamage, ammotype, "LaserGun2", "Reload")
		{
			this._playerMode = PlayerMode.SpaceSMG;
			this.ReloadTime = TimeSpan.FromSeconds(2.05);
			this.Automatic = true;
			this.RoundsPerReload = (this.ClipCapacity = 20);
			this.ShoulderMagnification = 1.3f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.375);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(0.91);
			this.MinInnaccuracy = Angle.FromDegrees(0.875);
			this.MaxInnaccuracy = Angle.FromDegrees(2.125);
			this.Recoil = Angle.FromDegrees(3f);
		}
	}
}
