using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class BedrockDepositer : Biome
	{
		public BedrockDepositer(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new IntNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = this._noiseFunction.ComputeNoise(worldX, worldZ);
			num = 1 + num * 3 / 256;
			int num2 = num;
			for (int i = 0; i < num2; i++)
			{
				int num3 = i + minY;
				IntVector3 intVector = new IntVector3(worldX, num3, worldZ);
				int num4 = terrain.MakeIndexFromWorldIndexVector(intVector);
				terrain._blocks[num4] = Biome.bedrockBlock;
			}
		}

		private const int BedRockVariance = 3;

		private IntNoise _noiseFunction = new IntNoise(new Random(1));
	}
}
