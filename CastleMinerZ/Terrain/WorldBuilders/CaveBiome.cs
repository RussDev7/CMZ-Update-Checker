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
			int[] array = new int[]
			{
				Biome.enemyBlockRareOff,
				Biome.enemyBlockOff,
				Biome.alienSpawnOff
			};
			int enemyBlockRareOff2 = Biome.enemyBlockRareOff;
			int enemyBlockOff2 = Biome.enemyBlockOff;
			int alienSpawnOff = Biome.alienSpawnOff;
			int hellSpawnOff = Biome.hellSpawnOff;
			int[] array2 = array;
			int num = MathTools.RandomInt(array2.Length);
			return array2[num];
		}

		private int GetEnemyBlockOverride(int blockID, IntVector3 worldPos, float noise)
		{
			float num = 0.5f;
			float num2 = 0f;
			float num3 = 0.35f;
			if (noise < num3 * num)
			{
				blockID = Biome.hellSpawnOff;
			}
			else if (noise < num3 * num2)
			{
				blockID = Biome.alienSpawnOff;
			}
			return blockID;
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			bool flag = true;
			for (int i = 0; i < 128; i++)
			{
				int num = i + minY;
				IntVector3 intVector = new IntVector3(worldX, num, worldZ);
				int num2 = terrain.MakeIndexFromWorldIndexVector(intVector);
				int num3 = terrain._blocks[num2];
				if (Biome.emptyblock != num3 && Biome.uninitblock != num3 && Biome.sandBlock != num3)
				{
					Vector3 vector = intVector * 0.0625f * new Vector3(1f, 1.5f, 1f);
					float num4 = this._noiseFunction.ComputeNoise(vector);
					num4 += this._noiseFunction.ComputeNoise(vector * 2f) / 2f;
					if (num4 < -0.35f)
					{
						this.emptyBlockCount++;
						terrain._blocks[num2] = Biome.emptyblock;
						if (flag && terrain._blocks[num2] != Biome.dirtblock && terrain._blocks[num2] != Biome.grassblock)
						{
							if (this.emptyBlockCount % this.lootBlockModifier == 0)
							{
								terrain._blocks[num2] = Biome.lootBlock;
							}
							if (this.emptyBlockCount % this.enemyBlockModifier == 0)
							{
								terrain._blocks[num2] = this.GetEnemyBlock(intVector, num4);
							}
							if (this.emptyBlockCount % this.luckyLootBlockModifier == 0)
							{
								terrain._blocks[num2] = Biome.luckyLootBlock;
							}
							flag = false;
						}
					}
				}
				else
				{
					flag = true;
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
