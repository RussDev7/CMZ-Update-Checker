using System;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public abstract class Biome
	{
		public Biome(WorldInfo worldInfo)
		{
		}

		public abstract void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender);

		protected WorldInfo WorldInfo;

		public float WaterDepth = 12f;

		protected static readonly int uninitblock = Block.SetType(0, BlockTypeEnum.NumberOfBlocks);

		protected static readonly int lanternblock = Block.SetType(0, BlockTypeEnum.Lantern);

		protected static readonly int fixedLanternblock = Block.SetType(0, BlockTypeEnum.FixedLantern);

		protected static readonly int emptyblock = Block.SetType(0, BlockTypeEnum.Empty);

		protected static readonly int rockblock = Block.SetType(0, BlockTypeEnum.Rock);

		protected static readonly int grassblock = Block.SetType(0, BlockTypeEnum.Grass);

		protected static readonly int dirtblock = Block.SetType(0, BlockTypeEnum.Dirt);

		protected static readonly int surfaceLavablock = Block.SetType(0, BlockTypeEnum.SurfaceLava);

		protected static readonly int deepLavablock = Block.SetType(0, BlockTypeEnum.DeepLava);

		protected static readonly int coalBlock = Block.SetType(0, BlockTypeEnum.CoalOre);

		protected static readonly int copperBlock = Block.SetType(0, BlockTypeEnum.CopperOre);

		protected static readonly int ironBlock = Block.SetType(0, BlockTypeEnum.IronOre);

		protected static readonly int goldBlock = Block.SetType(0, BlockTypeEnum.GoldOre);

		protected static readonly int diamondsBlock = Block.SetType(0, BlockTypeEnum.DiamondOre);

		protected static readonly int bedrockBlock = Block.SetType(0, BlockTypeEnum.Bedrock);

		protected static readonly int sandBlock = Block.SetType(0, BlockTypeEnum.Sand);

		protected static readonly int snowBlock = Block.SetType(0, BlockTypeEnum.Snow);

		protected static readonly int iceBlock = Block.SetType(0, BlockTypeEnum.Ice);

		protected static readonly int LogBlock = Block.SetType(0, BlockTypeEnum.Log);

		protected static readonly int LeafBlock = Block.SetType(0, BlockTypeEnum.Leaves);

		protected static readonly int BloodSToneBlock = Block.SetType(0, BlockTypeEnum.BloodStone);

		protected static readonly int SpaceRockBlock = Block.SetType(0, BlockTypeEnum.SpaceRock);

		protected static readonly int torchdown = Block.SetType(0, BlockTypeEnum.TorchNEGY);

		protected static readonly int SlimeBlock = Block.SetType(0, BlockTypeEnum.Slime);

		protected static readonly int lootBlock = Block.SetType(0, BlockTypeEnum.LootBlock);

		protected static readonly int luckyLootBlock = Block.SetType(0, BlockTypeEnum.LuckyLootBlock);

		protected static readonly int enemyBlockOff = Block.SetType(0, BlockTypeEnum.EnemySpawnOff);

		protected static readonly int enemyBlockOn = Block.SetType(0, BlockTypeEnum.EnemySpawnOn);

		protected static readonly int enemyBlockRareOff = Block.SetType(0, BlockTypeEnum.EnemySpawnRareOff);

		protected static readonly int enemyBlockRareOn = Block.SetType(0, BlockTypeEnum.EnemySpawnRareOn);

		protected static readonly int alienSpawnOff = Block.SetType(0, BlockTypeEnum.AlienSpawnOff);

		protected static readonly int alienSpawnOn = Block.SetType(0, BlockTypeEnum.AlienSpawnOn);

		protected static readonly int hellSpawnOff = Block.SetType(0, BlockTypeEnum.HellSpawnOff);

		protected static readonly int hellSpawnOn = Block.SetType(0, BlockTypeEnum.HellSpawnOn);

		protected static readonly int bossSpawnOff = Block.SetType(0, BlockTypeEnum.BossSpawnOff);

		protected static readonly int bossSpawnOn = Block.SetType(0, BlockTypeEnum.BossSpawnOn);
	}
}
