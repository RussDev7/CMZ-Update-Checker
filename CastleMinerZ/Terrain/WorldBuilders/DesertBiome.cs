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
			int freq = 1;
			float noise = 0f;
			int octives = 4;
			for (int i = 0; i < octives; i++)
			{
				noise += this._noiseFunction.ComputeNoise(0.009375f * (float)worldX * (float)freq, 0.009375f * (float)worldZ * (float)freq) / (float)freq;
				freq *= 2;
			}
			int groundLimit = 66 + (int)(noise * 16f);
			if (groundLimit <= 66)
			{
				groundLimit = 66;
			}
			int dirtLimit = groundLimit - 3;
			for (int y = 0; y <= groundLimit; y++)
			{
				if (terrain._resetRequested)
				{
					return;
				}
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				if (y <= dirtLimit)
				{
					terrain._blocks[index] = Biome.rockblock;
				}
				else
				{
					terrain._blocks[index] = Biome.sandBlock;
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
