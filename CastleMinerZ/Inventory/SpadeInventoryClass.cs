using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class SpadeInventoryClass : ModelInventoryItemClass
	{
		public SpadeInventoryClass(InventoryItemIDs id, ToolMaterialTypes material, Model model, string name, string description, float meleeDamage)
			: base(id, model, name, description, 1, TimeSpan.FromSeconds(0.30000001192092896), Color.Gray)
		{
			this._playerMode = PlayerMode.Pick;
			this.Material = material;
			this.ToolColor = CMZColors.GetMaterialcColor(this.Material);
			this.EnemyDamage = meleeDamage;
			this.EnemyDamageType = DamageType.BLUNT;
			switch (this.Material)
			{
			case ToolMaterialTypes.Wood:
				this.ItemSelfDamagePerUse = 0.005f;
				return;
			case ToolMaterialTypes.Stone:
				this.ItemSelfDamagePerUse = 0.0025f;
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

		public override InventoryItem CreateItem(int stackCount)
		{
			return new SpadeInventoryItem(this, stackCount);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel castleMinerToolModel = new CastleMinerToolModel(this._model, use, attachedToLocalPlayer);
			castleMinerToolModel.EnablePerPixelLighting();
			castleMinerToolModel.ToolColor = this.ToolColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(32f / castleMinerToolModel.GetLocalBoundingSphere().Radius), quaternion);
				Vector3 vector = new Vector3(-10f, -10f, 0f);
				matrix.Translation = vector;
				castleMinerToolModel.LocalToParent = matrix;
				castleMinerToolModel.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				castleMinerToolModel.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 1.5707964f) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.7853982f);
				castleMinerToolModel.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
				break;
			}
			return castleMinerToolModel;
		}

		public ToolMaterialTypes Material;
	}
}
