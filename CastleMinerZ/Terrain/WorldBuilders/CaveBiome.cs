using System;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class CaveBiome : Biome
	{
		public CaveBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		private int GetEnemyBlock(IntVector3 worldPos, float noise)
		{
			int enemyBlockRareOff = Biome.enemyBlockRareOff;
			int enemyBlockOff = Biome.enemyBlockOff;
			int[] midBlockIDs = new int[]
			{
				Biome.enemyBlockRareOff,
				Biome.enemyBlockOff,
				Biome.alienSpawnOff
			};
			int enemyBlockRareOff2 = Biome.enemyBlockRareOff;
			int enemyBlockOff2 = Biome.enemyBlockOff;
			int alienSpawnOff = Biome.alienSpawnOff;
			int hellSpawnOff = Biome.hellSpawnOff;
			int[] blockIDs = midBlockIDs;
			int roll = MathTools.RandomInt(blockIDs.Length);
			return blockIDs[roll];
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			bool lastBlockNotEmpty = true;
			for (int y = 0; y < 128; y++)
			{
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				int existing = terrain._blocks[index];
				if (Biome.emptyblock != existing && Biome.uninitblock != existing && Biome.sandBlock != existing)
				{
					Vector3 wv = worldPos * 0.0625f * new Vector3(1f, 1.5f, 1f);
					float noise = this._noiseFunction.ComputeNoise(wv);
					noise += this._noiseFunction.ComputeNoise(wv * 2f) / 2f;
					if (noise < -0.35f)
					{
						this.emptyBlockCount++;
						terrain._blocks[index] = Biome.emptyblock;
						if (lastBlockNotEmpty && terrain._blocks[index] != Biome.dirtblock && terrain._blocks[index] != Biome.grassblock)
						{
							if (this.emptyBlockCount % this.lootBlockModifier == 0)
							{
								terrain._blocks[index] = Biome.lootBlock;
							}
							if (this.emptyBlockCount % this.enemyBlockModifier == 0)
							{
								terrain._blocks[index] = this.GetEnemyBlock(worldPos, noise);
							}
							if (this.emptyBlockCount % this.luckyLootBlockModifier == 0)
							{
								terrain._blocks[index] = Biome.luckyLootBlock;
							}
							lastBlockNotEmpty = false;
						}
					}
				}
				else
				{
					lastBlockNotEmpty = true;
				}
			}
		}

		public void SetLootModifiersByGameMode()
		{
			switch (CastleMinerZGame.Instance.GameMode)
			{
			case GameModeTypes.Endurance:
				this.lootBlockModifier = 1000000;
				this.luckyLootBlockModifier = 1000000;
				return;
			case GameModeTypes.Survival:
				this.lootBlockModifier = 20000;
				this.luckyLootBlockModifier = 35000;
				this.enemyBlockModifier = 2000;
				return;
			case GameModeTypes.DragonEndurance:
				this.lootBlockModifier = 1000000;
				this.luckyLootBlockModifier = 1000000;
				return;
			case GameModeTypes.Creative:
				this.lootBlockModifier = 1000000;
				this.luckyLootBlockModifier = 1000000;
				this.enemyBlockModifier = 2000;
				return;
			case GameModeTypes.Exploration:
				this.lootBlockModifier = 2100;
				this.luckyLootBlockModifier = 7000;
				this.enemyBlockModifier = 1000;
				return;
			case GameModeTypes.Scavenger:
				this.lootBlockModifier = 150;
				this.luckyLootBlockModifier = 1000;
				this.enemyBlockModifier = 200;
				return;
			default:
				return;
			}
		}

		private const float caveDensity = 0.0625f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));

		private int lootBlockModifier = 5000;

		private int luckyLootBlockModifier = 10001;

		private int enemyBlockModifier = 2100;

		private int emptyBlockCount;
	}
}
