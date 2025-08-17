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
			ClockEntity clockEntity = new ClockEntity(this._model, use, attachedToLocalPlayer);
			if (use != ItemUse.UI)
			{
				clockEntity.TrackPosition = false;
			}
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 1.5707964f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(22.4f / clockEntity.GetLocalBoundingSphere().Radius), quaternion);
				matrix.Translation = new Vector3(0f, 0f, 0f);
				clockEntity.LocalToParent = matrix;
				clockEntity.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				clockEntity.TrackPosition = true;
				clockEntity.LocalRotation = new Quaternion(0.6469873f, 0.1643085f, 0.7078394f, -0.2310277f);
				clockEntity.LocalPosition = new Vector3(0f, 0.09360941f, 0f);
				break;
			}
			return clockEntity;
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
