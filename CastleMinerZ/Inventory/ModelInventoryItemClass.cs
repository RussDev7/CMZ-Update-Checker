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
			CastleMinerToolModel ent = new CastleMinerToolModel(this._model, use, attachedToLocalPlayer);
			ent.EnablePerPixelLighting();
			ent.ToolColor = this.ToolColor;
			ent.ToolColor2 = this.ToolColor2;
			ent.EmissiveColor = this.EmissiveColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Matrix i;
				if (this.ID == InventoryItemIDs.GunPowder || this.ID == InventoryItemIDs.ExplosivePowder)
				{
					Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 0.7853982f, 0f);
					float scale = 28.8f / ent.GetLocalBoundingSphere().Radius;
					i = Matrix.Transform(Matrix.CreateScale(scale), mat);
				}
				else
				{
					Quaternion mat2 = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
					float scale2 = 28.8f / ent.GetLocalBoundingSphere().Radius;
					if (this.ID >= InventoryItemIDs.Iron && this.ID <= InventoryItemIDs.Gold)
					{
						scale2 *= 1.5f;
					}
					i = Matrix.Transform(Matrix.CreateScale(scale2), mat2);
					if ((this.ID >= InventoryItemIDs.BrassCasing && this.ID <= InventoryItemIDs.BloodStoneBullets) || this.ID == InventoryItemIDs.LaserBullets || this.ID == InventoryItemIDs.DiamondCasing)
					{
						Vector3 t = ent.GetLocalBoundingSphere().Center * scale2;
						t.X -= 7f;
						i.Translation = t;
					}
					else if (this.ID == InventoryItemIDs.Diamond)
					{
						Vector3 t2 = i.Translation;
						t2.X -= 3f;
						t2.Y -= 10f;
						i.Translation = t2;
					}
					else if (this.ID >= InventoryItemIDs.Iron && this.ID <= InventoryItemIDs.Gold)
					{
						Vector3 t3 = i.Translation;
						t3.X -= 5f;
						t3.Y -= 5f;
						i.Translation = t3;
					}
					else if (this.ID >= InventoryItemIDs.Coal && this.ID < InventoryItemIDs.Diamond)
					{
						i *= Matrix.CreateScale(0.9f);
						Vector3 t4 = i.Translation;
						t4.X += 7f;
						t4.Y += 4f;
						i.Translation = t4;
					}
					else if (this.ID == InventoryItemIDs.RocketAmmo)
					{
						i *= Matrix.CreateScale(0.5f);
					}
				}
				ent.LocalToParent = i;
				ent.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				ent.LocalRotation = new Quaternion(0.4816553f, 0.05900274f, 0.8705468f, -0.08170173f);
				ent.LocalPosition = new Vector3(0f, 0.1119255f, 0f);
				if (this.ID >= InventoryItemIDs.Coal && this.ID <= InventoryItemIDs.GoldOre)
				{
					ent.LocalScale = new Vector3(0.35f, 0.35f, 0.35f);
					ent.LocalPosition = new Vector3(0f, 0.0719255f, 0f);
				}
				else
				{
					ent.LocalScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				ent.EnablePerPixelLighting();
				break;
			case ItemUse.Pickup:
				if (this.ID >= InventoryItemIDs.BrassCasing && this.ID <= InventoryItemIDs.BloodStoneBullets)
				{
					Matrix j = Matrix.CreateScale(2.5f);
					ent.LocalToParent = j;
				}
				ent.EnablePerPixelLighting();
				break;
			}
			return ent;
		}

		protected Model _model;

		public Color ToolColor = Color.Gray;

		public Color ToolColor2 = Color.Gray;

		public Color EmissiveColor = Color.Black;
	}
}
