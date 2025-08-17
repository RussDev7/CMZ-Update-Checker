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
			TorchEntity torchEntity = new TorchEntity(false);
			torchEntity.SetPosition(BlockType.GetType(blockType).Facing);
			return torchEntity;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			Entity entity = null;
			switch (use)
			{
			case ItemUse.UI:
			{
				ModelEntity modelEntity = new ModelEntity(TorchEntity._torchModel);
				modelEntity.EnableDefaultLighting();
				entity = modelEntity;
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
				float num = 32f / modelEntity.GetLocalBoundingSphere().Radius;
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(num), quaternion);
				matrix.Translation = new Vector3(-15f, -15f, 0f);
				modelEntity.LocalToParent = matrix;
				break;
			}
			case ItemUse.Hand:
			{
				TorchEntity torchEntity = new TorchEntity(false);
				entity = torchEntity;
				torchEntity.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 1.5707964f) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.7853982f);
				torchEntity.LocalScale = new Vector3(0.5f, 0.5f, 0.5f);
				torchEntity.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
				break;
			}
			case ItemUse.Pickup:
			{
				TorchEntity torchEntity2 = new TorchEntity(false);
				entity = torchEntity2;
				break;
			}
			}
			return entity;
		}
	}
}
