using System;
using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.Inventory
{
	public class SpadeInventoryItem : InventoryItem
	{
		public SpadeInventoryItem(InventoryItem.InventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
			SpadeInventoryClass spadeInventoryClass = (SpadeInventoryClass)base.ItemClass;
		}

		public override TimeSpan TimeToDig(BlockTypeEnum blockType)
		{
			SpadeInventoryClass pcls = (SpadeInventoryClass)base.ItemClass;
			switch (pcls.Material)
			{
			case ToolMaterialTypes.Wood:
				return base.TimeToDig(blockType);
			case ToolMaterialTypes.Stone:
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.Grass:
					break;
				case BlockTypeEnum.Sand:
					return TimeSpan.FromSeconds(0.75);
				default:
					if (blockType == BlockTypeEnum.Snow)
					{
						return TimeSpan.FromSeconds(0.75);
					}
					switch (blockType)
					{
					case BlockTypeEnum.TNT:
					case BlockTypeEnum.C4:
						break;
					default:
						goto IL_02DD;
					}
					break;
				}
				return TimeSpan.FromSeconds(1.0);
			case ToolMaterialTypes.Copper:
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.Grass:
					break;
				case BlockTypeEnum.Sand:
					return TimeSpan.FromSeconds(0.5);
				default:
					if (blockType == BlockTypeEnum.Snow)
					{
						return TimeSpan.FromSeconds(0.5);
					}
					switch (blockType)
					{
					case BlockTypeEnum.TNT:
					case BlockTypeEnum.C4:
						break;
					default:
						goto IL_02DD;
					}
					break;
				}
				return TimeSpan.FromSeconds(0.5);
			case ToolMaterialTypes.Iron:
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
					return TimeSpan.FromSeconds(0.25);
				case BlockTypeEnum.Grass:
					break;
				case BlockTypeEnum.Sand:
					return TimeSpan.FromSeconds(0.25);
				default:
					if (blockType == BlockTypeEnum.Snow)
					{
						return TimeSpan.FromSeconds(0.25);
					}
					switch (blockType)
					{
					case BlockTypeEnum.TNT:
					case BlockTypeEnum.C4:
						break;
					default:
						goto IL_02DD;
					}
					break;
				}
				return TimeSpan.FromSeconds(0.25);
			case ToolMaterialTypes.Gold:
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.Grass:
					break;
				case BlockTypeEnum.Sand:
					return TimeSpan.FromSeconds(0.1);
				default:
					if (blockType == BlockTypeEnum.Snow)
					{
						return TimeSpan.FromSeconds(0.1);
					}
					switch (blockType)
					{
					case BlockTypeEnum.TNT:
					case BlockTypeEnum.C4:
						break;
					default:
						goto IL_02DD;
					}
					break;
				}
				return TimeSpan.FromSeconds(0.1);
			case ToolMaterialTypes.Diamond:
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
					return TimeSpan.FromSeconds(0.0);
				case BlockTypeEnum.Grass:
					break;
				case BlockTypeEnum.Sand:
					return TimeSpan.FromSeconds(0.0);
				default:
					if (blockType == BlockTypeEnum.Snow)
					{
						return TimeSpan.FromSeconds(0.0);
					}
					switch (blockType)
					{
					case BlockTypeEnum.TNT:
					case BlockTypeEnum.C4:
						break;
					default:
						goto IL_02DD;
					}
					break;
				}
				return TimeSpan.FromSeconds(0.0);
			case ToolMaterialTypes.BloodStone:
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
					return TimeSpan.FromSeconds(0.0);
				case BlockTypeEnum.Grass:
					break;
				case BlockTypeEnum.Sand:
					return TimeSpan.FromSeconds(0.0);
				default:
					if (blockType == BlockTypeEnum.Snow)
					{
						return TimeSpan.FromSeconds(0.0);
					}
					switch (blockType)
					{
					case BlockTypeEnum.TNT:
					case BlockTypeEnum.C4:
						break;
					default:
						goto IL_02DD;
					}
					break;
				}
				return TimeSpan.FromSeconds(0.0);
			}
			IL_02DD:
			return base.TimeToDig(blockType);
		}
	}
}
