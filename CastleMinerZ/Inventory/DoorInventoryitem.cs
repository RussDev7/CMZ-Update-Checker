using System;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Inventory
{
	public class DoorInventoryitem : BlockInventoryItem
	{
		public DoorInventoryitem(BlockInventoryItemClass classtype, int stackCount)
			: base(classtype, stackCount)
		{
			this._baseDoorType = classtype.BlockType._type;
		}

		public override void SetLocation(IntVector3 location)
		{
			if (BlockType.IsUpperDoor(base.BlockTypeID))
			{
				location += new IntVector3(0, -1, 0);
			}
			base.SetLocation(location);
		}

		public override void SetModelNameIndex(int modelNameIndex)
		{
			DoorInventoryItemClass doorInventoryItemClass = base.ItemClass as DoorInventoryItemClass;
			doorInventoryItemClass.ModelName = (DoorEntity.ModelNameEnum)modelNameIndex;
			base.SetModelNameIndex(modelNameIndex);
		}

		public override void AlterBlock(Player player, IntVector3 addSpot, BlockFace inFace)
		{
			base.AlterBlock(player, addSpot, inFace);
			AlterBlockMessage.Send((LocalNetworkGamer)player.Gamer, addSpot + new IntVector3(0, 1, 0), DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalUpperDoorClosed, this._baseDoorType));
		}

		public override bool CanPlaceHere(IntVector3 addSpot, BlockFace inFace)
		{
			BlockTerrain.Instance.GetBlockWithChanges(addSpot + new IntVector3(1, 0, 0));
			BlockTerrain.Instance.GetBlockWithChanges(addSpot + new IntVector3(-1, 0, 0));
			BlockTerrain.Instance.GetBlockWithChanges(addSpot + new IntVector3(0, 0, 1));
			BlockTerrain.Instance.GetBlockWithChanges(addSpot + new IntVector3(0, 0, -1));
			BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(addSpot + new IntVector3(0, -1, 0));
			BlockTypeEnum blockWithChanges2 = BlockTerrain.Instance.GetBlockWithChanges(addSpot + new IntVector3(0, 1, 0));
			return !BlockType.IsEmpty(blockWithChanges) && BlockType.IsEmpty(blockWithChanges2);
		}

		public static BlockTypeEnum GetDoorPiece(BlockTypeEnum baseDoorPiece, BlockTypeEnum doorTypeEnum)
		{
			if (doorTypeEnum == BlockTypeEnum.NormalLowerDoor)
			{
				return baseDoorPiece;
			}
			if (doorTypeEnum == BlockTypeEnum.StrongLowerDoor)
			{
				switch (baseDoorPiece)
				{
				case BlockTypeEnum.NormalLowerDoorClosedZ:
					return BlockTypeEnum.StrongLowerDoorClosedZ;
				case BlockTypeEnum.NormalLowerDoorClosedX:
					return BlockTypeEnum.StrongLowerDoorClosedX;
				case BlockTypeEnum.NormalUpperDoorClosed:
					return BlockTypeEnum.StrongUpperDoorClosed;
				case BlockTypeEnum.NormalLowerDoorOpenZ:
					return BlockTypeEnum.StrongLowerDoorOpenZ;
				case BlockTypeEnum.NormalLowerDoorOpenX:
					return BlockTypeEnum.StrongLowerDoorOpenX;
				case BlockTypeEnum.NormalUpperDoorOpen:
					return BlockTypeEnum.StrongUpperDoorOpen;
				}
			}
			return baseDoorPiece;
		}

		public override BlockTypeEnum GetConstructedBlockType(BlockFace face, IntVector3 position)
		{
			BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(position + new IntVector3(1, 0, 0));
			BlockTypeEnum blockWithChanges2 = BlockTerrain.Instance.GetBlockWithChanges(position + new IntVector3(-1, 0, 0));
			BlockTypeEnum blockWithChanges3 = BlockTerrain.Instance.GetBlockWithChanges(position + new IntVector3(0, 0, 1));
			BlockTypeEnum blockWithChanges4 = BlockTerrain.Instance.GetBlockWithChanges(position + new IntVector3(0, 0, -1));
			if (!BlockType.IsEmpty(blockWithChanges) && !BlockType.IsEmpty(blockWithChanges2))
			{
				return DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalLowerDoorClosedX, this._baseDoorType);
			}
			if (!BlockType.IsEmpty(blockWithChanges3) && !BlockType.IsEmpty(blockWithChanges4))
			{
				return DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalLowerDoorClosedZ, this._baseDoorType);
			}
			if (!BlockType.IsEmpty(blockWithChanges) || !BlockType.IsEmpty(blockWithChanges2))
			{
				return DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalLowerDoorClosedX, this._baseDoorType);
			}
			if (!BlockType.IsEmpty(blockWithChanges3) || !BlockType.IsEmpty(blockWithChanges4))
			{
				return DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalLowerDoorClosedZ, this._baseDoorType);
			}
			return DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalLowerDoorClosedZ, this._baseDoorType);
		}

		private BlockTypeEnum _baseDoorType = BlockTypeEnum.NormalLowerDoor;
	}
}
