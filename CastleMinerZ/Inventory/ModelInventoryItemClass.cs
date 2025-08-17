using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class ModelInventoryItemClass : InventoryItem.InventoryItemClass
	{
		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, int maxStack, TimeSpan ts, Color recolor1)
			: base(id, name, description, maxStack, ts)
		{
			this.ToolColor = recolor1;
			this.ToolColor2 = Color.Gray;
			this._model = model;
		}

		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, int maxStack, TimeSpan ts, Color recolor1, Color recolor2)
			: base(id, name, description, maxStack, ts)
		{
			this.ToolColor = recolor1;
			this.ToolColor2 = recolor2;
			this._model = model;
		}

		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, int maxStack, TimeSpan ts, Color recolor1, string useSound)
			: base(id, name, description, maxStack, ts, useSound)
		{
			this.ToolColor = recolor1;
			this.ToolColor2 = Color.Gray;
			this._model = model;
		}

		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, int maxStack, TimeSpan ts, Color recolor1, Color recolor2, string useSound)
			: base(id, name, description, maxStack, ts, useSound)
		{
			this.ToolColor = recolor1;
			this.ToolColor2 = recolor2;
			this._model = model;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel castleMinerToolModel = new CastleMinerToolModel(this._model, use, attachedToLocalPlayer);
			castleMinerToolModel.EnablePerPixelLighting();
			castleMinerToolModel.ToolColor = this.ToolColor;
			castleMinerToolModel.ToolColor2 = this.ToolColor2;
			castleMinerToolModel.EmissiveColor = this.EmissiveColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Matrix matrix;
				if (this.ID == InventoryItemIDs.GunPowder || this.ID == InventoryItemIDs.ExplosivePowder)
				{
					Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0.7853982f, 0f);
					float num = 28.8f / castleMinerToolModel.GetLocalBoundingSphere().Radius;
					matrix = Matrix.Transform(Matrix.CreateScale(num), quaternion);
				}
				else
				{
					Quaternion quaternion2 = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
					float num2 = 28.8f / castleMinerToolModel.GetLocalBoundingSphere().Radius;
					if (this.ID >= InventoryItemIDs.Iron && this.ID <= InventoryItemIDs.Gold)
					{
						num2 *= 1.5f;
					}
					matrix = Matrix.Transform(Matrix.CreateScale(num2), quaternion2);
					if ((this.ID >= InventoryItemIDs.BrassCasing && this.ID <= InventoryItemIDs.BloodStoneBullets) || this.ID == InventoryItemIDs.LaserBullets || this.ID == InventoryItemIDs.DiamondCasing)
					{
						Vector3 vector = castleMinerToolModel.GetLocalBoundingSphere().Center * num2;
						vector.X -= 7f;
						matrix.Translation = vector;
					}
					else if (this.ID == InventoryItemIDs.Diamond)
					{
						Vector3 translation = matrix.Translation;
						translation.X -= 3f;
						translation.Y -= 10f;
						matrix.Translation = translation;
					}
					else if (this.ID >= InventoryItemIDs.Iron && this.ID <= InventoryItemIDs.Gold)
					{
						Vector3 translation2 = matrix.Translation;
						translation2.X -= 5f;
						translation2.Y -= 5f;
						matrix.Translation = translation2;
					}
					else if (this.ID >= InventoryItemIDs.Coal && this.ID < InventoryItemIDs.Diamond)
					{
						matrix *= Matrix.CreateScale(0.9f);
						Vector3 translation3 = matrix.Translation;
						translation3.X += 7f;
						translation3.Y += 4f;
						matrix.Translation = translation3;
					}
					else if (this.ID == InventoryItemIDs.RocketAmmo)
					{
						matrix *= Matrix.CreateScale(0.5f);
					}
				}
				castleMinerToolModel.LocalToParent = matrix;
				castleMinerToolModel.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				castleMinerToolModel.LocalRotation = new Quaternion(0.4816553f, 0.05900274f, 0.8705468f, -0.08170173f);
				castleMinerToolModel.LocalPosition = new Vector3(0f, 0.1119255f, 0f);
				if (this.ID >= InventoryItemIDs.Coal && this.ID <= InventoryItemIDs.GoldOre)
				{
					castleMinerToolModel.LocalScale = new Vector3(0.35f, 0.35f, 0.35f);
					castleMinerToolModel.LocalPosition = new Vector3(0f, 0.0719255f, 0f);
				}
				else
				{
					castleMinerToolModel.LocalScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			case ItemUse.Pickup:
				if (this.ID >= InventoryItemIDs.BrassCasing && this.ID <= InventoryItemIDs.BloodStoneBullets)
				{
					Matrix matrix2 = Matrix.CreateScale(2.5f);
					castleMinerToolModel.LocalToParent = matrix2;
				}
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			}
			return castleMinerToolModel;
		}

		protected Model _model;

		public Color ToolColor = Color.Gray;

		public Color ToolColor2 = Color.Gray;

		public Color EmissiveColor = Color.Black;
	}
}
