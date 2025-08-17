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
			int num = (int)MathHelper.Lerp(66f, 4f, blender);
			int num2 = num - 3;
			for (int i = 0; i < num; i++)
			{
				if (terrain._resetRequested)
				{
					return;
				}
				int num3 = i + minY;
				IntVector3 intVector = new IntVector3(worldX, num3, worldZ);
				int num4 = terrain.MakeIndexFromWorldIndexVector(intVector);
				if (i <= num2)
				{
					terrain._blocks[num4] = Biome.bedrockBlock;
				}
				else if (i <= num)
				{
					terrain._blocks[num4] = Biome.sandBlock;
				}
			}
		}

		private const int GroundPlane = 66;

		private const int SandThickness = 3;
	}
}
