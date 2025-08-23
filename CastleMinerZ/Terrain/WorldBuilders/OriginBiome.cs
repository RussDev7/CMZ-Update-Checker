using System;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class OriginBiome : Biome
	{
		public OriginBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int distSq = worldX * worldX + worldZ * worldZ;
			if (distSq < 15625)
			{
				int dist = (int)Math.Round(Math.Sqrt((double)distSq));
				if (dist <= 4)
				{
					int reduction = dist * 8;
					int height = 125 - reduction;
					for (int y = 0; y < height; y++)
					{
						int worldY = y + minY;
						IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
						int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
						terrain._blocks[index] = Biome.bedrockBlock;
						if (y == height - 1 && dist < 2)
						{
							terrain._blocks[index] = Biome.fixedLanternblock;
						}
					}
				}
			}
		}

		public const int MaxHeight = 125;
	}
}
