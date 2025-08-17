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
			int num = 66;
			int num2 = (int)MathHelper.Lerp(44f, 76f, blender);
			int num3 = 1;
			float num4 = 0f;
			int num5 = 4;
			for (int i = 0; i < num5; i++)
			{
				num4 += this._noiseFunction.ComputeNoise(0.009375f * (float)worldX * (float)num3, 0.009375f * (float)worldZ * (float)num3) / (float)num3;
				num3 *= 2;
			}
			int num6 = num2 + (int)(num4 * 32f);
			this._noiseFunction.ComputeNoise((float)worldX * 0.0625f + 1000f, (float)worldZ * 0.0625f + 1000f);
			bool flag = false;
			if (num6 <= 66)
			{
				flag = true;
				num6 = num;
			}
			int num7 = num6 - 3;
			int num8 = (int)(this._noiseFunction.ComputeNoise((float)worldX * 0.5f, (float)worldZ * 0.5f) * 4f);
			for (int j = 0; j < 128; j++)
			{
				if (j < 100)
				{
					int num9 = j + minY;
					IntVector3 intVector = new IntVector3(worldX, num9, worldZ);
					int num10 = terrain.MakeIndexFromWorldIndexVector(intVector);
					Vector3 vector = intVector * 0.0625f / 2f * new Vector3(1f, 1.5f, 1f) + new Vector3(1000f, 2000f, 1500f);
					num4 = this._noiseFunction.ComputeNoise(vector);
					num4 += this._noiseFunction.ComputeNoise(vector * 2f) / 2f;
					float num11 = -1f + 0.65f * blender;
					if (num4 < num11)
					{
						terrain._blocks[num10] = Biome.rockblock;
					}
				}
			}
			for (int k = 0; k < 127; k++)
			{
				int num12 = k + minY;
				IntVector3 intVector2 = new IntVector3(worldX, num12, worldZ);
				int num13 = terrain.MakeIndexFromWorldIndexVector(intVector2);
				int num14 = terrain.MakeIndexFromWorldIndexVector(new IntVector3(worldX, num12 + 1, worldZ));
				if (terrain._blocks[num13] == Biome.rockblock && terrain._blocks[num14] != Biome.rockblock)
				{
					terrain._blocks[num13] = Biome.grassblock;
				}
			}
			if (num6 >= 128)
			{
				num6 = 127;
			}
			for (int l = 0; l <= num6; l++)
			{
				int num15 = l + minY;
				IntVector3 intVector3 = new IntVector3(worldX, num15, worldZ);
				int num16 = terrain.MakeIndexFromWorldIndexVector(intVector3);
				if (l >= num7)
				{
					if (num6 + num8 > 85)
					{
						if (num6 + num8 > 88)
						{
							terrain._blocks[num16] = Biome.rockblock;
						}
						else
						{
							terrain._blocks[num16] = Biome.dirtblock;
						}
					}
					else if (flag)
					{
						terrain._blocks[num16] = Biome.sandBlock;
					}
					else if (l == num6)
					{
						terrain._blocks[num16] = Biome.grassblock;
					}
					else
					{
						terrain._blocks[num16] = Biome.dirtblock;
					}
				}
				else
				{
					terrain._blocks[num16] = Biome.rockblock;
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
