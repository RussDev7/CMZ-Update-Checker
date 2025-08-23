using System;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class ClassicBiome : Biome
	{
		public ClassicBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int adjustedHillHeight = (int)MathHelper.Lerp(0f, 32f, blender);
			int adjustedGround = (int)MathHelper.Lerp(32f, 86f, blender);
			int freq = 1;
			float noise = 0f;
			int octives = 4;
			for (int i = 0; i < octives; i++)
			{
				noise += this._noiseFunction.ComputeNoise(0.009375f * (float)worldX * (float)freq, 0.009375f * (float)worldZ * (float)freq) / (float)freq;
				freq *= 2;
			}
			int groundLimit = adjustedGround + (int)(noise * (float)adjustedHillHeight);
			bool inValley = false;
			if (groundLimit <= 66)
			{
				groundLimit = 66;
				inValley = true;
			}
			if (groundLimit >= 128)
			{
				groundLimit = 127;
			}
			int dirtLimit = groundLimit - 3;
			noise = this._noiseFunction.ComputeNoise((float)worldX * 0.5f, (float)worldZ * 0.5f);
			int landBias = (int)(noise * 4f);
			for (int y = 0; y <= groundLimit; y++)
			{
				if (terrain._resetRequested)
				{
					return;
				}
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				terrain._blocks[index] = Biome.rockblock;
				if (y >= dirtLimit)
				{
					if (groundLimit + landBias > 95)
					{
						if (groundLimit + landBias > 98)
						{
							if (groundLimit + landBias > 108)
							{
								terrain._blocks[index] = Biome.snowBlock;
							}
							else
							{
								terrain._blocks[index] = Biome.rockblock;
							}
						}
						else
						{
							terrain._blocks[index] = Biome.dirtblock;
						}
					}
					else if (inValley)
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

		private const int BedRockVariance = 3;

		private const int DirtThickness = 3;

		private const int CaveEnd = 118;

		private const int GroundPlane = 86;

		private const int MaxHillHeight = 32;

		private const int MaxValleyDepth = 20;

		private const float worldScale = 0.009375f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
