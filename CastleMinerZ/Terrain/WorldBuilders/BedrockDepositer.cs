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
			int noise = this._noiseFunction.ComputeNoise(worldX, worldZ);
			noise = 1 + noise * 3 / 256;
			int bedRockLevel = noise;
			for (int y = 0; y < bedRockLevel; y++)
			{
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				terrain._blocks[index] = Biome.bedrockBlock;
			}
		}

		private const int BedRockVariance = 3;

		private IntNoise _noiseFunction = new IntNoise(new Random(1));
	}
}
