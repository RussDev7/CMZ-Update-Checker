using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class ClockInventoryItemClass : ModelInventoryItemClass
	{
		public ClockInventoryItemClass(InventoryItemIDs id, Model model)
			: base(id, model, Strings.Clock, Strings.Show_the_time_of_day, 1, TimeSpan.FromSeconds(0.30000001192092896), Color.Gray)
		{
			this._playerMode = PlayerMode.Generic;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ClockEntity result = new ClockEntity(this._model, use, attachedToLocalPlayer);
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
	}
}
