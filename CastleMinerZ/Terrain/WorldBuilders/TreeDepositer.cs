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
			float num = this._noiseFunction.ComputeNoise((float)worldX * 0.4375f, (float)worldZ * 0.4375f);
			if (num > 0.6f)
			{
				int i;
				for (i = 124; i > 0; i--)
				{
					int num2 = i + minY;
					IntVector3 intVector = new IntVector3(worldX, num2, worldZ);
					int num3 = terrain.MakeIndexFromWorldIndexVector(intVector);
					if (Block.GetTypeIndex(terrain._blocks[num3]) == BlockTypeEnum.Grass)
					{
						break;
					}
				}
				if (i <= 1)
				{
					return;
				}
				i++;
				float num4 = 9f * (num - 0.6f);
				int num5 = this.TreeHeight + (int)num4;
				for (int j = 0; j < num5; j++)
				{
					int num6 = i + j + minY;
					IntVector3 intVector2 = new IntVector3(worldX, num6, worldZ);
					int num7 = terrain.MakeIndexFromWorldIndexVector(intVector2);
					BlockTypeEnum typeIndex = Block.GetTypeIndex(terrain._blocks[num7]);
					if (typeIndex != BlockTypeEnum.Empty && typeIndex != BlockTypeEnum.NumberOfBlocks)
					{
						num5 = j;
						break;
					}
					terrain._blocks[num7] = Biome.LogBlock;
				}
				int num8 = i + num5;
				for (int k = -3; k <= 3; k++)
				{
					for (int l = -3; l <= 3; l++)
					{
						for (int m = -3; m <= 3; m++)
						{
							IntVector3 intVector3 = new IntVector3(worldX + k, num8 + m + minY, worldZ + l);
							int num9 = terrain.MakeIndexFromWorldIndexVector(intVector3);
							BlockTypeEnum typeIndex2 = Block.GetTypeIndex(terrain._blocks[num9]);
							if (typeIndex2 == BlockTypeEnum.Empty || typeIndex2 == BlockTypeEnum.NumberOfBlocks)
							{
								float num10 = this._noiseFunction.ComputeNoise(intVector3 * 0.5f);
								float num11 = 1f - (float)Math.Sqrt((double)(k * k + m * m + l * l)) / 3f;
								if (num10 + num11 > 0.25f)
								{
									terrain._blocks[num9] = Biome.LeafBlock;
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
