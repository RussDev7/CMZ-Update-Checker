using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	internal class DesertBiome : Biome
	{
		public DesertBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = 1;
			float num2 = 0f;
			int num3 = 4;
			for (int i = 0; i < num3; i++)
			{
				num2 += this._noiseFunction.ComputeNoise(0.009375f * (float)worldX * (float)num, 0.009375f * (float)worldZ * (float)num) / (float)num;
				num *= 2;
			}
			int num4 = 66 + (int)(num2 * 16f);
			if (num4 <= 66)
			{
				num4 = 66;
			}
			int num5 = num4 - 3;
			for (int j = 0; j <= num4; j++)
			{
				if (terrain._resetRequested)
				{
					return;
				}
				int num6 = j + minY;
				IntVector3 intVector = new IntVector3(worldX, num6, worldZ);
				int num7 = terrain.MakeIndexFromWorldIndexVector(intVector);
				if (j <= num5)
				{
					terrain._blocks[num7] = Biome.rockblock;
				}
				else
				{
					terrain._blocks[num7] = Biome.sandBlock;
				}
			}
		}

		private const int BedRockVariance = 3;

		private const int DirtThickness = 3;

		private const int GroundPlane = 66;

		private const int MaxHillHeight = 16;

		private const int MaxValleyDepth = 0;

		private const float worldScale = 0.009375f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
