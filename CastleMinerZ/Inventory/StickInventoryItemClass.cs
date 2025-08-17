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
			CastleMinerToolModel castleMinerToolModel = new CastleMinerToolModel(this._model, use, attachedToLocalPlayer);
			castleMinerToolModel.EnablePerPixelLighting();
			castleMinerToolModel.ToolColor = Color.Transparent;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(32f / castleMinerToolModel.GetLocalBoundingSphere().Radius), quaternion);
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
	}
}
