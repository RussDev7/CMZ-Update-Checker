using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class KnifeInventoryItemClass : ModelInventoryItemClass
	{
		public KnifeInventoryItemClass(InventoryItemIDs id, Model model, ToolMaterialTypes material, string name, string description, float meleeDamage, float itemselfDamage, TimeSpan coolDown)
			: base(id, model, name, description, 1, coolDown, Color.Gray)
		{
			this._playerMode = PlayerMode.Pick;
			this.ItemSelfDamagePerUse = itemselfDamage;
			this.EnemyDamage = meleeDamage;
			this.EnemyDamageType = DamageType.BLADE;
			this.Material = material;
			this.ToolColor = CMZColors.GetMaterialcColor(this.Material);
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new KnifeInventoryItem(this, stackCount);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel ent = new CastleMinerToolModel(this._model, use, attachedToLocalPlayer);
			ent.EnablePerPixelLighting();
			ent.ToolColor = this.ToolColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0.31415927f) * Quaternion.CreateFromYawPitchRoll(0f, -1.5707964f, 0f);
				Matrix i = Matrix.Transform(Matrix.CreateScale(35.2f / ent.GetLocalBoundingSphere().Radius), mat);
				i.Translation = new Vector3(16f, -19f, -16f);
				ent.LocalToParent = i;
				ent.EnableDefaultLighting();
				break;
			}
			}
			return ent;
		}

		public ToolMaterialTypes Material;
	}
}
