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
			int num = worldX * worldX + worldZ * worldZ;
			if (num < 15625)
			{
				int num2 = (int)Math.Round(Math.Sqrt((double)num));
				if (num2 <= 4)
				{
					int num3 = num2 * 8;
					int num4 = 125 - num3;
					for (int i = 0; i < num4; i++)
					{
						int num5 = i + minY;
						IntVector3 intVector = new IntVector3(worldX, num5, worldZ);
						int num6 = terrain.MakeIndexFromWorldIndexVector(intVector);
						terrain._blocks[num6] = Biome.bedrockBlock;
						if (i == num4 - 1 && num2 < 2)
						{
							terrain._blocks[num6] = Biome.fixedLanternblock;
						}
					}
				}
			}
		}

		public const int MaxHeight = 125;
	}
}
