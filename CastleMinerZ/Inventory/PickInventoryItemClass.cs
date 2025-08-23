using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class PickInventoryItemClass : ModelInventoryItemClass
	{
		public PickInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, Model model, string name, string description, float meleeDamage)
			: base(id, model, name, description, 1, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.GetMaterialcColor(material))
		{
			this._playerMode = PlayerMode.Pick;
			this.EnemyDamage = meleeDamage;
			this.EnemyDamageType = DamageType.BLADE;
			this.Material = material;
			switch (this.Material)
			{
			case ToolMaterialTypes.Wood:
				this.ItemSelfDamagePerUse = 0.0033333334f;
				return;
			case ToolMaterialTypes.Stone:
				this.ItemSelfDamagePerUse = 0.002f;
				return;
			case ToolMaterialTypes.Copper:
				this.ItemSelfDamagePerUse = 0.00125f;
				return;
			case ToolMaterialTypes.Iron:
				this.ItemSelfDamagePerUse = 0.0005f;
				return;
			case ToolMaterialTypes.Gold:
				this.ItemSelfDamagePerUse = 0.00033333333f;
				return;
			case ToolMaterialTypes.Diamond:
				this.ItemSelfDamagePerUse = 0.00025f;
				return;
			case ToolMaterialTypes.BloodStone:
				this.ItemSelfDamagePerUse = 0.0002f;
				return;
			default:
				return;
			}
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel ent = new CastleMinerToolModel(this._model, use, attachedToLocalPlayer);
			ent.EnablePerPixelLighting();
			ent.ToolColor = this.ToolColor;
			ent.EmissiveColor = this.EmissiveColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
				Matrix i = Matrix.Transform(Matrix.CreateScale(25.6f / ent.GetLocalBoundingSphere().Radius), mat);
				i.Translation += new Vector3(-4f, -4f, 0f);
				ent.LocalToParent = i;
				ent.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				ent.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 1.5707964f) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.7853982f);
				ent.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
				break;
			}
			return ent;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new PickInventoryItem(this, stackCount);
		}

		public ToolMaterialTypes Material;

		public enum ToolTierName
		{
			None,
			Wood,
			Copper,
			Iron,
			Gold,
			Diamond,
			Bloodstone,
			Laser
		}
	}
}
