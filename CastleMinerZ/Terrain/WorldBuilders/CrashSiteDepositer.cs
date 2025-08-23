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
			int intblend = (int)(blender * 10f);
			float noise = this._noiseFunction.ComputeNoise(0.0046875f * (float)worldX, 0.0046875f * (float)worldZ);
			CrashSiteDepositer.noiseHighestValue = Math.Max(CrashSiteDepositer.noiseHighestValue, noise);
			if (noise > 0.5f)
			{
				int craterDepth = (int)((noise - 0.5f) * 7f * 20f);
				int moundHeight = 0;
				if (noise > 0.55f)
				{
					moundHeight = Math.Min(craterDepth, (int)((noise - 0.55f) * 10f * 20f));
				}
				Math.Max(20, 66 - craterDepth - (int)((float)moundHeight * 1.5f));
				for (int y = 20; y < 126; y++)
				{
					int worldY = y + minY;
					IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
					int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
					int num = terrain._blocks[index];
					int blockBelowWorldY = y - 1 + minY;
					IntVector3 blockBelowWorldPos = new IntVector3(worldX, blockBelowWorldY, worldZ);
					int blockBelowIndex = terrain.MakeIndexFromWorldIndexVector(blockBelowWorldPos);
					if (y < 66 - craterDepth - moundHeight - 10)
					{
						if (terrain._blocks[index] != Biome.BloodSToneBlock && (blockBelowIndex < 0 || terrain._blocks[blockBelowIndex] != Biome.BloodSToneBlock))
						{
							terrain._blocks[index] = Biome.emptyblock;
						}
					}
					else if (moundHeight > 0 && y < 66 - craterDepth + moundHeight)
					{
						terrain._blocks[index] = Biome.SpaceRockBlock;
					}
					else if (y >= 66 - craterDepth + moundHeight)
					{
						terrain._blocks[index] = Biome.emptyblock;
					}
					if (terrain._blocks[index] == Biome.SpaceRockBlock && y < 66 - craterDepth + (moundHeight - 3))
					{
						IntVector3 diapos = worldPos + new IntVector3(777, 777, 777);
						int slimeNoise = this._slimeNoiseFunction.ComputeNoise(diapos / 2);
						int slimeNoise2 = this._slimeNoiseFunction.ComputeNoise(diapos);
						slimeNoise += (slimeNoise2 - 128) / 8;
						if (slimeNoise > 265 - intblend)
						{
							terrain._blocks[index] = Biome.SlimeBlock;
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
