using System;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	internal class CostalBiome : Biome
	{
		public CostalBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
			this.WaterDepth = 36f;
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float terrainblender)
		{
			terrain.WaterLevel = -31.5f;
			int freq = 1;
			float noise = 0f;
			int octives = 4;
			for (int i = 0; i < octives; i++)
			{
				noise += this._noiseFunction.ComputeNoise(0.009375f * (float)worldX * (float)freq, 0.009375f * (float)worldZ * (float)freq) / (float)freq;
				freq *= 2;
			}
			noise += this._noiseFunction.ComputeNoise(0.00234375f * (float)worldX, 0.00234375f * (float)worldZ);
			noise += this._noiseFunction.ComputeNoise(0.0046875f * (float)worldX, 0.0046875f * (float)worldZ) / 2f;
			bool isSand = false;
			float landMeters = 32f + noise * 32f;
			float beachWidth = 8f;
			if (landMeters <= 32f + beachWidth && landMeters >= 32f - beachWidth)
			{
				float adjustedHeight = (landMeters - 32f) / 4f + 32f;
				float blender = Math.Abs(landMeters - 32f) / beachWidth;
				if (blender < 0.75f)
				{
					isSand = true;
					blender = 0f;
				}
				else
				{
					blender -= 0.75f;
					blender *= 4f;
				}
				landMeters = landMeters * blender + adjustedHeight * (1f - blender);
			}
			int groundLimit = (int)landMeters;
			if (groundLimit < 0)
			{
				groundLimit = 0;
			}
			int dirtLimit = groundLimit - 3;
			noise = this._noiseFunction.ComputeNoise((float)worldX * 0.5f, (float)worldZ * 0.5f);
			int landBias = (int)(noise * 4f);
			noise = (noise + 1f) / 2f;
			int bedRockLevel = 1 + (int)(noise * 3f);
			noise = this._noiseFunction.ComputeNoise((float)worldX * 0.0625f, (float)worldZ * 0.0625f) * 10f;
			int lavaLevel = (int)noise;
			if (lavaLevel > 1)
			{
			}
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
							if (groundLimit + landBias > 41)
							{
								if (groundLimit + landBias > 44)
								{
									if (groundLimit + landBias > 54)
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
							else if (isSand)
							{
								terrain._blocks[index] = Biome.sandBlock;
							}
							else if (y == groundLimit)
							{
								if ((float)y < 32f)
								{
									terrain._blocks[index] = Biome.dirtblock;
								}
								else
								{
									terrain._blocks[index] = Biome.grassblock;
								}
							}
							else
							{
								terrain._blocks[index] = Biome.dirtblock;
							}
						}
						else
						{
							if (y < 32)
							{
								if (y < 16)
								{
									noise = this._noiseFunction.ComputeNoise(0.375f * (float)(worldX + 1750), 0.375f * (float)(worldY + 1750), 0.375f * (float)(worldZ + 1750));
									if (noise > 0.75f)
									{
										terrain._blocks[index] = Biome.diamondsBlock;
									}
								}
								noise = this._noiseFunction.ComputeNoise(0.25f * (float)(worldX + 777), 0.25f * (float)(worldY + 777), 0.25f * (float)(worldZ + 777));
								if (noise < -0.75f)
								{
									terrain._blocks[index] = Biome.goldBlock;
								}
								noise = this._noiseFunction.ComputeNoise(0.25f * (float)(worldX + 5432), 0.25f * (float)(worldY + 5432), 0.25f * (float)(worldZ + 5432));
								if (noise > 0.8f)
								{
									terrain._blocks[index] = Biome.surfaceLavablock;
								}
							}
							noise = this._noiseFunction.ComputeNoise(0.25f * (float)(worldX + 250), 0.25f * (float)(worldY + 250), 0.25f * (float)(worldZ + 250));
							if (noise > 0.74f)
							{
								terrain._blocks[index] = Biome.ironBlock;
							}
							noise = this._noiseFunction.ComputeNoise(0.3125f * (float)(worldX + 1000), 0.3125f * (float)(worldY + 1000), 0.3125f * (float)(worldZ + 1000));
							if (noise < -0.5f)
							{
								terrain._blocks[index] = Biome.coalBlock;
							}
						}
						if (y > 4 && y < 64)
						{
							Vector3 wv = worldPos * 0.0625f * new Vector3(1f, 1.5f, 1f);
							noise = this._noiseFunction.ComputeNoise(wv);
							noise += this._noiseFunction.ComputeNoise(wv * 2f) / 2f;
							if (noise < -0.35f)
							{
								terrain._blocks[index] = Biome.emptyblock;
							}
						}
					}
				}
			}
		}

		private const int BedRockVariance = 3;

		private const int DirtThickness = 3;

		private const int CaveStart = 4;

		private const int CaveEnd = 64;

		private const int GroundPlane = 32;

		private const int MaxHillHeight = 32;

		private const int MaxValleyDepth = 20;

		private const float worldScale = 0.009375f;

		private const float caveDensity = 0.0625f;

		private const float coalDensity = 0.3125f;

		private const float ironDensity = 0.25f;

		private const float goldDensity = 0.25f;

		private const float diamondDensity = 0.375f;

		private const float lavaLakeDensity = 0.0625f;

		private const float lavaSpecDensity = 0.25f;

		private const float waterLevel = 32f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
