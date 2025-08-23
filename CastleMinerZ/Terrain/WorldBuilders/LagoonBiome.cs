using System;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class LagoonBiome : Biome
	{
		public LagoonBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int SandLevel = 66;
			int adjustedGroundPlane = (int)MathHelper.Lerp(44f, 76f, blender);
			int freq = 1;
			float noise = 0f;
			int octives = 4;
			for (int i = 0; i < octives; i++)
			{
				noise += this._noiseFunction.ComputeNoise(0.009375f * (float)worldX * (float)freq, 0.009375f * (float)worldZ * (float)freq) / (float)freq;
				freq *= 2;
			}
			int groundLimit = adjustedGroundPlane + (int)(noise * 32f);
			this._noiseFunction.ComputeNoise((float)worldX * 0.0625f + 1000f, (float)worldZ * 0.0625f + 1000f);
			bool doSand = false;
			if (groundLimit <= 66)
			{
				doSand = true;
				groundLimit = SandLevel;
			}
			int dirtLimit = groundLimit - 3;
			int landBias = (int)(this._noiseFunction.ComputeNoise((float)worldX * 0.5f, (float)worldZ * 0.5f) * 4f);
			for (int y = 0; y < 128; y++)
			{
				if (y < 100)
				{
					int worldY = y + minY;
					IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
					int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
					Vector3 wv = worldPos * 0.0625f / 2f * new Vector3(1f, 1.5f, 1f) + new Vector3(1000f, 2000f, 1500f);
					noise = this._noiseFunction.ComputeNoise(wv);
					noise += this._noiseFunction.ComputeNoise(wv * 2f) / 2f;
					float overhangBias = -1f + 0.65f * blender;
					if (noise < overhangBias)
					{
						terrain._blocks[index] = Biome.rockblock;
					}
				}
			}
			for (int y2 = 0; y2 < 127; y2++)
			{
				int worldY2 = y2 + minY;
				IntVector3 worldPos2 = new IntVector3(worldX, worldY2, worldZ);
				int index2 = terrain.MakeIndexFromWorldIndexVector(worldPos2);
				int indexAbove = terrain.MakeIndexFromWorldIndexVector(new IntVector3(worldX, worldY2 + 1, worldZ));
				if (terrain._blocks[index2] == Biome.rockblock && terrain._blocks[indexAbove] != Biome.rockblock)
				{
					terrain._blocks[index2] = Biome.grassblock;
				}
			}
			if (groundLimit >= 128)
			{
				groundLimit = 127;
			}
			for (int y3 = 0; y3 <= groundLimit; y3++)
			{
				int worldY3 = y3 + minY;
				IntVector3 worldPos3 = new IntVector3(worldX, worldY3, worldZ);
				int index3 = terrain.MakeIndexFromWorldIndexVector(worldPos3);
				if (y3 >= dirtLimit)
				{
					if (groundLimit + landBias > 85)
					{
						if (groundLimit + landBias > 88)
						{
							terrain._blocks[index3] = Biome.rockblock;
						}
						else
						{
							terrain._blocks[index3] = Biome.dirtblock;
						}
					}
					else if (doSand)
					{
						terrain._blocks[index3] = Biome.sandBlock;
					}
					else if (y3 == groundLimit)
					{
						terrain._blocks[index3] = Biome.grassblock;
					}
					else
					{
						terrain._blocks[index3] = Biome.dirtblock;
					}
				}
				else
				{
					terrain._blocks[index3] = Biome.rockblock;
				}
			}
		}

		private const int BedRockVariance = 3;

		private const int DirtThickness = 3;

		private const int GroundPlane = 76;

		private const int SandLevel = 66;

		private const int MaxHillHeight = 32;

		private const int MaxValleyDepth = 20;

		private const float worldScale = 0.009375f;

		private const float sandLakeDensity = 0.0625f;

		private const float overhangDensity = 0.0625f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
