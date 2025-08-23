using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	internal class ArcticBiome : Biome
	{
		public ArcticBiome(WorldInfo worldInfo)
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
			int groundLimit = 64 + (int)(noise * 16f);
			bool inValley = false;
			if (groundLimit <= 54)
			{
				groundLimit = 54;
				inValley = true;
			}
			int dirtLimit = groundLimit - 3;
			for (int y = 0; y < 128; y++)
			{
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				if (y == groundLimit)
				{
					if (inValley)
					{
						terrain._blocks[index] = Biome.iceBlock;
					}
					else
					{
						terrain._blocks[index] = Biome.snowBlock;
					}
				}
				else if (y == groundLimit - 1)
				{
					terrain._blocks[index] = Biome.iceBlock;
				}
				else if (y <= groundLimit)
				{
					terrain._blocks[index] = Biome.rockblock;
				}
				else if (y <= dirtLimit)
				{
					terrain._blocks[index] = Biome.snowBlock;
				}
			}
		}

		private const int DirtThickness = 3;

		private const int GroundPlane = 64;

		private const int MaxHillHeight = 16;

		private const int MaxValleyDepth = 10;

		private const float worldScale = 0.009375f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
