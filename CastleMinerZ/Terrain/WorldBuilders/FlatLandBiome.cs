using System;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	internal class FlatLandBiome : Biome
	{
		public FlatLandBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			IntVector3 intVector = new IntVector3(worldX, minY, worldZ);
			int num = terrain.MakeIndexFromWorldIndexVector(intVector);
			terrain._blocks[num] = Biome.bedrockBlock;
			intVector = new IntVector3(worldX, minY + 1, worldZ);
			num = terrain.MakeIndexFromWorldIndexVector(intVector);
			terrain._blocks[num] = Biome.grassblock;
		}
	}
}
