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
			for (int z = 0; z < 16; z++)
			{
				int worldZ = z + minLoc.Z;
				long zsqu = (long)worldZ * (long)worldZ;
				for (int x = 0; x < 16; x++)
				{
					int worldX = x + minLoc.X;
					float dist = (float)Math.Sqrt((double)((long)worldX * (long)worldX + zsqu));
					float hellBlender = 1f;
					int flips = 0;
					while (dist > 4400f)
					{
						dist -= 4400f;
						flips++;
					}
					if ((flips & 1) != 0)
					{
						dist = 4400f - dist;
					}
					if (dist < 200f)
					{
						this.classic.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
					}
					else if (dist < 300f)
					{
						float blender = (dist - 200f) / 100f;
						this.classic.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f - blender);
						this.lagoon.BuildColumn(terrain, worldX, worldZ, minLoc.Y, blender);
					}
					else if (dist < 900f)
					{
						this.lagoon.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
					}
					else if (dist < 1000f)
					{
						float blender2 = (dist - 900f) / 100f;
						this.lagoon.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f - blender2);
						this.dessert.BuildColumn(terrain, worldX, worldZ, minLoc.Y, blender2);
					}
					else if (dist < 1600f)
					{
						this.dessert.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
					}
					else if (dist < 1700f)
					{
						float blender3 = (dist - 1600f) / 100f;
						this.dessert.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f - blender3);
						this.mountains.BuildColumn(terrain, worldX, worldZ, minLoc.Y, blender3);
					}
					else if (dist < 2300f)
					{
						this.mountains.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
					}
					else if (dist < 2400f)
					{
						float blender4 = (dist - 2300f) / 100f;
						this.mountains.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f - blender4);
						this.arctic.BuildColumn(terrain, worldX, worldZ, minLoc.Y, blender4);
					}
					else if (dist < 3000f)
					{
						this.arctic.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
					}
					else if (dist < 3600f)
					{
						float blender5 = (dist - 3000f) / 600f;
						hellBlender = 1f - blender5;
						this.decent.BuildColumn(terrain, worldX, worldZ, minLoc.Y, blender5);
					}
					float worldBlender = MathHelper.Clamp(dist / 3600f, 0f, 1f);
					this.hellCeiling.BuildColumn(terrain, worldX, worldZ, minLoc.Y, hellBlender);
					if (dist > 300f && dist < 3600f)
					{
						this.crashSiteDepositer.BuildColumn(terrain, worldX, worldZ, minLoc.Y, worldBlender);
					}
					this.caves.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
					this.oreDepositor.BuildColumn(terrain, worldX, worldZ, minLoc.Y, worldBlender);
					this.hell.BuildColumn(terrain, worldX, worldZ, minLoc.Y, hellBlender);
					this.bedrockDepositor.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
					this.orginArea.BuildColumn(terrain, worldX, worldZ, minLoc.Y, 1f);
				}
			}
			for (int z2 = 3; z2 < 13; z2++)
			{
				int worldZ2 = z2 + minLoc.Z;
				for (int x2 = 3; x2 < 13; x2++)
				{
					int worldX2 = x2 + minLoc.X;
					this.trees.BuildColumn(terrain, worldX2, worldZ2, minLoc.Y, 1f);
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
