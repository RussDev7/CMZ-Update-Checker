using System;
using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.Terrain
{
	public class SpawnData
	{
		public SpawnData(BlockTypeEnum pBlockTypeSource, int pSpawnPoints, float pSpawnRate, int pWaveCount, float pWaveRate, int pMinTier, int pMaxTier, int pLuckyLootChance, int pLuckyLootChancePerTier, PackageBitFlags pPackageFlags)
		{
			this.id = pBlockTypeSource.ToString();
			this.blockTypeSource = pBlockTypeSource;
			this.spawnPoints = pSpawnPoints;
			this.spawnRate = pSpawnRate;
			this.waveCount = pWaveCount;
			this.waveRate = pWaveRate;
			this.minTier = pMinTier;
			this.maxTier = pMaxTier;
			this.baseLuckyLootChance = pLuckyLootChance;
			this.luckyLootChancePerTier = pLuckyLootChancePerTier;
			this.packageFlags = pPackageFlags;
		}

		public SpawnData Copy()
		{
			return new SpawnData(this.blockTypeSource, this.spawnPoints, this.spawnRate, this.waveCount, this.waveRate, this.minTier, this.maxTier, this.baseLuckyLootChance, this.luckyLootChancePerTier, this.packageFlags)
			{
				surgeChance = this.surgeChance,
				bossEnding = this.bossEnding
			};
		}

		public string id;

		public BlockTypeEnum blockTypeSource;

		public int minTier;

		public int maxTier;

		public int spawnPoints;

		public float spawnRate;

		public int waveCount;

		public float waveRate;

		public float surgeChance;

		public bool bossEnding;

		public int baseLuckyLootChance;

		public int luckyLootChancePerTier;

		public PackageBitFlags packageFlags;
	}
}
