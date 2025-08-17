using System;
using DNA.Audio;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;

namespace DNA.CastleMinerZ.Inventory
{
	public class PickInventoryItem : InventoryItem
	{
		public PickInventoryItem(InventoryItem.InventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
			PickInventoryItemClass pickInventoryItemClass = (PickInventoryItemClass)base.ItemClass;
			if (pickInventoryItemClass.ID == InventoryItemIDs.IronLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.GoldLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.DiamondLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.BloodStoneLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.CopperLaserSword)
			{
				this._useSound = "LightSaberSwing";
			}
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (this._useSound != null && (controller.Use.Held || controller.Shoulder.Held) && base.CoolDownTimer.Expired)
			{
				SoundManager.Instance.PlayInstance(this._useSound);
			}
			base.ProcessInput(hud, controller);
		}

		public override InventoryItem CreatesWhenDug(BlockTypeEnum block, IntVector3 location)
		{
			PickInventoryItemClass pickInventoryItemClass = (PickInventoryItemClass)base.ItemClass;
			InventoryItemIDs outputFromBlock = this.GetOutputFromBlock(block);
			int pickaxeBlockTier = this.GetPickaxeBlockTier(block);
			int toolTier = this.GetToolTier(pickInventoryItemClass);
			int num = 1;
			if (toolTier >= pickaxeBlockTier && outputFromBlock != InventoryItemIDs.BareHands)
			{
				return InventoryItem.CreateItem(outputFromBlock, num);
			}
			return base.CreatesWhenDug(block, location);
		}

		private InventoryItemIDs MatchOutputFromList(BlockTypeEnum block, BlockTypeEnum[] harvestList)
		{
			InventoryItemIDs inventoryItemIDs = InventoryItemIDs.RockBlock;
			for (int i = 0; i < harvestList.Length; i++)
			{
				if (harvestList[i] == block)
				{
					inventoryItemIDs = this.GetOutputFromBlock(block);
				}
			}
			return inventoryItemIDs;
		}

		private InventoryItemIDs GetOutputFromBlock(BlockTypeEnum block)
		{
			InventoryItemIDs inventoryItemIDs = InventoryItemIDs.BareHands;
			switch (block)
			{
			case BlockTypeEnum.GoldOre:
				inventoryItemIDs = InventoryItemIDs.GoldOre;
				break;
			case BlockTypeEnum.IronOre:
				inventoryItemIDs = InventoryItemIDs.IronOre;
				break;
			case BlockTypeEnum.CopperOre:
				inventoryItemIDs = InventoryItemIDs.CopperOre;
				break;
			case BlockTypeEnum.CoalOre:
				inventoryItemIDs = InventoryItemIDs.Coal;
				break;
			case BlockTypeEnum.DiamondOre:
				inventoryItemIDs = InventoryItemIDs.Diamond;
				break;
			default:
				switch (block)
				{
				case BlockTypeEnum.IronWall:
					inventoryItemIDs = InventoryItemIDs.IronWall;
					break;
				case BlockTypeEnum.CopperWall:
					inventoryItemIDs = InventoryItemIDs.CopperWall;
					break;
				case BlockTypeEnum.GoldenWall:
					inventoryItemIDs = InventoryItemIDs.GoldenWall;
					break;
				case BlockTypeEnum.DiamondWall:
					inventoryItemIDs = InventoryItemIDs.DiamondWall;
					break;
				}
				break;
			}
			return inventoryItemIDs;
		}

		private int GetPickaxeBlockTier(BlockTypeEnum blockType)
		{
			int num = int.MaxValue;
			switch (blockType)
			{
			case BlockTypeEnum.Dirt:
			case BlockTypeEnum.Grass:
			case BlockTypeEnum.Sand:
			case BlockTypeEnum.Rock:
			case BlockTypeEnum.Snow:
			case BlockTypeEnum.Leaves:
			case BlockTypeEnum.Wood:
				num = 0;
				break;
			case BlockTypeEnum.Lantern:
			case BlockTypeEnum.FixedLantern:
			case BlockTypeEnum.SurfaceLava:
			case BlockTypeEnum.DeepLava:
			case BlockTypeEnum.Bedrock:
			case BlockTypeEnum.Log:
				break;
			case BlockTypeEnum.GoldOre:
				num = 5;
				break;
			case BlockTypeEnum.IronOre:
				num = 3;
				break;
			case BlockTypeEnum.CopperOre:
				num = 2;
				break;
			case BlockTypeEnum.CoalOre:
				num = 1;
				break;
			case BlockTypeEnum.DiamondOre:
				num = 6;
				break;
			case BlockTypeEnum.Ice:
				num = 0;
				break;
			case BlockTypeEnum.BloodStone:
				num = 8;
				break;
			case BlockTypeEnum.SpaceRock:
				num = 9;
				break;
			case BlockTypeEnum.IronWall:
				num = 3;
				break;
			case BlockTypeEnum.CopperWall:
				num = 2;
				break;
			case BlockTypeEnum.GoldenWall:
				num = 4;
				break;
			case BlockTypeEnum.DiamondWall:
				num = 5;
				break;
			default:
				switch (blockType)
				{
				case BlockTypeEnum.Slime:
					num = 10;
					break;
				case BlockTypeEnum.SpaceRockInventory:
					num = 9;
					break;
				default:
					switch (blockType)
					{
					case BlockTypeEnum.LootBlock:
						num = 3;
						break;
					case BlockTypeEnum.LuckyLootBlock:
						num = 6;
						break;
					}
					break;
				}
				break;
			}
			return num;
		}

		private int GetToolTier(PickInventoryItemClass pcls)
		{
			int num = 0;
			if (pcls.ID == InventoryItemIDs.IronLaserSword || pcls.ID == InventoryItemIDs.GoldLaserSword || pcls.ID == InventoryItemIDs.DiamondLaserSword || pcls.ID == InventoryItemIDs.BloodStoneLaserSword || pcls.ID == InventoryItemIDs.CopperLaserSword)
			{
				num = 12;
			}
			else
			{
				switch (pcls.Material)
				{
				case ToolMaterialTypes.Wood:
					num = 1;
					break;
				case ToolMaterialTypes.Stone:
					num = 3;
					break;
				case ToolMaterialTypes.Copper:
					num = 4;
					break;
				case ToolMaterialTypes.Iron:
					num = 5;
					break;
				case ToolMaterialTypes.Gold:
					num = 6;
					break;
				case ToolMaterialTypes.Diamond:
					num = 8;
					break;
				case ToolMaterialTypes.BloodStone:
					num = 10;
					break;
				}
			}
			return num;
		}

		private float GetBlockDifficultyModifier(int blockTier)
		{
			float num = 5f;
			switch (blockTier)
			{
			case 0:
				num = 0.1f;
				break;
			case 1:
				num = 0.25f;
				break;
			case 2:
				num = 0.5f;
				break;
			case 3:
				num = 1f;
				break;
			case 4:
				num = 1.5f;
				break;
			case 5:
				num = 2f;
				break;
			case 6:
				num = 3f;
				break;
			case 7:
				num = 4.5f;
				break;
			case 8:
				num = 6f;
				break;
			case 9:
				num = 8f;
				break;
			case 10:
				num = 10f;
				break;
			}
			return num;
		}

		private float GetDifficultyRatio(int diff)
		{
			float num = 1f;
			switch (diff)
			{
			case 0:
				num = 1f;
				break;
			case 1:
				num = 0.75f;
				break;
			case 2:
				num = 0.5f;
				break;
			case 3:
				num = 0.35f;
				break;
			case 4:
				num = 0.25f;
				break;
			case 5:
				num = 0.2f;
				break;
			case 6:
				num = 0.17f;
				break;
			case 7:
				num = 0.15f;
				break;
			case 8:
				num = 0.14f;
				break;
			case 9:
				num = 0.13f;
				break;
			case 10:
				num = 0.12f;
				break;
			}
			return num;
		}

		private float GetAverageDigTimeForToolTier(int toolTier)
		{
			float num = 20f;
			switch (toolTier)
			{
			case 0:
				num = 16f;
				break;
			case 1:
				num = 14f;
				break;
			case 2:
				num = 12f;
				break;
			case 3:
				num = 10f;
				break;
			case 4:
				num = 9f;
				break;
			case 5:
				num = 6f;
				break;
			case 6:
				num = 4f;
				break;
			case 7:
				num = 3f;
				break;
			case 8:
				num = 2f;
				break;
			case 9:
				num = 1.5f;
				break;
			case 10:
				num = 1f;
				break;
			case 12:
				num = 0.5f;
				break;
			}
			return num;
		}

		public override TimeSpan TimeToDig(BlockTypeEnum blockType)
		{
			PickInventoryItemClass pickInventoryItemClass = (PickInventoryItemClass)base.ItemClass;
			int pickaxeBlockTier = this.GetPickaxeBlockTier(blockType);
			int toolTier = this.GetToolTier(pickInventoryItemClass);
			if (pickaxeBlockTier > toolTier)
			{
				return base.TimeToDig(blockType);
			}
			float num = this.ComputeDigTime(toolTier, pickaxeBlockTier);
			return TimeSpan.FromSeconds((double)num);
		}

		private float ComputeDigTime(int toolTier, int blockTier)
		{
			int num = toolTier - blockTier;
			float num2 = this.GetAverageDigTimeForToolTier(toolTier);
			float blockDifficultyModifier = this.GetBlockDifficultyModifier(blockTier);
			num2 *= blockDifficultyModifier;
			num2 *= this.GetDifficultyRatio(num);
			if (num2 < 0.01f)
			{
				num2 = 0.01f;
			}
			return num2;
		}

		public TimeSpan TimeToDigOld(BlockTypeEnum blockType)
		{
			PickInventoryItemClass pickInventoryItemClass = (PickInventoryItemClass)base.ItemClass;
			if (pickInventoryItemClass.ID == InventoryItemIDs.IronLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.GoldLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.DiamondLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.BloodStoneLaserSword || pickInventoryItemClass.ID == InventoryItemIDs.CopperLaserSword)
			{
				switch (blockType)
				{
				case BlockTypeEnum.Dirt:
				case BlockTypeEnum.Grass:
				case BlockTypeEnum.Sand:
				case BlockTypeEnum.Rock:
				case BlockTypeEnum.Snow:
				case BlockTypeEnum.Leaves:
				case BlockTypeEnum.Wood:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.Lantern:
				case BlockTypeEnum.FixedLantern:
				case BlockTypeEnum.SurfaceLava:
				case BlockTypeEnum.DeepLava:
				case BlockTypeEnum.Bedrock:
				case BlockTypeEnum.Log:
					break;
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.BloodStone:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.SpaceRock:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.DiamondWall:
					return TimeSpan.FromSeconds(0.5);
				default:
					switch (blockType)
					{
					case BlockTypeEnum.Slime:
						return TimeSpan.FromSeconds(4.0);
					case BlockTypeEnum.SpaceRockInventory:
						return TimeSpan.FromSeconds(2.0);
					}
					break;
				}
			}
			switch (pickInventoryItemClass.Material)
			{
			case ToolMaterialTypes.Wood:
				return base.TimeToDig(blockType);
			case ToolMaterialTypes.Stone:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.GoldOre:
					break;
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(7.0);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(6.0);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(3.0);
				default:
					if (blockType == BlockTypeEnum.Ice)
					{
						return TimeSpan.FromSeconds(1.0);
					}
					break;
				}
				break;
			case ToolMaterialTypes.Copper:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.GoldOre:
					break;
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(6.0);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(1.5);
				default:
					if (blockType == BlockTypeEnum.Ice)
					{
						return TimeSpan.FromSeconds(0.75);
					}
					if (blockType == BlockTypeEnum.CopperWall)
					{
						return TimeSpan.FromSeconds(3.0);
					}
					break;
				}
				break;
			case ToolMaterialTypes.Iron:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(6.0);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.DiamondOre:
				case BlockTypeEnum.SurfaceLava:
				case BlockTypeEnum.DeepLava:
				case BlockTypeEnum.Bedrock:
				case BlockTypeEnum.Snow:
					break;
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.25);
				default:
					switch (blockType)
					{
					case BlockTypeEnum.IronWall:
						return TimeSpan.FromSeconds(3.0);
					case BlockTypeEnum.CopperWall:
						return TimeSpan.FromSeconds(1.5);
					}
					break;
				}
				break;
			case ToolMaterialTypes.Gold:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.25);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(5.0);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(3.0);
				}
				break;
			case ToolMaterialTypes.Diamond:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.25);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.05);
				case BlockTypeEnum.BloodStone:
					return TimeSpan.FromSeconds(8.0);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(2.0);
				case BlockTypeEnum.DiamondWall:
					return TimeSpan.FromSeconds(3.0);
				}
				break;
			case ToolMaterialTypes.BloodStone:
				switch (blockType)
				{
				case BlockTypeEnum.Rock:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.GoldOre:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.IronOre:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.CopperOre:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.CoalOre:
					return TimeSpan.FromSeconds(0.1);
				case BlockTypeEnum.DiamondOre:
					return TimeSpan.FromSeconds(1.5);
				case BlockTypeEnum.SurfaceLava:
				case BlockTypeEnum.DeepLava:
				case BlockTypeEnum.Bedrock:
				case BlockTypeEnum.Snow:
				case BlockTypeEnum.Log:
				case BlockTypeEnum.Leaves:
				case BlockTypeEnum.Wood:
					break;
				case BlockTypeEnum.Ice:
					return TimeSpan.FromSeconds(0.01);
				case BlockTypeEnum.BloodStone:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.SpaceRock:
					return TimeSpan.FromSeconds(3.0);
				case BlockTypeEnum.IronWall:
					return TimeSpan.FromSeconds(0.5);
				case BlockTypeEnum.CopperWall:
					return TimeSpan.FromSeconds(0.2);
				case BlockTypeEnum.GoldenWall:
					return TimeSpan.FromSeconds(1.0);
				case BlockTypeEnum.DiamondWall:
					return TimeSpan.FromSeconds(1.5);
				default:
					switch (blockType)
					{
					case BlockTypeEnum.Slime:
						return TimeSpan.FromSeconds(6.0);
					case BlockTypeEnum.SpaceRockInventory:
						return TimeSpan.FromSeconds(3.0);
					}
					break;
				}
				break;
			}
			return base.TimeToDig(blockType);
		}

		private const InventoryItemIDs CANNOT_HARVEST_ID = InventoryItemIDs.BareHands;

		private string _useSound;
	}
}
