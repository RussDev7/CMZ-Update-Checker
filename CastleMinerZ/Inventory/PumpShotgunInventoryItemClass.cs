using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class PumpShotgunInventoryItemClass : GunInventoryItemClass
	{
		public PumpShotgunInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Shotgun\\Model"), name, description, TimeSpan.FromMinutes(0.016666666666666666), material, damage, durabilitydamage, ammotype, "Shotgun", "ShotGunReload")
		{
			this._playerMode = PlayerMode.PumpShotgun;
			this.ReloadTime = TimeSpan.FromSeconds(0.567);
			this.Automatic = false;
			this.ClipCapacity = 6;
			this.RoundsPerReload = 1;
			this.FlightTime = 0.4f;
			this.ShoulderMagnification = 1.3f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(3.375);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(5.375);
			this.MinInnaccuracy = Angle.FromDegrees(3.375);
			this.MaxInnaccuracy = Angle.FromDegrees(5.375);
			this.Recoil = Angle.FromDegrees(10f);
			this.Velocity = 50f;
			this.EnemyDamageType = DamageType.SHOTGUN;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (use == ItemUse.UI)
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.9424779f) * Quaternion.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(28.8f / modelEntity.GetLocalBoundingSphere().Radius), quaternion);
				matrix.Translation = new Vector3(8f, -14f, -16f);
				modelEntity.LocalToParent = matrix;
			}
			else if (use == ItemUse.Pickup)
			{
				Matrix matrix2 = Matrix.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				modelEntity.LocalToParent = matrix2;
			}
			return modelEntity;
		}
	}
}
