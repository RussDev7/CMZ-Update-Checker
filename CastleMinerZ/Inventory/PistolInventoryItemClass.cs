using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class PistolInventoryItemClass : GunInventoryItemClass
	{
		public PistolInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Pistol\\Model"), name, description, TimeSpan.FromSeconds(0.10000000149011612), material, damage, durabilitydamage, ammotype, "GunShot4", "Reload")
		{
			this._playerMode = PlayerMode.Pistol;
			this.ReloadTime = TimeSpan.FromSeconds(1.6299999952316284);
			this.Automatic = false;
			this.RoundsPerReload = (this.ClipCapacity = 8);
			this.ShoulderMagnification = 1f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.625);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(1.25);
			this.MinInnaccuracy = Angle.FromDegrees(1.25);
			this.MaxInnaccuracy = Angle.FromDegrees(2.5);
			this.Recoil = Angle.FromDegrees(3f);
			this.FlightTime = 1f;
			this.Velocity = 75f;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (use == ItemUse.UI)
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f) * Quaternion.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(25.6f / modelEntity.GetLocalBoundingSphere().Radius), quaternion);
				matrix.Translation = new Vector3(4f, -12f, -16f);
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
