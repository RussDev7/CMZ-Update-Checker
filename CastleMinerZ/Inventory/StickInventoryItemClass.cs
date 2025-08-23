using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class StickInventoryItemClass : ModelInventoryItemClass
	{
		public StickInventoryItemClass(InventoryItemIDs id, Color color, Model model, string name, string description, float meleeDamage)
			: base(id, model, name, description, 64, TimeSpan.FromSeconds(0.30000001192092896), color)
		{
			this._playerMode = PlayerMode.Generic;
			this.EnemyDamage = meleeDamage;
			this.EnemyDamageType = DamageType.BLUNT;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel ent = new CastleMinerToolModel(this._model, use, attachedToLocalPlayer);
			ent.EnablePerPixelLighting();
			ent.ToolColor = Color.Transparent;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
				Matrix i = Matrix.Transform(Matrix.CreateScale(32f / ent.GetLocalBoundingSphere().Radius), mat);
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
	}
}
