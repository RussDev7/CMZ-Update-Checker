using System;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class DoorInventoryItemClass : ItemBlockInventoryItemClass
	{
		public DoorInventoryItemClass(InventoryItemIDs itemID, BlockTypeEnum blockType, DoorEntity.ModelNameEnum modelName, string longDescription)
			: base(itemID, blockType, longDescription)
		{
			this._playerMode = PlayerMode.Block;
			this.ModelName = modelName;
			this.ModelNameIndex = (int)DoorEntity.GetModelNameFromInventoryId(itemID);
			BlockInventoryItemClass.DoorClasses[modelName] = this;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new DoorInventoryitem(this, stackCount);
		}

		public override Entity CreateWorldEntity(bool attachedToLocalPlayer, BlockTypeEnum blockType, DoorEntity.ModelNameEnum modelName)
		{
			DoorEntity doorEntity = new DoorEntity(modelName, blockType);
			doorEntity.SetPosition(blockType);
			return doorEntity;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			Entity entity = null;
			switch (use)
			{
			case ItemUse.UI:
			{
				ModelEntity modelEntity = new ModelEntity(DoorEntity.GetDoorModel(this.ModelName));
				modelEntity.EnableDefaultLighting();
				entity = modelEntity;
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f);
				float num = 31.36f / modelEntity.GetLocalBoundingSphere().Radius;
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(num), quaternion);
				matrix.Translation = new Vector3(-14f, -28f, 0f);
				modelEntity.LocalToParent = matrix;
				break;
			}
			case ItemUse.Hand:
			{
				this.ModelName = DoorEntity.GetModelNameFromInventoryId(this.ID);
				DoorEntity doorEntity = new DoorEntity(this.ModelName, this.BlockType._type);
				entity = doorEntity;
				doorEntity.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 1.5707964f) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.7853982f);
				doorEntity.LocalScale = new Vector3(0.1f, 0.1f, 0.1f);
				doorEntity.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
				break;
			}
			case ItemUse.Pickup:
			{
				Door door = CastleMinerZGame.Instance.CurrentWorld.GetDoor(this.Location);
				if (door != null)
				{
					this.ModelName = door.ModelName;
				}
				DoorEntity doorEntity2 = new DoorEntity(this.ModelName, this.BlockType._type);
				entity = doorEntity2;
				break;
			}
			}
			if (entity != null)
			{
				entity.EntityColor = new Color?(Color.Gray);
			}
			return entity;
		}

		public DoorEntity.ModelNameEnum ModelName = DoorEntity.ModelNameEnum.Wood;
	}
}
