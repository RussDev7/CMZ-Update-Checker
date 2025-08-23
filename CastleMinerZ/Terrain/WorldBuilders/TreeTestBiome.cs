using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class TreeTestBiome : Biome
	{
		public TreeTestBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			IntVector3 worldPos = new IntVector3(worldX, minY, worldZ);
			int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
			terrain._blocks[index] = Biome.bedrockBlock;
			worldPos = new IntVector3(worldX, minY + 1, worldZ);
			index = terrain.MakeIndexFromWorldIndexVector(worldPos);
			terrain._blocks[index] = Biome.grassblock;
			int goundHeight = 2;
			float treeNoise = this._noiseFunction.ComputeNoise((float)worldX * 0.4375f, (float)worldZ * 0.4375f);
			if (treeNoise > 0.6f)
			{
				float treeVar = 9f * (treeNoise - 0.6f);
				int trunkHeight = 6 + (int)treeVar;
				for (int y = goundHeight; y < goundHeight + trunkHeight; y++)
				{
					int worldY = y + minY;
					worldPos = new IntVector3(worldX, worldY, worldZ);
					index = terrain.MakeIndexFromWorldIndexVector(worldPos);
					terrain._blocks[index] = Biome.LogBlock;
				}
				for (int y2 = goundHeight + trunkHeight; y2 < goundHeight + trunkHeight + 2; y2++)
				{
					int worldY2 = y2 + minY;
					worldPos = new IntVector3(worldX, worldY2, worldZ);
					index = terrain.MakeIndexFromWorldIndexVector(worldPos);
					terrain._blocks[index] = Biome.LeafBlock;
				}
				return;
			}
			if (treeNoise > -0.25f)
			{
				float closestTreeNoise = float.MinValue;
				int treeWidth = 3;
				for (int x = -treeWidth; x < treeWidth; x++)
				{
					for (int z = -treeWidth; z < treeWidth; z++)
					{
						closestTreeNoise = Math.Max(closestTreeNoise, this._noiseFunction.ComputeNoise((float)(worldX + x) * 0.4375f, (float)(worldZ + z) * 0.4375f));
					}
				}
				if (closestTreeNoise > 0.6f)
				{
					float treeVar2 = 9f * (closestTreeNoise - 0.6f);
					int trunkHeight2 = 6 + (int)treeVar2;
					int leafHeight = 1 + (int)((treeNoise + 0.25f) * 4f);
					int leafbase = trunkHeight2 - 2;
					for (int y3 = goundHeight + leafbase; y3 < goundHeight + leafbase + leafHeight; y3++)
					{
						int worldY3 = y3 + minY;
						worldPos = new IntVector3(worldX, worldY3, worldZ);
						index = terrain.MakeIndexFromWorldIndexVector(worldPos);
						terrain._blocks[index] = Biome.LeafBlock;
					}
				}
			}
		}

		private const float treeScale = 0.4375f;

		private const float TreeDescrim = 0.6f;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
