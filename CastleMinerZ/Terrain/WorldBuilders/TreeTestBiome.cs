using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class TreeTestBiome : Biome
	{
		public TreeTestBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			IntVector3 intVector = new IntVector3(worldX, minY, worldZ);
			int num = terrain.MakeIndexFromWorldIndexVector(intVector);
			terrain._blocks[num] = Biome.bedrockBlock;
			intVector = new IntVector3(worldX, minY + 1, worldZ);
			num = terrain.MakeIndexFromWorldIndexVector(intVector);
			terrain._blocks[num] = Biome.grassblock;
			int num2 = 2;
			float num3 = this._noiseFunction.ComputeNoise((float)worldX * 0.4375f, (float)worldZ * 0.4375f);
			if (num3 > 0.6f)
			{
				float num4 = 9f * (num3 - 0.6f);
				int num5 = 6 + (int)num4;
				for (int i = num2; i < num2 + num5; i++)
				{
					int num6 = i + minY;
					intVector = new IntVector3(worldX, num6, worldZ);
					num = terrain.MakeIndexFromWorldIndexVector(intVector);
					terrain._blocks[num] = Biome.LogBlock;
				}
				for (int j = num2 + num5; j < num2 + num5 + 2; j++)
				{
					int num7 = j + minY;
					intVector = new IntVector3(worldX, num7, worldZ);
					num = terrain.MakeIndexFromWorldIndexVector(intVector);
					terrain._blocks[num] = Biome.LeafBlock;
				}
				return;
			}
			if (num3 > -0.25f)
			{
				float num8 = float.MinValue;
				int num9 = 3;
				for (int k = -num9; k < num9; k++)
				{
					for (int l = -num9; l < num9; l++)
					{
						num8 = Math.Max(num8, this._noiseFunction.ComputeNoise((float)(worldX + k) * 0.4375f, (float)(worldZ + l) * 0.4375f));
					}
				}
				if (num8 > 0.6f)
				{
					float num10 = 9f * (num8 - 0.6f);
					int num11 = 6 + (int)num10;
					int num12 = 1 + (int)((num3 + 0.25f) * 4f);
					int num13 = num11 - 2;
					for (int m = num2 + num13; m < num2 + num13 + num12; m++)
					{
						int num14 = m + minY;
						intVector = new IntVector3(worldX, num14, worldZ);
						num = terrain.MakeIndexFromWorldIndexVector(intVector);
						terrain._blocks[num] = Biome.LeafBlock;
					}
				}
			}
		}

		private const float treeScale = 0.4375f;

		private const float TreeDescrim = 0.6f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
