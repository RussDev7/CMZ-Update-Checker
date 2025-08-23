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
			PickInventoryItemClass pcls = (PickInventoryItemClass)base.ItemClass;
			if (pcls.ID == InventoryItemIDs.IronLaserSword || pcls.ID == InventoryItemIDs.GoldLaserSword || pcls.ID == InventoryItemIDs.DiamondLaserSword || pcls.ID == InventoryItemIDs.BloodStoneLaserSword || pcls.ID == InventoryItemIDs.CopperLaserSword)
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
			PickInventoryItemClass pcls = (PickInventoryItemClass)base.ItemClass;
			InventoryItemIDs output = this.GetOutputFromBlock(block);
			int blockTier = this.GetPickaxeBlockTier(block);
			int toolTier = this.GetToolTier(pcls);
			int quantity = 1;
			if (toolTier >= blockTier && output != InventoryItemIDs.BareHands)
			{
				return InventoryItem.CreateItem(output, quantity);
			}
			return base.CreatesWhenDug(block, location);
		}

		private InventoryItemIDs MatchOutputFromList(BlockTypeEnum block, BlockTypeEnum[] harvestList)
		{
			InventoryItemIDs output = InventoryItemIDs.RockBlock;
			for (int i = 0; i < harvestList.Length; i++)
			{
				if (harvestList[i] == block)
				{
					output = this.GetOutputFromBlock(block);
				}
			}
			return output;
		}

		private InventoryItemIDs GetOutputFromBlock(BlockTypeEnum block)
		{
			InventoryItemIDs output = InventoryItemIDs.BareHands;
			switch (block)
			{
			case BlockTypeEnum.GoldOre:
				output = InventoryItemIDs.GoldOre;
				break;
			case BlockTypeEnum.IronOre:
				output = InventoryItemIDs.IronOre;
				break;
			case BlockTypeEnum.CopperOre:
				output = InventoryItemIDs.CopperOre;
				break;
			case BlockTypeEnum.CoalOre:
				output = InventoryItemIDs.Coal;
				break;
			case BlockTypeEnum.DiamondOre:
				output = InventoryItemIDs.Diamond;
				break;
			default:
				switch (block)
				{
				case BlockTypeEnum.IronWall:
					output = InventoryItemIDs.IronWall;
					break;
				case BlockTypeEnum.CopperWall:
					output = InventoryItemIDs.CopperWall;
					break;
				case BlockTypeEnum.GoldenWall:
					output = InventoryItemIDs.GoldenWall;
					break;
				case BlockTypeEnum.DiamondWall:
					output = InventoryItemIDs.DiamondWall;
					break;
				}
				break;
			}
			return output;
		}

		private int GetPickaxeBlockTier(BlockTypeEnum blockType)
		{
			int blockTier = int.MaxValue;
			switch (blockType)
			{
			case BlockTypeEnum.Dirt:
			case BlockTypeEnum.Grass:
			case BlockTypeEnum.Sand:
			case BlockTypeEnum.Rock:
			case BlockTypeEnum.Snow:
			case BlockTypeEnum.Leaves:
			case BlockTypeEnum.Wood:
				blockTier = 0;
				break;
			case BlockTypeEnum.Lantern:
			case BlockTypeEnum.FixedLantern:
			case BlockTypeEnum.SurfaceLava:
			case BlockTypeEnum.DeepLava:
			case BlockTypeEnum.Bedrock:
			case BlockTypeEnum.Log:
				break;
			case BlockTypeEnum.GoldOre:
				blockTier = 5;
				break;
			case BlockTypeEnum.IronOre:
				blockTier = 3;
				break;
			case BlockTypeEnum.CopperOre:
				blockTier = 2;
				break;
			case BlockTypeEnum.CoalOre:
				blockTier = 1;
				break;
			case BlockTypeEnum.DiamondOre:
				blockTier = 6;
				break;
			case BlockTypeEnum.Ice:
				blockTier = 0;
				break;
			case BlockTypeEnum.BloodStone:
				blockTier = 8;
				break;
			case BlockTypeEnum.SpaceRock:
				blockTier = 9;
				break;
			case BlockTypeEnum.IronWall:
				blockTier = 3;
				break;
			case BlockTypeEnum.CopperWall:
				blockTier = 2;
				break;
			case BlockTypeEnum.GoldenWall:
				blockTier = 4;
				break;
			case BlockTypeEnum.DiamondWall:
				blockTier = 5;
				break;
			default:
				switch (blockType)
				{
				case BlockTypeEnum.Slime:
					blockTier = 10;
					break;
				case BlockTypeEnum.SpaceRockInventory:
					blockTier = 9;
					break;
				default:
					switch (blockType)
					{
					case BlockTypeEnum.LootBlock:
						blockTier = 3;
						break;
					case BlockTypeEnum.LuckyLootBlock:
						blockTier = 6;
						break;
					}
					break;
				}
				break;
			}
			return blockTier;
		}

		private int GetToolTier(PickInventoryItemClass pcls)
		{
			int toolTier = 0;
			if (pcls.ID == InventoryItemIDs.IronLaserSword || pcls.ID == InventoryItemIDs.GoldLaserSword || pcls.ID == InventoryItemIDs.DiamondLaserSword || pcls.ID == InventoryItemIDs.BloodStoneLaserSword || pcls.ID == InventoryItemIDs.CopperLaserSword)
			{
				toolTier = 12;
			}
			else
			{
				switch (pcls.Material)
				{
				case ToolMaterialTypes.Wood:
					toolTier = 1;
					break;
				case ToolMaterialTypes.Stone:
					toolTier = 3;
					break;
				case ToolMaterialTypes.Copper:
					toolTier = 4;
					break;
				case ToolMaterialTypes.Iron:
					toolTier = 5;
					break;
				case ToolMaterialTypes.Gold:
					toolTier = 6;
					break;
				case ToolMaterialTypes.Diamond:
					toolTier = 8;
					break;
				case ToolMaterialTypes.BloodStone:
					toolTier = 10;
					break;
				}
			}
			return toolTier;
		}

		private float GetBlockDifficultyModifier(int blockTier)
		{
			float digTime = 5f;
			switch (blockTier)
			{
			case 0:
				digTime = 0.1f;
				break;
			case 1:
				digTime = 0.25f;
				break;
			case 2:
				digTime = 0.5f;
				break;
			case 3:
				digTime = 1f;
				break;
			case 4:
				digTime = 1.5f;
				break;
			case 5:
				digTime = 2f;
				break;
			case 6:
				digTime = 3f;
				break;
			case 7:
				digTime = 4.5f;
				break;
			case 8:
				digTime = 6f;
				break;
			case 9:
				digTime = 8f;
				break;
			case 10:
				digTime = 10f;
				break;
			}
			return digTime;
		}

		private float GetDifficultyRatio(int diff)
		{
			float diffRatio = 1f;
			switch (diff)
			{
			case 0:
				diffRatio = 1f;
				break;
			case 1:
				diffRatio = 0.75f;
				break;
			case 2:
				diffRatio = 0.5f;
				break;
			case 3:
				diffRatio = 0.35f;
				break;
			case 4:
				diffRatio = 0.25f;
				break;
			case 5:
				diffRatio = 0.2f;
				break;
			case 6:
				diffRatio = 0.17f;
				break;
			case 7:
				diffRatio = 0.15f;
				break;
			case 8:
				diffRatio = 0.14f;
				break;
			case 9:
				diffRatio = 0.13f;
				break;
			case 10:
				diffRatio = 0.12f;
				break;
			}
			return diffRatio;
		}

		private float GetAverageDigTimeForToolTier(int toolTier)
		{
			float digTime = 20f;
			switch (toolTier)
			{
			case 0:
				digTime = 16f;
				break;
			case 1:
				digTime = 14f;
				break;
			case 2:
				digTime = 12f;
				break;
			case 3:
				digTime = 10f;
				break;
			case 4:
				digTime = 9f;
				break;
			case 5:
				digTime = 6f;
				break;
			case 6:
				digTime = 4f;
				break;
			case 7:
				digTime = 3f;
				break;
			case 8:
				digTime = 2f;
				break;
			case 9:
				digTime = 1.5f;
				break;
			case 10:
				digTime = 1f;
				break;
			case 12:
				digTime = 0.5f;
				break;
			}
			return digTime;
		}

		public override TimeSpan TimeToDig(BlockTypeEnum blockType)
		{
			PickInventoryItemClass pcls = (PickInventoryItemClass)base.ItemClass;
			int blockTier = this.GetPickaxeBlockTier(blockType);
			int toolTier = this.GetToolTier(pcls);
			if (blockTier > toolTier)
			{
				return base.TimeToDig(blockType);
			}
			float seconds = this.ComputeDigTime(toolTier, blockTier);
			return TimeSpan.FromSeconds((double)seconds);
		}

		private float ComputeDigTime(int toolTier, int blockTier)
		{
			int diff = toolTier - blockTier;
			float seconds = this.GetAverageDigTimeForToolTier(toolTier);
			float digRatio = this.GetBlockDifficultyModifier(blockTier);
			seconds *= digRatio;
			seconds *= this.GetDifficultyRatio(diff);
			if (seconds < 0.01f)
			{
				seconds = 0.01f;
			}
			return seconds;
		}

		public TimeSpan TimeToDigOld(BlockTypeEnum blockType)
		{
			PickInventoryItemClass pcls = (PickInventoryItemClass)base.ItemClass;
			if (pcls.ID == InventoryItemIDs.IronLaserSword || pcls.ID == InventoryItemIDs.GoldLaserSword || pcls.ID == InventoryItemIDs.DiamondLaserSword || pcls.ID == InventoryItemIDs.BloodStoneLaserSword || pcls.ID == InventoryItemIDs.CopperLaserSword)
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
			switch (pcls.Material)
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
