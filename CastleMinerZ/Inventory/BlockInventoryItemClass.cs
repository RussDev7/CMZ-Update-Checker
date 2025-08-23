using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class BlockInventoryItemClass : InventoryItem.InventoryItemClass
	{
		public override InventoryItem CreateItem(int stackCount)
		{
			return new BlockInventoryItem(this, stackCount);
		}

		public static InventoryItem CreateBlockItem(BlockTypeEnum blockType, int stackCount, IntVector3 location)
		{
			Door door = null;
			if (BlockType.IsDoor(blockType))
			{
				door = CastleMinerZGame.Instance.CurrentWorld.GetDoor(location);
			}
			if (door != null)
			{
				InventoryItem.InventoryItemClass blockClass;
				if (BlockInventoryItemClass.DoorClasses.TryGetValue(door.ModelName, out blockClass))
				{
					return blockClass.CreateItem(stackCount);
				}
				return null;
			}
			else
			{
				InventoryItem.InventoryItemClass blockClass;
				if (BlockInventoryItemClass.BlockClasses.TryGetValue(BlockType.GetType(blockType).ParentBlockType, out blockClass))
				{
					return blockClass.CreateItem(stackCount);
				}
				return null;
			}
		}

		public BlockInventoryItemClass(InventoryItemIDs id, BlockTypeEnum blockType, string description, float meleeDamage)
			: base(id, BlockType.GetType(blockType).Name, description, 999, TimeSpan.FromSeconds(0.10000000149011612), "Place")
		{
			if (blockType == BlockTypeEnum.NormalLowerDoor)
			{
				blockType = BlockTypeEnum.NormalLowerDoor;
			}
			this.BlockType = BlockType.GetType(blockType);
			BlockInventoryItemClass.BlockClasses[blockType] = this;
			this._playerMode = PlayerMode.Block;
			this.EnemyDamageType = DamageType.BLUNT;
			this.EnemyDamage = meleeDamage;
		}

		public BlockInventoryItemClass(InventoryItemIDs id, BlockTypeEnum blockType, string description, float meleeDamage, int maxStackCount)
			: base(id, BlockType.GetType(blockType).Name, description, maxStackCount, TimeSpan.FromSeconds(0.10000000149011612), "Place")
		{
			if (blockType == BlockTypeEnum.NormalLowerDoor)
			{
				blockType = BlockTypeEnum.NormalLowerDoor;
			}
			this.BlockType = BlockType.GetType(blockType);
			BlockInventoryItemClass.BlockClasses[blockType] = this;
			this._playerMode = PlayerMode.Block;
			this.EnemyDamageType = DamageType.BLUNT;
			this.EnemyDamage = meleeDamage;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			BlockEntity entity = new BlockEntity(this.BlockType._type, use, attachedToLocalPlayer);
			switch (use)
			{
			case ItemUse.UI:
			{
				Matrix i = Matrix.CreateFromYawPitchRoll(3.1415927f, -0.2617994f, 0.2617994f);
				entity.LocalToParent = i;
				entity.UIObject();
				entity.Scale = 38.4f;
				break;
			}
			case ItemUse.Hand:
				entity.LocalRotation = new Quaternion(-0.02067951f, -0.007977718f, 0.03257636f, 0.9992235f);
				entity.LocalPosition = new Vector3(0f, 0.06533548f, 0f);
				entity.Scale = 0.1f;
				break;
			case ItemUse.Pickup:
				entity.Scale = 0.2f;
				break;
			}
			entity.Update(CastleMinerZGame.Instance, new GameTime());
			return entity;
		}

		public static Dictionary<BlockTypeEnum, InventoryItem.InventoryItemClass> BlockClasses = new Dictionary<BlockTypeEnum, InventoryItem.InventoryItemClass>();

		public static Dictionary<DoorEntity.ModelNameEnum, InventoryItem.InventoryItemClass> DoorClasses = new Dictionary<DoorEntity.ModelNameEnum, InventoryItem.InventoryItemClass>();

		public BlockType BlockType;
	}
}
