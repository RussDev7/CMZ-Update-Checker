using System;
using DNA.CastleMinerZ.Globalization;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class CompassInventoryItemClass : ModelInventoryItemClass
	{
		public CompassInventoryItemClass(InventoryItemIDs id, Model model)
			: base(id, model, Strings.Compass, Strings.Show_the_direction_to_or_away_from_the_start_point + ". " + Strings.In_endurance_mode_travel_in_the_direction_of_the_green_arrow, 1, TimeSpan.FromSeconds(0.30000001192092896), Color.White)
		{
			this._playerMode = PlayerMode.Generic;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CompassEntity ent = new CompassEntity(this._model, use, attachedToLocalPlayer);
			if (use != ItemUse.UI)
			{
				ent.TrackPosition = false;
			}
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 1.5707964f, 0f);
				Matrix i = Matrix.Transform(Matrix.CreateScale(22.4f / ent.GetLocalBoundingSphere().Radius), mat);
				i.Translation = new Vector3(0f, 0f, 0f);
				ent.LocalToParent = i;
				ent.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				ent.TrackPosition = true;
				ent.LocalRotation = new Quaternion(0.6469873f, 0.1643085f, 0.7078394f, -0.2310277f);
				ent.LocalPosition = new Vector3(0f, 0.09360941f, 0f);
				break;
			}
			return ent;
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
