using System;
using System.Collections.Generic;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Terrain
{
	public class Spawner
	{
		public IntVector3 Location
		{
			get
			{
				return this._location;
			}
		}

		public Spawner(IntVector3 location)
		{
			this._location = location;
			this._currentSpawnID = "spawner" + Spawner.TotalSpawnerCount;
			Spawner.TotalSpawnerCount++;
		}

		public Spawner(BinaryReader reader)
		{
			this.Read(reader);
		}

		public void SetSourceData(BlockTypeEnum blockSource)
		{
			this._currentData = this.GetSpawnData(blockSource);
		}

		public void SetSourceFromID(string spawnID)
		{
			this._currentData = this.GetSpawnData(spawnID);
		}

		public void StartSpawner(BlockTypeEnum blockSource)
		{
			this.spawnerView = new SpawnBlockView(this.Location, blockSource);
			this.SetState(Spawner.SpawnState.StartTriggered);
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			this._originalBlockType = blockSource;
			this._currentBlockType = this.GetActiveSpawnBlockType();
			GameUtils.ClearSurroundingBlocks(this.Location, 3);
			this.TriggerNearbySpawnBlocks();
			this.spawnerView.SetBlockLight(true);
			this.SetSourceData(blockSource);
			this._currentlySpawnedEnemyCount = 0;
			this._remainingWaveCount = this._currentData.waveCount;
			this.StartWave();
		}

		private Player GetRandomPlayer(List<TargetSearchResult> targetList)
		{
			if (targetList.Count == 0)
			{
				return CastleMinerZGame.Instance.LocalPlayer;
			}
			string text = "";
			foreach (TargetSearchResult targetSearchResult in targetList)
			{
				text = text + targetSearchResult.player.Gamer.Gamertag + ", ";
			}
			int num = MathTools.RandomInt(0, targetList.Count);
			DebugUtils.Log(string.Concat(new object[]
			{
				"TargetList: ",
				text,
				" Roll ",
				num,
				" = ",
				targetList[num].player
			}));
			num = Math.Min(num, targetList.Count - 1);
			return targetList[num].player;
		}

		private void UpdateTargetList()
		{
			List<TargetSearchResult> list = TargetUtils.FindTargetsInRange(this.Location, 24f, true);
			foreach (TargetSearchResult targetSearchResult in list)
			{
				bool flag = true;
				foreach (TargetSearchResult targetSearchResult2 in this._nearbyPlayers)
				{
					if (targetSearchResult2.player == targetSearchResult.player)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					BroadcastTextMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, targetSearchResult.player.Gamer.Gamertag + " is a valid spawn target!");
				}
			}
			this._nearbyPlayers.Clear();
			this._nearbyPlayers = list;
		}

		public bool CanStart()
		{
			return this._currentState == Spawner.SpawnState.Listening;
		}

		private void StartWave()
		{
			this._remainingSpawnValue += this._currentData.spawnPoints / this._currentData.waveCount;
			this.SetState(Spawner.SpawnState.Activated);
		}

		public void HandleStopSpawningMessage()
		{
			this.StopSpawning();
			this.SetState(Spawner.SpawnState.Depleted);
		}

		private void HandleWaveComplete()
		{
			this._remainingWaveCount--;
			if (this._remainingWaveCount <= 0)
			{
				this.HandleStopSpawningMessage();
				return;
			}
			this.SetState(Spawner.SpawnState.WaveIntermission);
			this._spawnTimer = this._currentData.waveRate;
		}

		private Spawner.DifficultyData GetCurrentDifficulty()
		{
			return new Spawner.DifficultyData
			{
				spawnersDefeated = Spawner.SpawnerDefeatedCount,
				difficultyMode = CastleMinerZGame.Instance.Difficulty
			};
		}

		private float GetSoftCappedValue(float value, float softCap, float hardCap, float factor = 3f)
		{
			if (value > softCap)
			{
				value += (value - softCap) / factor;
			}
			if (value > hardCap)
			{
				value += (value - hardCap) / (factor * 10f);
			}
			return value;
		}

		private float GetSpawnPoints(float basePoints, Spawner.DifficultyData diffData)
		{
			basePoints += 300f;
			basePoints += this.GetSoftCappedValue((float)diffData.spawnersDefeated * 150f, 1000f, 2000f, 3f);
			switch (diffData.difficultyMode)
			{
			default:
				basePoints *= 1f;
				break;
			case GameDifficultyTypes.HARD:
				basePoints *= 1.5f;
				break;
			}
			return basePoints;
		}

		private SpawnData GetMockSpawnData()
		{
			bool flag = MathTools.RandomInt(0, 1) == 1;
			SpawnData spawnData;
			if (flag)
			{
				spawnData = Spawner._SpawnData[1];
			}
			else
			{
				spawnData = Spawner._SpawnData[0];
			}
			return spawnData;
		}

		private void ApplyDifficultyModifiers(ref SpawnData spawnData, Spawner.DifficultyData difficultyData)
		{
			float num = this.GetSpawnPoints((float)spawnData.spawnPoints, difficultyData);
			num = MathTools.RandomFloat(num * 0.8f, num * 1.2f);
			spawnData.spawnPoints = (int)num;
			spawnData.maxTier = Math.Min(spawnData.maxTier + difficultyData.spawnersDefeated, 10);
		}

		private SpawnData GetSpawnData(string spawnID)
		{
			SpawnData spawnDataById = this.GetSpawnDataById(spawnID);
			return this.GetLocalSpawnData(spawnDataById);
		}

		private SpawnData GetSpawnData(BlockTypeEnum blockSource)
		{
			SpawnData spawnDataByBlockSource = this.GetSpawnDataByBlockSource(blockSource);
			return this.GetLocalSpawnData(spawnDataByBlockSource);
		}

		private SpawnData GetLocalSpawnData(SpawnData sourceSpawnData)
		{
			SpawnData spawnData = sourceSpawnData;
			if (spawnData == null)
			{
				spawnData = this.GetMockSpawnData();
			}
			SpawnData spawnData2 = spawnData.Copy();
			this.ApplyDifficultyModifiers(ref spawnData2, this.GetCurrentDifficulty());
			return spawnData2;
		}

		public void StopSpawning()
		{
			this._remainingSpawnValue = 0;
			this._spawnTimer = 0f;
		}

		private SpawnData GetSpawnDataById(string id)
		{
			foreach (SpawnData spawnData2 in Spawner._SpawnData)
			{
				if (spawnData2.id.Equals(id))
				{
					return spawnData2;
				}
			}
			return null;
		}

		private SpawnData GetSpawnDataByBlockSource(BlockTypeEnum blockSource)
		{
			foreach (SpawnData spawnData2 in Spawner._SpawnData)
			{
				if (blockSource == spawnData2.blockTypeSource)
				{
					return spawnData2;
				}
			}
			return null;
		}

		private void HandlePlayerDiedOrFled()
		{
			this.SetState(Spawner.SpawnState.Listening);
		}

		private void HandleSpawnEvent()
		{
			if (this._currentState == Spawner.SpawnState.WaveIntermission)
			{
				this.StartWave();
				return;
			}
			this.UpdateTargetList();
			this.SetState(Spawner.SpawnState.Spawning);
			LootResult randomSpawn = PossibleLootType.GetRandomSpawn(this.Location.Y, 1, 5, false, this._remainingSpawnValue, this._currentData.packageFlags);
			IntVector3 location = this.Location;
			location.Y++;
			int num = randomSpawn.value / randomSpawn.count;
			int num2 = num * randomSpawn.count;
			Vector3 vector = location + Vector3.Zero;
			for (int i = 0; i < randomSpawn.count; i++)
			{
				string gamertag = this.GetRandomPlayer(this._nearbyPlayers).Gamer.Gamertag;
				EnemyManager.Instance.SpawnEnemy(vector, randomSpawn.spawnID, this.Location, num, gamertag);
				this._currentlySpawnedEnemyCount++;
				DebugUtils.Log(string.Concat(new object[] { "Spawning Enemy #", this._currentlySpawnedEnemyCount, " Value ", num, " RemainingSpawnValue ", this._remainingSpawnValue }));
			}
			this.DeductSpawnValue(num2);
		}

		private void ResetSpawnTimer()
		{
			this._spawnTimer = this._currentData.spawnRate;
		}

		public void SetState(Spawner.SpawnState newState)
		{
			this._currentState = newState;
			switch (newState)
			{
			case Spawner.SpawnState.Activated:
				this.ResetSpawnTimer();
				return;
			case Spawner.SpawnState.Spawning:
			case Spawner.SpawnState.WaveIntermission:
			case Spawner.SpawnState.RewardCollected:
				break;
			case Spawner.SpawnState.Depleted:
				this._spawnTimer = 0f;
				this.spawnerView.SetBlockLight(false);
				return;
			case Spawner.SpawnState.Completed:
			{
				Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
				if (this._currentData.blockTypeSource == BlockTypeEnum.AlienSpawnOff)
				{
					this.PlaceRewardBlock(new IntVector3(this.Location.X, this.Location.Y, this.Location.Z + 1), this.GetRewardBlockType(this.Tier));
				}
				if (this._currentData.blockTypeSource == BlockTypeEnum.BossSpawnOff)
				{
					this.PlaceRewardBlock(new IntVector3(this.Location.X + 1, this.Location.Y, this.Location.Z), this.GetRewardBlockType(this.Tier));
					this.PlaceRewardBlock(new IntVector3(this.Location.X - 1, this.Location.Y, this.Location.Z), this.GetRewardBlockType(this.Tier));
					if (MathTools.RandomInt(100) < 50)
					{
						this.PlaceRewardBlock(new IntVector3(this.Location.X, this.Location.Y - 1, this.Location.Z), this.GetRewardBlockType(this.Tier));
					}
					if (MathTools.RandomInt(100) < 50)
					{
						this.PlaceRewardBlock(new IntVector3(this.Location.X, this.Location.Y + 1, this.Location.Z), this.GetRewardBlockType(this.Tier));
					}
				}
				this.PlaceRewardBlock(this.Location, this.GetRewardBlockType(this.Tier));
				NetworkGamer gamerFromID = CastleMinerZGame.Instance.GetGamerFromID(this._lastPlayerID);
				string text = localPlayer.Gamer.Gamertag;
				if (gamerFromID != null)
				{
					text = gamerFromID.Gamertag;
				}
				BroadcastTextMessage.Send((LocalNetworkGamer)localPlayer.Gamer, text + " " + Strings.Has_extinguished_a_spawn_block);
				Spawner.SpawnerDefeatedCount++;
				break;
			}
			case Spawner.SpawnState.Reset:
				this.ResetSpawner();
				return;
			default:
				return;
			}
		}

		private void PlaceRewardBlock(IntVector3 rewardBlockLocation, BlockTypeEnum rewardBlockType)
		{
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			AlterBlockMessage.Send((LocalNetworkGamer)localPlayer.Gamer, rewardBlockLocation, rewardBlockType);
		}

		private BlockTypeEnum SetCurrentBlock(BlockTypeEnum blockType)
		{
			this._currentBlockType = blockType;
			return this._currentBlockType;
		}

		private void DeductSpawnValue(int value)
		{
			this._remainingSpawnValue -= value;
			if (this._remainingSpawnValue < 100)
			{
				this.HandleWaveComplete();
				return;
			}
			this.SetState(Spawner.SpawnState.Activated);
		}

		private void DeductEnemyCount()
		{
			this._currentlySpawnedEnemyCount--;
			if (this._currentState == Spawner.SpawnState.Depleted && this._currentlySpawnedEnemyCount <= 0)
			{
				this.SetState(Spawner.SpawnState.Completed);
			}
		}

		public void HandleEnemyDefeated(int spawnValue, byte killerID)
		{
			this._lastPlayerID = killerID;
			this.DeductEnemyCount();
		}

		public void HandleEnemyRemoved(int spawnValue)
		{
			if (this._currentState == Spawner.SpawnState.Listening)
			{
				return;
			}
			DebugUtils.Log(string.Concat(new object[] { "HandleEnemyRemoved: ", this._remainingSpawnValue, "(+", spawnValue, ")EnemyCount ", this._currentlySpawnedEnemyCount, " State ", this._currentState }));
			this.SetState(Spawner.SpawnState.Reset);
		}

		public bool IsHellBlock()
		{
			return this._originalBlockType == BlockTypeEnum.HellSpawnOff;
		}

		public void HandleRewardCollected()
		{
			if (this._currentState == Spawner.SpawnState.Completed)
			{
				this.SetState(Spawner.SpawnState.RewardCollected);
			}
		}

		public void UpdateSpawner(GameTime gameTime)
		{
			if (this._currentState == Spawner.SpawnState.None || this._currentState == Spawner.SpawnState.Spawning)
			{
				return;
			}
			if (this._spawnTimer == 0f)
			{
				return;
			}
			this._spawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			this.spawnerView.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
			if (this._spawnTimer <= 0f)
			{
				this.HandleSpawnEvent();
			}
		}

		private BlockTypeEnum GetRewardBlockType(LootGrade tier)
		{
			int num = (int)(this._currentData.baseLuckyLootChance + tier * (LootGrade)this._currentData.luckyLootChancePerTier);
			if (num < MathTools.RandomInt(1, 100))
			{
				return BlockTypeEnum.LuckyLootBlock;
			}
			return BlockTypeEnum.LootBlock;
		}

		public void ResetSpawner()
		{
			this.SetState(Spawner.SpawnState.Listening);
			this._spawnTimer = 0f;
			this._currentBlockType = this._originalBlockType;
			this.spawnerView.Reset();
			AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this.Location, this._currentBlockType);
			BroadcastTextMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, "No available targets. Spawner reset. Stay closer to avoid this!");
			EnemyManager.Instance.ResetSpawnerEnemies(this);
		}

		public void Write(BinaryWriter writer)
		{
			this._location.Write(writer);
			writer.Write(this._currentSpawnID);
		}

		public void Read(BinaryReader reader)
		{
			this._location = IntVector3.Read(reader);
			this._currentSpawnID = reader.ReadString();
		}

		private BlockTypeEnum GetActiveSpawnBlockType()
		{
			return this.spawnerView.GetActiveSpawnBlockType();
		}

		private void TriggerNearbySpawnBlocks()
		{
			int num = 10;
			for (int i = this.Location.Y; i < num + this.Location.Y; i++)
			{
				for (int j = this.Location.X - num; j < num + this.Location.X; j++)
				{
					for (int k = this.Location.Z - num; k < num + this.Location.Z; k++)
					{
						IntVector3 intVector = new IntVector3(j, i, k);
						if (!(intVector == this.Location))
						{
							BlockTypeEnum block = InGameHUD.GetBlock(intVector);
							if (BlockType.IsSpawnerClickable(block))
							{
								Spawner spawner = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(intVector, true, block);
								if (spawner.CanStart())
								{
									if (CastleMinerZGame.Instance.IsOnlineGame)
									{
										BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, Strings.A_linked_spawner_was_also_triggered);
									}
									spawner.StartSpawner(block);
								}
							}
						}
					}
				}
			}
		}

		private const float c_LightOnTime = 0.8f;

		private const float c_LightOffTime = 0.3f;

		private const int c_MinimumRequiredSpawnValue = 100;

		private static readonly SpawnData[] _SpawnData = new SpawnData[]
		{
			new SpawnData(BlockTypeEnum.EnemySpawnOff, 1000, 2f, 4, 5f, 1, 2, 20, 7, PackageBitFlags.None),
			new SpawnData(BlockTypeEnum.EnemySpawnRareOff, 1200, 0.5f, 3, 4f, 1, 3, 50, 10, PackageBitFlags.None),
			new SpawnData(BlockTypeEnum.AlienSpawnOff, 500, 4f, 3, 7f, 3, 3, 50, 10, PackageBitFlags.Alien),
			new SpawnData(BlockTypeEnum.AlienHordeOff, 1200, 2f, 3, 8f, 3, 3, 50, 10, PackageBitFlags.Alien),
			new SpawnData(BlockTypeEnum.HellSpawnOff, 1500, 4f, 3, 7f, 4, 7, 50, 10, PackageBitFlags.Hell),
			new SpawnData(BlockTypeEnum.BossSpawnOff, 1000, 4f, 1, 3f, 4, 7, 50, 10, PackageBitFlags.Epic | PackageBitFlags.Hell | PackageBitFlags.Boss)
		};

		public static int SpawnerDefeatedCount = 0;

		public static int TotalSpawnerCount = 0;

		public bool Destroyed;

		public LootGrade Tier;

		private IntVector3 _location;

		private Spawner.SpawnState _currentState = Spawner.SpawnState.Listening;

		private SpawnData _currentData;

		private string _currentSpawnID;

		private float _spawnTimer;

		private int _remainingSpawnValue;

		private int _currentlySpawnedEnemyCount;

		private int _remainingWaveCount;

		private BlockTypeEnum _currentBlockType;

		private List<int> _enemyIDs = new List<int>();

		private byte _lastPlayerID;

		private BlockTypeEnum _originalBlockType;

		private List<TargetSearchResult> _nearbyPlayers = new List<TargetSearchResult>();

		private SpawnBlockView spawnerView;

		private struct DifficultyData
		{
			public int depth;

			public int distance;

			public int days;

			public int players;

			public int spawnersDefeated;

			public GameDifficultyTypes difficultyMode;
		}

		public enum SpawnState
		{
			None,
			Chargeable,
			Listening,
			StartTriggered,
			Activated,
			Spawning,
			WaveIntermission,
			Depleted,
			Completed,
			RewardCollected,
			Reset
		}
	}
}
