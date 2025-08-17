using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class AssultRifleInventoryItemClass : GunInventoryItemClass
	{
		public AssultRifleInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\AR\\Model"), name, description, TimeSpan.FromSeconds(0.10000000149011612), material, bulletdamage, durabilitydamage, ammotype, "GunShot3", "AssaultReload")
		{
			this._playerMode = PlayerMode.Assault;
			this.ReloadTime = TimeSpan.FromSeconds(3.0);
			this.Automatic = true;
			this.RoundsPerReload = (this.ClipCapacity = 30);
			this.ShoulderMagnification = 1.35f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.625f);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(1.4);
			this.MinInnaccuracy = Angle.FromDegrees(2f);
			this.MaxInnaccuracy = Angle.FromDegrees(4.625);
			this.Recoil = Angle.FromDegrees(3f);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (use == ItemUse.UI)
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.86393803f) * Quaternion.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(32f / modelEntity.GetLocalBoundingSphere().Radius), quaternion);
				matrix.Translation = new Vector3(6f, -12f, -16f);
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
