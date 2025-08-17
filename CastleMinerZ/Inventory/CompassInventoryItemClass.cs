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
			CompassEntity compassEntity = new CompassEntity(this._model, use, attachedToLocalPlayer);
			if (use != ItemUse.UI)
			{
				compassEntity.TrackPosition = false;
			}
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 1.5707964f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(22.4f / compassEntity.GetLocalBoundingSphere().Radius), quaternion);
				matrix.Translation = new Vector3(0f, 0f, 0f);
				compassEntity.LocalToParent = matrix;
				compassEntity.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				compassEntity.TrackPosition = true;
				compassEntity.LocalRotation = new Quaternion(0.6469873f, 0.1643085f, 0.7078394f, -0.2310277f);
				compassEntity.LocalPosition = new Vector3(0f, 0.09360941f, 0f);
				break;
			}
			return compassEntity;
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
