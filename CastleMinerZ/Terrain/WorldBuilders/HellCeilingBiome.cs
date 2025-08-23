using System;
using DNA.Drawing.Noise;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class HellCeilingBiome : Biome
	{
		public HellCeilingBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int adjustedHellHeight = (int)MathHelper.Lerp(0f, 32f, blender);
			int freq = 1;
			float noise = 0f;
			int octives = 4;
			for (int i = 0; i < octives; i++)
			{
				noise += this._noiseFunction.ComputeNoise(0.03125f * (float)worldX * (float)freq + 1000f, 0.03125f * (float)worldZ * (float)freq + 1000f) / (float)freq;
				freq *= 2;
			}
			noise += 1f;
			int ceilingHeight = adjustedHellHeight - (int)(noise * 4f);
			for (int y = 0; y <= ceilingHeight; y++)
			{
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				if (y < ceilingHeight)
				{
					terrain._blocks[index] = Biome.emptyblock;
				}
				else if (y == ceilingHeight && terrain._blocks[index] == Biome.rockblock)
				{
					terrain._blocks[index] = Biome.BloodSToneBlock;
				}
			}
		}

		private const int HellHeight = 32;

		private const int MaxHillHeight = 32;

		private const float worldScale = 0.03125f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
