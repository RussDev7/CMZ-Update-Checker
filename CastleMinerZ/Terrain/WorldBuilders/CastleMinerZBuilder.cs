using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	internal class CastleMinerZBuilder : WorldBuilder
	{
		public CastleMinerZBuilder(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this.ocean = new OceanBiome(worldInfo);
			this.classic = new ClassicBiome(worldInfo);
			this.dessert = new DesertBiome(worldInfo);
			this.lagoon = new LagoonBiome(worldInfo);
			this.mountains = new MountainBiome(worldInfo);
			this.arctic = new ArcticBiome(worldInfo);
			this.oreDepositor = new OreDepositer(worldInfo);
			this.bedrockDepositor = new BedrockDepositer(worldInfo);
			this.decent = new DecentBiome(worldInfo);
			this.caves = new CaveBiome(worldInfo);
			this.hell = new HellFloorBiome(worldInfo);
			this.trees = new TreeDepositer(worldInfo);
			this.testBiome = new TreeTestBiome(worldInfo);
			this.orginArea = new OriginBiome(worldInfo);
			this.hellCeiling = new HellCeilingBiome(worldInfo);
			this.crashSiteDepositer = new CrashSiteDepositer(worldInfo);
			this.caves.SetLootModifiersByGameMode();
		}

		public override void BuildWorldChunk(BlockTerrain terrain, IntVector3 minLoc)
		{
			terrain.WaterLevel = 1.5f;
			for (int i = 0; i < 16; i++)
			{
				int num = i + minLoc.Z;
				long num2 = (long)num * (long)num;
				for (int j = 0; j < 16; j++)
				{
					int num3 = j + minLoc.X;
					float num4 = (float)Math.Sqrt((double)((long)num3 * (long)num3 + num2));
					float num5 = 1f;
					int num6 = 0;
					while (num4 > 4400f)
					{
						num4 -= 4400f;
						num6++;
					}
					if ((num6 & 1) != 0)
					{
						num4 = 4400f - num4;
					}
					if (num4 < 200f)
					{
						this.classic.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
					}
					else if (num4 < 300f)
					{
						float num7 = (num4 - 200f) / 100f;
						this.classic.BuildColumn(terrain, num3, num, minLoc.Y, 1f - num7);
						this.lagoon.BuildColumn(terrain, num3, num, minLoc.Y, num7);
					}
					else if (num4 < 900f)
					{
						this.lagoon.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
					}
					else if (num4 < 1000f)
					{
						float num8 = (num4 - 900f) / 100f;
						this.lagoon.BuildColumn(terrain, num3, num, minLoc.Y, 1f - num8);
						this.dessert.BuildColumn(terrain, num3, num, minLoc.Y, num8);
					}
					else if (num4 < 1600f)
					{
						this.dessert.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
					}
					else if (num4 < 1700f)
					{
						float num9 = (num4 - 1600f) / 100f;
						this.dessert.BuildColumn(terrain, num3, num, minLoc.Y, 1f - num9);
						this.mountains.BuildColumn(terrain, num3, num, minLoc.Y, num9);
					}
					else if (num4 < 2300f)
					{
						this.mountains.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
					}
					else if (num4 < 2400f)
					{
						float num10 = (num4 - 2300f) / 100f;
						this.mountains.BuildColumn(terrain, num3, num, minLoc.Y, 1f - num10);
						this.arctic.BuildColumn(terrain, num3, num, minLoc.Y, num10);
					}
					else if (num4 < 3000f)
					{
						this.arctic.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
					}
					else if (num4 < 3600f)
					{
						float num11 = (num4 - 3000f) / 600f;
						num5 = 1f - num11;
						this.decent.BuildColumn(terrain, num3, num, minLoc.Y, num11);
					}
					float num12 = MathHelper.Clamp(num4 / 3600f, 0f, 1f);
					this.hellCeiling.BuildColumn(terrain, num3, num, minLoc.Y, num5);
					if (num4 > 300f && num4 < 3600f)
					{
						this.crashSiteDepositer.BuildColumn(terrain, num3, num, minLoc.Y, num12);
					}
					this.caves.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
					this.oreDepositor.BuildColumn(terrain, num3, num, minLoc.Y, num12);
					this.hell.BuildColumn(terrain, num3, num, minLoc.Y, num5);
					this.bedrockDepositor.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
					this.orginArea.BuildColumn(terrain, num3, num, minLoc.Y, 1f);
				}
			}
			for (int k = 3; k < 13; k++)
			{
				int num13 = k + minLoc.Z;
				for (int l = 3; l < 13; l++)
				{
					int num14 = l + minLoc.X;
					this.trees.BuildColumn(terrain, num14, num13, minLoc.Y, 1f);
				}
			}
			this.hell.PostChunkProcess();
		}

		private const int BandWidth = 600;

		private const int TransitionWidth = 300;

		private CaveBiome caves;

		private OceanBiome ocean;

		private ClassicBiome classic;

		private DesertBiome dessert;

		private LagoonBiome lagoon;

		private MountainBiome mountains;

		private DecentBiome decent;

		private ArcticBiome arctic;

		private OreDepositer oreDepositor;

		private BedrockDepositer bedrockDepositor;

		private HellFloorBiome hell;

		private HellCeilingBiome hellCeiling;

		private TreeDepositer trees;

		private OriginBiome orginArea;

		private CrashSiteDepositer crashSiteDepositer;

		private TreeTestBiome testBiome;
	}
}
