using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserPistolClass : LaserGunInventoryItemClass
	{
		public LaserPistolClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\Pistol\\Model"), name, description, TimeSpan.FromSeconds(0.05999999865889549), material, bulletdamage, durabilitydamage, ammotype, "LaserGun4", "Reload")
		{
			this._playerMode = PlayerMode.SpacePistol;
			this.ReloadTime = TimeSpan.FromSeconds(1.25);
			this.Automatic = false;
			this.RoundsPerReload = (this.ClipCapacity = 7);
			this.ShoulderMagnification = 1f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.625);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(1.25);
			this.MinInnaccuracy = Angle.FromDegrees(1.25);
			this.MaxInnaccuracy = Angle.FromDegrees(2.5);
			this.Recoil = Angle.FromDegrees(3f);
		}
	}
}
