using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class OceanBiome : Biome
	{
		public OceanBiome(WorldInfo winfo)
			: base(winfo)
		{
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int groundLimit = (int)MathHelper.Lerp(66f, 4f, blender);
			int sandLimit = groundLimit - 3;
			for (int y = 0; y < groundLimit; y++)
			{
				if (terrain._resetRequested)
				{
					return;
				}
				int worldY = y + minY;
				IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
				int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
				if (y <= sandLimit)
				{
					terrain._blocks[index] = Biome.bedrockBlock;
				}
				else if (y <= groundLimit)
				{
					terrain._blocks[index] = Biome.sandBlock;
				}
			}
		}

		private const int GroundPlane = 66;

		private const int SandThickness = 3;
	}
}
