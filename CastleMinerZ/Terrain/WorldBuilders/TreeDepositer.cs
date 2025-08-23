using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class TreeDepositer : Biome
	{
		public TreeDepositer(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			float treeNoise = this._noiseFunction.ComputeNoise((float)worldX * 0.4375f, (float)worldZ * 0.4375f);
			if (treeNoise > 0.6f)
			{
				int groundHeight;
				for (groundHeight = 124; groundHeight > 0; groundHeight--)
				{
					int worldY = groundHeight + minY;
					IntVector3 worldPos = new IntVector3(worldX, worldY, worldZ);
					int index = terrain.MakeIndexFromWorldIndexVector(worldPos);
					if (Block.GetTypeIndex(terrain._blocks[index]) == BlockTypeEnum.Grass)
					{
						break;
					}
				}
				if (groundHeight <= 1)
				{
					return;
				}
				groundHeight++;
				float treeVar = 9f * (treeNoise - 0.6f);
				int trunkHeight = this.TreeHeight + (int)treeVar;
				for (int y = 0; y < trunkHeight; y++)
				{
					int worldY2 = groundHeight + y + minY;
					IntVector3 worldPos2 = new IntVector3(worldX, worldY2, worldZ);
					int index2 = terrain.MakeIndexFromWorldIndexVector(worldPos2);
					BlockTypeEnum existing = Block.GetTypeIndex(terrain._blocks[index2]);
					if (existing != BlockTypeEnum.Empty && existing != BlockTypeEnum.NumberOfBlocks)
					{
						trunkHeight = y;
						break;
					}
					terrain._blocks[index2] = Biome.LogBlock;
				}
				int endHeight = groundHeight + trunkHeight;
				for (int x = -3; x <= 3; x++)
				{
					for (int z = -3; z <= 3; z++)
					{
						for (int y2 = -3; y2 <= 3; y2++)
						{
							IntVector3 worldPos3 = new IntVector3(worldX + x, endHeight + y2 + minY, worldZ + z);
							int index3 = terrain.MakeIndexFromWorldIndexVector(worldPos3);
							BlockTypeEnum existing2 = Block.GetTypeIndex(terrain._blocks[index3]);
							if (existing2 == BlockTypeEnum.Empty || existing2 == BlockTypeEnum.NumberOfBlocks)
							{
								float noise = this._noiseFunction.ComputeNoise(worldPos3 * 0.5f);
								float distBlender = 1f - (float)Math.Sqrt((double)(x * x + y2 * y2 + z * z)) / 3f;
								if (noise + distBlender > 0.25f)
								{
									terrain._blocks[index3] = Biome.LeafBlock;
								}
							}
						}
					}
				}
			}
		}

		private const float treeScale = 0.4375f;

		private const float TreeDescrim = 0.6f;

		public const int TreeWidth = 3;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));

		private int TreeHeight = 5;
	}
}
