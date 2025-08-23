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
			IntVector3 worldPos = new IntVector3(worldX, minY, worldZ);
			int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
			terrain._blocks[index] = Biome.bedrockBlock;
			worldPos = new IntVector3(worldX, minY + 1, worldZ);
			index = terrain.MakeIndexFromWorldIndexVector(worldPos);
			terrain._blocks[index] = Biome.grassblock;
		}
	}
}
