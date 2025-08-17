using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserGunInventoryItemClass : GunInventoryItemClass
	{
		public LaserGunInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, TimeSpan fireRate, ToolMaterialTypes material, float bulletDamage, float itemSelfDamage, InventoryItem.InventoryItemClass ammoClass, string shootSound, string reloadSound)
			: base(id, model, name, description, fireRate, material, bulletDamage, itemSelfDamage, ammoClass, shootSound, reloadSound)
		{
			this.TracerColor = CMZColors.GetLaserMaterialcColor(this.Material).ToVector4();
			this.EmissiveColor = CMZColors.GetLaserMaterialcColor(this.Material);
		}

		public override bool NeedsDropCompensation
		{
			get
			{
				return false;
			}
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (use == ItemUse.UI)
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f) * Quaternion.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(32f / modelEntity.GetLocalBoundingSphere().Radius), quaternion);
				switch (this.ID)
				{
				case InventoryItemIDs.IronSpacePistol:
				case InventoryItemIDs.IronSpaceSMGGun:
				case InventoryItemIDs.CopperSpacePistol:
				case InventoryItemIDs.CopperSpaceSMGGun:
				case InventoryItemIDs.GoldSpacePistol:
				case InventoryItemIDs.GoldSpaceSMGGun:
				case InventoryItemIDs.DiamondSpacePistol:
				case InventoryItemIDs.DiamondSpaceSMGGun:
					matrix.Translation = new Vector3(13f, -21f, -16f);
					goto IL_0114;
				case InventoryItemIDs.CopperSpaceAssultRifle:
				case InventoryItemIDs.GoldSpaceAssultRifle:
				case InventoryItemIDs.DiamondSpaceAssultRifle:
					matrix.Translation = new Vector3(9f, -17f, -16f);
					goto IL_0114;
				}
				matrix.Translation = new Vector3(9f, -17f, -16f);
				IL_0114:
				modelEntity.LocalToParent = matrix;
			}
			else if (use == ItemUse.Pickup)
			{
				Matrix matrix2 = Matrix.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				modelEntity.LocalToParent = matrix2;
			}
			return modelEntity;
		}

		internal virtual bool IsHarvestWeapon()
		{
			return false;
		}
	}
}
