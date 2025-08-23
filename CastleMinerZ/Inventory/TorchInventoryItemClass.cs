using System;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class TorchInventoryItemClass : ItemBlockInventoryItemClass
	{
		public TorchInventoryItemClass()
			: base(InventoryItemIDs.Torch, BlockTypeEnum.Torch, Strings.Use_these_to_light_your_world + ". " + Strings.They_also_keep_some_monsters_away)
		{
			this._playerMode = PlayerMode.Pick;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new TorchInventoryitem(this, stackCount);
		}

		public override Entity CreateWorldEntity(bool attachedToLocalPlayer, BlockTypeEnum blockType, DoorEntity.ModelNameEnum modelName)
		{
			TorchEntity entity = new TorchEntity(false);
			entity.SetPosition(BlockType.GetType(blockType).Facing);
			return entity;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			Entity entity = null;
			switch (use)
			{
			case ItemUse.UI:
			{
				ModelEntity me = new ModelEntity(TorchEntity._torchModel);
				me.EnableDefaultLighting();
				entity = me;
				Quaternion mat = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
				float scale = 32f / me.GetLocalBoundingSphere().Radius;
				Matrix i = Matrix.Transform(Matrix.CreateScale(scale), mat);
				i.Translation = new Vector3(-15f, -15f, 0f);
				me.LocalToParent = i;
				break;
			}
			case ItemUse.Hand:
			{
				TorchEntity te = new TorchEntity(false);
				entity = te;
				te.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 1.5707964f) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.7853982f);
				te.LocalScale = new Vector3(0.5f, 0.5f, 0.5f);
				te.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
				break;
			}
			case ItemUse.Pickup:
			{
				TorchEntity te2 = new TorchEntity(false);
				entity = te2;
				break;
			}
			}
			return entity;
		}
	}
}
