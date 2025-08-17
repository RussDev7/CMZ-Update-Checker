using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class CrashSiteDepositer : Biome
	{
		public CrashSiteDepositer(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = (int)(blender * 10f);
			float num2 = this._noiseFunction.ComputeNoise(0.0046875f * (float)worldX, 0.0046875f * (float)worldZ);
			CrashSiteDepositer.noiseHighestValue = Math.Max(CrashSiteDepositer.noiseHighestValue, num2);
			if (num2 > 0.5f)
			{
				int num3 = (int)((num2 - 0.5f) * 7f * 20f);
				int num4 = 0;
				if (num2 > 0.55f)
				{
					num4 = Math.Min(num3, (int)((num2 - 0.55f) * 10f * 20f));
				}
				Math.Max(20, 66 - num3 - (int)((float)num4 * 1.5f));
				for (int i = 20; i < 126; i++)
				{
					int num5 = i + minY;
					IntVector3 intVector = new IntVector3(worldX, num5, worldZ);
					int num6 = terrain.MakeIndexFromWorldIndexVector(intVector);
					int num7 = terrain._blocks[num6];
					int num8 = i - 1 + minY;
					IntVector3 intVector2 = new IntVector3(worldX, num8, worldZ);
					int num9 = terrain.MakeIndexFromWorldIndexVector(intVector2);
					if (i < 66 - num3 - num4 - 10)
					{
						if (terrain._blocks[num6] != Biome.BloodSToneBlock && (num9 < 0 || terrain._blocks[num9] != Biome.BloodSToneBlock))
						{
							terrain._blocks[num6] = Biome.emptyblock;
						}
					}
					else if (num4 > 0 && i < 66 - num3 + num4)
					{
						terrain._blocks[num6] = Biome.SpaceRockBlock;
					}
					else if (i >= 66 - num3 + num4)
					{
						terrain._blocks[num6] = Biome.emptyblock;
					}
					if (terrain._blocks[num6] == Biome.SpaceRockBlock && i < 66 - num3 + (num4 - 3))
					{
						IntVector3 intVector3 = intVector + new IntVector3(777, 777, 777);
						int num10 = this._slimeNoiseFunction.ComputeNoise(intVector3 / 2);
						int num11 = this._slimeNoiseFunction.ComputeNoise(intVector3);
						num10 += (num11 - 128) / 8;
						if (num10 > 265 - num)
						{
							terrain._blocks[num6] = Biome.SlimeBlock;
						}
					}
				}
			}
		}

		private const float worldScale = 0.0046875f;

		private const int GroundPlane = 66;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));

		private IntNoise _slimeNoiseFunction = new IntNoise(new Random(1));

		private static float noiseHighestValue;
	}
}
