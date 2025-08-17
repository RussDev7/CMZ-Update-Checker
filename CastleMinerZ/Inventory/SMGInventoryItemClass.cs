using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class SMGInventoryItemClass : GunInventoryItemClass
	{
		public SMGInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\SMG\\Model"), name, description, TimeSpan.FromMinutes(0.0010672358591248667), material, damage, durabilitydamage, ammotype, "GunShot2", "Reload")
		{
			this._playerMode = PlayerMode.SMG;
			this.ReloadTime = TimeSpan.FromSeconds(2.0);
			this.Automatic = true;
			this.RoundsPerReload = (this.ClipCapacity = 20);
			this.FlightTime = 1f;
			this.ShoulderMagnification = 1.3f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.375);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(0.91);
			this.MinInnaccuracy = Angle.FromDegrees(0.875);
			this.MaxInnaccuracy = Angle.FromDegrees(2.125);
			this.Recoil = Angle.FromDegrees(3f);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (use == ItemUse.UI)
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f) * Quaternion.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(32f / modelEntity.GetLocalBoundingSphere().Radius), quaternion);
				matrix.Translation = new Vector3(12f, -19f, -16f);
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
