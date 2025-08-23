using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class BoltRifleInventoryItemClass : GunInventoryItemClass
	{
		public BoltRifleInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Rifle\\Model"), name, description, TimeSpan.FromSeconds(0.0031746031746031746), material, damage, durabilitydamage, ammotype, "GunShot1", "AssaultReload")
		{
			this._playerMode = PlayerMode.BoltRifle;
			this.ReloadTime = TimeSpan.FromSeconds(2.4);
			this.Automatic = false;
			this.RoundsPerReload = (this.ClipCapacity = 8);
			this.ShoulderMagnification = 1.3f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(3.375);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(5.375);
			this.MinInnaccuracy = Angle.FromDegrees(3.375);
			this.MaxInnaccuracy = Angle.FromDegrees(5.375);
			this.Recoil = Angle.FromDegrees(10f);
			this.Velocity = 150f;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity result = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (use == ItemUse.UI)
			{
				Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.3455752f) * Quaternion.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				Matrix i = Matrix.Transform(Matrix.CreateScale(35.2f / result.GetLocalBoundingSphere().Radius), mat);
				i.Translation = new Vector3(12f, -16f, -16f);
				result.LocalToParent = i;
			}
			else if (use == ItemUse.Pickup)
			{
				Matrix j = Matrix.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				result.LocalToParent = j;
			}
			return result;
		}
	}
}
