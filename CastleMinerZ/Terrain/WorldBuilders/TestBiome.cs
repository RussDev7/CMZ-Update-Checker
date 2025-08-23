using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class TestBiome : Biome
	{
		public TestBiome(WorldInfo worldInfo)
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
			int groundLimit = 64 + (int)(noise * 32f);
			bool inValley = false;
			if (groundLimit <= 44)
			{
				groundLimit = 44;
				inValley = true;
			}
			int dirtLimit = groundLimit - 3;
			noise = this._noiseFunction.ComputeNoise((float)worldX * 0.5f, (float)worldZ * 0.5f);
			noise = (noise + 1f) / 2f;
			int bedRockLevel = 1 + (int)(noise * 3f);
			for (int y = 0; y < 128; y++)
			{
				if (terrain._resetRequested)
				{
					return;
				}
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				if (y <= groundLimit)
				{
					if (y < bedRockLevel)
					{
						terrain._blocks[index] = Biome.bedrockBlock;
					}
					else
					{
						terrain._blocks[index] = Biome.rockblock;
						if (y >= dirtLimit)
						{
							if (inValley)
							{
								terrain._blocks[index] = Biome.sandBlock;
							}
							else if (y == groundLimit)
							{
								terrain._blocks[index] = Biome.grassblock;
							}
							else
							{
								terrain._blocks[index] = Biome.dirtblock;
							}
						}
					}
				}
			}
		}

		private const int BedRockVariance = 3;

		private const int DirtThickness = 3;

		private const int GroundPlane = 64;

		private const int MaxHillHeight = 32;

		private const int MaxValleyDepth = 20;

		private const float worldScale = 0.009375f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
