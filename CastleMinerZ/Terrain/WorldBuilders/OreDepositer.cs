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
			int intblend = (int)(blender * 10f);
			for (int y = 0; y < 128; y++)
			{
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				if (terrain._blocks[index] == Biome.rockblock)
				{
					int noise = this._noiseFunction.ComputeNoise(worldPos / 4);
					int noise2 = this._noiseFunction.ComputeNoise(worldPos);
					noise += (noise2 - 128) / 8;
					if (noise > 255 - intblend)
					{
						terrain._blocks[index] = Biome.coalBlock;
					}
					else if (noise < intblend - 5)
					{
						terrain._blocks[index] = Biome.copperBlock;
					}
					this.GenerateLootBlock(terrain, y, worldPos, index);
					IntVector3 ironPos = worldPos + new IntVector3(1000, 1000, 1000);
					noise = this._noiseFunction.ComputeNoise(ironPos / 3);
					noise2 = this._noiseFunction.ComputeNoise(ironPos);
					noise += (noise2 - 128) / 8;
					if (noise > 264 - intblend)
					{
						terrain._blocks[index] = Biome.ironBlock;
					}
					else if (noise < -9 + intblend && y < 50)
					{
						terrain._blocks[index] = Biome.goldBlock;
					}
					if (y < 50)
					{
						IntVector3 diapos = worldPos + new IntVector3(777, 777, 777);
						noise = this._noiseFunction.ComputeNoise(diapos / 2);
						noise2 = this._noiseFunction.ComputeNoise(diapos);
						noise += (noise2 - 128) / 8;
						if (noise > 266 - intblend)
						{
							terrain._blocks[index] = Biome.surfaceLavablock;
						}
						else if (noise < -11 + intblend && y < 40)
						{
							terrain._blocks[index] = Biome.diamondsBlock;
						}
					}
				}
				if (terrain._blocks[index] == Biome.sandBlock || terrain._blocks[index] == Biome.snowBlock || terrain._blocks[index] == Biome.BloodSToneBlock)
				{
					this.GenerateLootBlock(terrain, y, worldPos, index);
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
			IntVector3 lootPos = worldPos + new IntVector3(333, 333, 333);
			if ((terrain._blocks[index] == Biome.sandBlock || terrain._blocks[index] == Biome.snowBlock) && worldLevel > 60)
			{
				return;
			}
			int noise = this._noiseFunction.ComputeNoise(lootPos / 5);
			int noise2 = this._noiseFunction.ComputeNoise(lootPos);
			int noise3 = this._noiseFunction.ComputeNoise(lootPos / 2);
			noise += (noise2 - 128) / 8;
			if (noise > 268)
			{
				if (noise3 > 249 && (worldLevel < 55 || worldLevel >= 100))
				{
					terrain._blocks[index] = Biome.luckyLootBlock;
					return;
				}
				if (noise3 > 145)
				{
					terrain._blocks[index] = Biome.lootBlock;
				}
			}
		}

		private void GenerateLootBlockScavengerMode(BlockTerrain terrain, int worldLevel, IntVector3 worldPos, int index)
		{
			int noiseTargetMod = 0;
			IntVector3 lootPos = worldPos + new IntVector3(333, 333, 333);
			int noise = this._noiseFunction.ComputeNoise(lootPos / 5);
			int noise2 = this._noiseFunction.ComputeNoise(lootPos);
			int noise3 = this._noiseFunction.ComputeNoise(lootPos / 2);
			noise += (noise2 - 128) / 8;
			if ((terrain._blocks[index] == Biome.sandBlock || terrain._blocks[index] == Biome.snowBlock) && worldLevel > 60)
			{
				noiseTargetMod = 1;
			}
			if (noise > 267 + noiseTargetMod)
			{
				if (noise3 > 250 + noiseTargetMod * 3)
				{
					terrain._blocks[index] = Biome.luckyLootBlock;
					return;
				}
				if (noise3 > 165)
				{
					terrain._blocks[index] = Biome.lootBlock;
				}
			}
		}

		private IntNoise _noiseFunction = new IntNoise(new Random(1));
	}
}
