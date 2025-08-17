using System;
using DNA.CastleMinerZ.AI;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserShotgunClass : LaserGunInventoryItemClass
	{
		public LaserShotgunClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Space\\Shotgun\\Model"), name, description, TimeSpan.FromSeconds(0.1899999976158142), material, bulletdamage, durabilitydamage, ammotype, "LaserGun5", "ShotGunReload")
		{
			this._playerMode = PlayerMode.SpacePumpShotgun;
			this.ReloadTime = TimeSpan.FromSeconds(0.5699999928474426);
			this.Automatic = false;
			this.RoundsPerReload = 1;
			this.ClipCapacity = 8;
			this.EnemyDamageType = DamageType.SHOTGUN;
			this.ShoulderMagnification = 1.3f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(3.375);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(5.375);
			this.MinInnaccuracy = Angle.FromDegrees(3.375);
			this.MaxInnaccuracy = Angle.FromDegrees(5.375);
			this.Recoil = Angle.FromDegrees(10f);
		}
	}
}
