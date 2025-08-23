using System;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class DecentBiome : Biome
	{
		public DecentBiome(WorldInfo worldInfo)
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
			int groundLimit = (int)MathHelper.Lerp(64f, -16f, blender) + (int)(noise * 16f);
			for (int y = 0; y < 128; y++)
			{
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				if (y <= groundLimit)
				{
					terrain._blocks[index] = Biome.rockblock;
				}
				Vector3 wv = worldPos * 0.0625f * new Vector3(1f, 1f, 1f);
				noise = this._noiseFunction.ComputeNoise(wv);
				noise += this._noiseFunction.ComputeNoise(wv * 2f) / 2f;
				if (noise < blender * 2f - 1f)
				{
					terrain._blocks[index] = Biome.emptyblock;
				}
			}
		}

		private const int DirtThickness = 3;

		private const int GroundPlane = 64;

		private const int MaxHillHeight = 16;

		private const int MaxValleyDepth = 10;

		private const float worldScale = 0.009375f;

		private const float caveDensity = 0.0625f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
