using System;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class OreDepositer : Biome
	{
		public OreDepositer(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new IntNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = (int)(blender * 10f);
			for (int i = 0; i < 128; i++)
			{
				int num2 = i + minY;
				IntVector3 intVector = new IntVector3(worldX, num2, worldZ);
				int num3 = terrain.MakeIndexFromWorldIndexVector(intVector);
				if (terrain._blocks[num3] == Biome.rockblock)
				{
					int num4 = this._noiseFunction.ComputeNoise(intVector / 4);
					int num5 = this._noiseFunction.ComputeNoise(intVector);
					num4 += (num5 - 128) / 8;
					if (num4 > 255 - num)
					{
						terrain._blocks[num3] = Biome.coalBlock;
					}
					else if (num4 < num - 5)
					{
						terrain._blocks[num3] = Biome.copperBlock;
					}
					this.GenerateLootBlock(terrain, i, intVector, num3);
					IntVector3 intVector2 = intVector + new IntVector3(1000, 1000, 1000);
					num4 = this._noiseFunction.ComputeNoise(intVector2 / 3);
					num5 = this._noiseFunction.ComputeNoise(intVector2);
					num4 += (num5 - 128) / 8;
					if (num4 > 264 - num)
					{
						terrain._blocks[num3] = Biome.ironBlock;
					}
					else if (num4 < -9 + num && i < 50)
					{
						terrain._blocks[num3] = Biome.goldBlock;
					}
					if (i < 50)
					{
						IntVector3 intVector3 = intVector + new IntVector3(777, 777, 777);
						num4 = this._noiseFunction.ComputeNoise(intVector3 / 2);
						num5 = this._noiseFunction.ComputeNoise(intVector3);
						num4 += (num5 - 128) / 8;
						if (num4 > 266 - num)
						{
							terrain._blocks[num3] = Biome.surfaceLavablock;
						}
						else if (num4 < -11 + num && i < 40)
						{
							terrain._blocks[num3] = Biome.diamondsBlock;
						}
					}
				}
				if (terrain._blocks[num3] == Biome.sandBlock || terrain._blocks[num3] == Biome.snowBlock || terrain._blocks[num3] == Biome.BloodSToneBlock)
				{
					this.GenerateLootBlock(terrain, i, intVector, num3);
				}
			}
		}

		private bool DoesModeAllowLootBlocks(GameModeTypes gameMode)
		{
			return gameMode == GameModeTypes.Creative || gameMode == GameModeTypes.Scavenger || gameMode == GameModeTypes.Exploration;
		}

		private void GenerateLootBlock(BlockTerrain terrain, int worldLevel, IntVector3 worldPos, int index)
		{
			switch (CastleMinerZGame.Instance.GameMode)
			{
			case GameModeTypes.Endurance:
			case GameModeTypes.DragonEndurance:
				break;
			case GameModeTypes.Survival:
				this.GenerateLootBlockSurvivalMode(terrain, worldLevel, worldPos, index);
				break;
			case GameModeTypes.Creative:
				this.GenerateLootBlockScavengerMode(terrain, worldLevel, worldPos, index);
				return;
			case GameModeTypes.Exploration:
				this.GenerateLootBlockSurvivalMode(terrain, worldLevel, worldPos, index);
				return;
			case GameModeTypes.Scavenger:
				this.GenerateLootBlockScavengerMode(terrain, worldLevel, worldPos, index);
				return;
			default:
				return;
			}
		}

		private void GenerateLootBlockSurvivalMode(BlockTerrain terrain, int worldLevel, IntVector3 worldPos, int index)
		{
			IntVector3 intVector = worldPos + new IntVector3(333, 333, 333);
			if ((terrain._blocks[index] == Biome.sandBlock || terrain._blocks[index] == Biome.snowBlock) && worldLevel > 60)
			{
				return;
			}
			int num = this._noiseFunction.ComputeNoise(intVector / 5);
			int num2 = this._noiseFunction.ComputeNoise(intVector);
			int num3 = this._noiseFunction.ComputeNoise(intVector / 2);
			num += (num2 - 128) / 8;
			if (num > 268)
			{
				if (num3 > 249 && (worldLevel < 55 || worldLevel >= 100))
				{
					terrain._blocks[index] = Biome.luckyLootBlock;
					return;
				}
				if (num3 > 145)
				{
					terrain._blocks[index] = Biome.lootBlock;
				}
			}
		}

		private void GenerateLootBlockScavengerMode(BlockTerrain terrain, int worldLevel, IntVector3 worldPos, int index)
		{
			int num = 0;
			IntVector3 intVector = worldPos + new IntVector3(333, 333, 333);
			int num2 = this._noiseFunction.ComputeNoise(intVector / 5);
			int num3 = this._noiseFunction.ComputeNoise(intVector);
			int num4 = this._noiseFunction.ComputeNoise(intVector / 2);
			num2 += (num3 - 128) / 8;
			if ((terrain._blocks[index] == Biome.sandBlock || terrain._blocks[index] == Biome.snowBlock) && worldLevel > 60)
			{
				num = 1;
			}
			if (num2 > 267 + num)
			{
				if (num4 > 250 + num * 3)
				{
					terrain._blocks[index] = Biome.luckyLootBlock;
					return;
				}
				if (num4 > 165)
				{
					terrain._blocks[index] = Biome.lootBlock;
				}
			}
		}

		private IntNoise _noiseFunction = new IntNoise(new Random(1));
	}
}
