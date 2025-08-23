using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class GPSItemClass : ModelInventoryItemClass
	{
		public GPSItemClass(InventoryItemIDs id, Model model, string name, string description)
			: base(id, model, name, description, 1, TimeSpan.FromSeconds(0.30000001192092896), Color.Gray)
		{
			this._playerMode = PlayerMode.Generic;
			switch (id)
			{
			case InventoryItemIDs.GPS:
				this.ItemSelfDamagePerUse = 0.1f;
				return;
			case InventoryItemIDs.TeleportGPS:
				break;
			default:
				if (id != InventoryItemIDs.SpawnBasic)
				{
					return;
				}
				break;
			}
			this.ItemSelfDamagePerUse = 1f;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			GPSEntity result = new GPSEntity(this._model, use, attachedToLocalPlayer);
			if (use != ItemUse.UI)
			{
				result.TrackPosition = false;
			}
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 1.5707964f, 0f);
				Matrix i = Matrix.Transform(Matrix.CreateScale(22.4f / result.GetLocalBoundingSphere().Radius), mat);
				i.Translation = new Vector3(0f, 0f, 0f);
				result.LocalToParent = i;
				result.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				result.TrackPosition = true;
				result.LocalRotation = new Quaternion(0.6469873f, 0.1643085f, 0.7078394f, -0.2310277f);
				result.LocalPosition = new Vector3(0f, 0.09360941f, 0f);
				break;
			}
			return result;
		}

		public override bool IsMeleeWeapon
		{
			get
			{
				return false;
			}
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new GPSItem(this, stackCount);
		}
	}
}
