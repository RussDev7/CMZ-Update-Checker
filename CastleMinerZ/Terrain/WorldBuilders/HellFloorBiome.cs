using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.Noise;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class HellFloorBiome : Biome
	{
		private bool IsBossSpawnerGameMode()
		{
			return CastleMinerZGame.Instance.GameMode == GameModeTypes.Scavenger || CastleMinerZGame.Instance.GameMode == GameModeTypes.Survival || CastleMinerZGame.Instance.GameMode == GameModeTypes.Exploration || CastleMinerZGame.Instance.GameMode == GameModeTypes.Creative;
		}

		private int GetNextBossBlockCountdown(int spawnCount)
		{
			float num = 0.2f;
			float num2 = 1.1f;
			if (this.Rnd != null)
			{
				this.Rnd.RandomDouble((double)(-(double)num), (double)num);
				double num3 = 1.0;
				int num4 = 4000 * (int)Math.Pow((double)(spawnCount + 1), (double)num2);
				return (int)(num3 * (double)num4);
			}
			return 0;
		}

		public HellFloorBiome(WorldInfo worldInfo)
			: base(worldInfo)
		{
			this._noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
			this.InitializeBossSpawnParameters(worldInfo);
		}

		private void InitializeBossSpawnParameters(WorldInfo worldInfo)
		{
			this.Rnd = new Random(worldInfo.Seed);
			this.bossSpawnBlockCountdown = this.GetNextBossBlockCountdown(CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned);
			this.bossSpawnerLocs = new List<IntVector3>();
			if (CastleMinerZGame.Instance.CurrentWorld.MaxHellBossSpawns == 0)
			{
				CastleMinerZGame.Instance.CurrentWorld.MaxHellBossSpawns = 50;
			}
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = 1;
			float num2 = 0f;
			int num3 = 4;
			for (int i = 0; i < num3; i++)
			{
				num2 += this._noiseFunction.ComputeNoise(0.03125f * (float)worldX * (float)num + 200f, 0.03125f * (float)worldZ * (float)num + 200f) / (float)num;
				num *= 2;
			}
			int num4 = 4 + (int)(num2 * 10f) + 3;
			for (int j = 0; j < 32; j++)
			{
				int num5 = j + minY;
				IntVector3 intVector = new IntVector3(worldX, num5, worldZ);
				int num6 = terrain.MakeIndexFromWorldIndexVector(intVector);
				if (j < num4)
				{
					terrain._blocks[num6] = Biome.BloodSToneBlock;
				}
				else if (j <= 4)
				{
					terrain._blocks[num6] = Biome.deepLavablock;
				}
				this.CheckForBossSpawns(terrain, intVector, num6, j, num4);
			}
		}

		private void CheckForBossSpawns(BlockTerrain terrain, IntVector3 worldPos, int index, int y, int groundlevel)
		{
			if (CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned >= CastleMinerZGame.Instance.CurrentWorld.MaxHellBossSpawns)
			{
				this.bossSpawnBlockCountdown = 0;
				return;
			}
			if (this.bossSpawnBlockCountdown != 0 && y == groundlevel && y > 4)
			{
				this.bossSpawnBlockCountdown--;
				if (this.bossSpawnBlockCountdown <= 0)
				{
					CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned++;
					this.bossSpawnBlockCountdown = this.GetNextBossBlockCountdown(CastleMinerZGame.Instance.CurrentWorld.HellBossesSpawned);
					terrain._blocks[index] = Biome.bossSpawnOff;
					this.bossSpawnerLocs.Add(worldPos);
					long num = (long)worldPos.Z * (long)worldPos.Z;
					Math.Sqrt((double)((long)worldPos.X * (long)worldPos.X + num));
				}
			}
		}

		private void ProcessBossSpawns()
		{
			if (!this.IsBossSpawnerGameMode())
			{
				return;
			}
			if (CastleMinerZGame.Instance == null || CastleMinerZGame.Instance.CurrentNetworkSession == null || CastleMinerZGame.Instance.LocalPlayer == null || CastleMinerZGame.Instance.LocalPlayer.Gamer == null || CastleMinerZGame.Instance.LocalPlayer.Gamer.Session == null)
			{
				return;
			}
			for (int i = 0; i < this.bossSpawnerLocs.Count; i++)
			{
				AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this.bossSpawnerLocs[i], BlockTypeEnum.BossSpawnOff);
			}
			if (this.bossSpawnerLocs.Count > 0)
			{
				this.bossSpawnerLocs.Clear();
			}
		}

		public void PostChunkProcess()
		{
			this.ProcessBossSpawns();
		}

		private const int HellHeight = 32;

		private const int LavaLevel = 4;

		private const int MaxHillHeight = 32;

		private const float worldScale = 0.03125f;

		private const int cMaxBossesToSpawn = 50;

		private List<IntVector3> bossSpawnerLocs;

		private Random Rnd;

		private int bossSpawnBlockCountdown;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));
	}
}
