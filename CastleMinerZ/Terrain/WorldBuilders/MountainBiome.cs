using System;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class MountainBiome : Biome
	{
		public MountainBiome(WorldInfo worldInfo)
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
				noise += this._noiseFunction.ComputeNoise(0.021875f * (float)worldX * (float)freq, 0.021875f * (float)worldZ * (float)freq) / (float)freq;
				freq *= 2;
			}
			int groundLimit = adjustedGround + (int)(noise * (float)adjustedHillHeight);
			freq = 1;
			noise = 0f;
			octives = 6;
			Vector2 wv = new Vector2((float)worldX * 0.00625f, (float)worldZ * 0.00625f);
			for (int j = 0; j < octives; j++)
			{
				noise += this._noiseFunction.ComputeNoise(wv * (float)freq) / (float)freq;
				freq *= 2;
			}
			noise = (noise + 1f) / 2f;
			float impulseinput = noise;
			impulseinput -= (float)Math.Floor((double)impulseinput);
			impulseinput -= 0.5f;
			noise = (float)Math.Pow(2.0, (double)(-(double)impulseinput * impulseinput * 1000f));
			groundLimit -= (int)(noise * (float)adjustedHillHeight);
			bool inValley = false;
			if (groundLimit <= 65)
			{
				groundLimit = 65;
				inValley = true;
			}
			noise = this._noiseFunction.ComputeNoise(wv + new Vector2(1000f, 1000f)) * blender;
			groundLimit += (int)(noise * 10f);
			int landBias = (int)(this._noiseFunction.ComputeNoise((float)worldX * 0.5f, (float)worldZ * 0.5f) * 4f);
			int snowLimit = groundLimit - 4;
			int snowAlt = 98;
			if (snowLimit < snowAlt)
			{
				snowLimit = snowAlt;
			}
			snowLimit += landBias;
			if (groundLimit >= 128)
			{
				groundLimit = 127;
			}
			for (int y = 0; y <= groundLimit; y++)
			{
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				terrain._blocks[index] = Biome.rockblock;
				if (inValley || y >= snowLimit)
				{
					terrain._blocks[index] = Biome.snowBlock;
				}
				else
				{
					terrain._blocks[index] = Biome.rockblock;
				}
			}
		}

		private const int BedRockVariance = 3;

		private const int DirtThickness = 3;

		private const int GroundPlane = 86;

		private const int MaxHillHeight = 32;

		private const int MaxValleyDepth = 21;

		private const float worldScale = 0.021875f;

		private const float roadDensity = 0.00625f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
