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
			int num = (int)MathHelper.Lerp(0f, 32f, blender);
			int num2 = 1;
			float num3 = 0f;
			int num4 = 4;
			for (int i = 0; i < num4; i++)
			{
				num3 += this._noiseFunction.ComputeNoise(0.03125f * (float)worldX * (float)num2 + 1000f, 0.03125f * (float)worldZ * (float)num2 + 1000f) / (float)num2;
				num2 *= 2;
			}
			num3 += 1f;
			int num5 = num - (int)(num3 * 4f);
			for (int j = 0; j <= num5; j++)
			{
				int num6 = j + minY;
				IntVector3 intVector = new IntVector3(worldX, num6, worldZ);
				int num7 = terrain.MakeIndexFromWorldIndexVector(intVector);
				if (j < num5)
				{
					terrain._blocks[num7] = Biome.emptyblock;
				}
				else if (j == num5 && terrain._blocks[num7] == Biome.rockblock)
				{
					terrain._blocks[num7] = Biome.BloodSToneBlock;
				}
			}
		}

		private const int HellHeight = 32;

		private const int MaxHillHeight = 32;

		private const float worldScale = 0.03125f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
