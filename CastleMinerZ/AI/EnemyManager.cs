using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Net.GamerServices;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class EnemyManager : Entity, IGameMessageHandler
	{
		public static bool ReadyToSpawnFelgard
		{
			get
			{
				return EnemyManager._spawnFelgardTimer.Expired;
			}
		}

		public EnemyManager()
		{
			EnemyManager.Instance = this;
			this.Visible = false;
			this.Collidee = false;
			this.Collider = false;
			this._graphicsDevice = CastleMinerZGame.Instance.GraphicsDevice;
			this._contentManager = CastleMinerZGame.Instance.Content;
			this._timeSinceLastSurfaceEnemy = 0f;
			this._timeSinceLastCaveEnemy = 0f;
			this._localSurfaceEnemyCount = 0;
			this._localCaveEnemyCount = 0;
			this._timeLeftTilFrenzy = -1f;
			this._enemies = new List<BaseZombie>();
			this.ZombieFestIsOn = false;
			this._fireballs = new List<FireballEntity>();
			this._dragon = null;
			this._dragonClient = null;
			int num = 8;
			this._fireballDamageBuffer = new IntVector3[(num + 1) * num * num];
			this._fireballDamageItemTypes = new BlockTypeEnum[this._fireballDamageBuffer.Length];
			this._dependentItemsToRemoveBuffer = new IntVector3[this._fireballDamageBuffer.Length];
			this._dependItemTypes = new BlockTypeEnum[this._fireballDamageBuffer.Length];
			this._rnd = new Random();
			this.InitializeDragonBox = true;
			this._searchForSpaceRockDelegate = new TaskDelegate(this.SearchForSpaceRock);
			this.dragonDistanceIndex = 0;
			this.NextDragonAllowedTime = TimeSpan.FromHours(1000000.0);
			this.NextTimedDragonType = DragonTypeEnum.FIRE;
			GameMessageManager.Instance.Subscribe(this, new GameMessageType[]
			{
				GameMessageType.LocalPlayerMinedBlock,
				GameMessageType.LocalPlayerPickedAtBlock,
				GameMessageType.LocalPlayerFiredGun
			});
		}

		public bool DragonIsActive
		{
			get
			{
				return this._dragonClient != null;
			}
		}

		public bool DragonIsAlive
		{
			get
			{
				return this._dragonClient != null && !this._dragonClient.Dead;
			}
		}

		public Vector3 DragonPosition
		{
			get
			{
				if (this._dragonClient != null)
				{
					return this._dragonClient.WorldPosition;
				}
				return Vector3.Zero;
			}
		}

		public void ApplyExplosiveDamageToZombies(Vector3 location, float damageRange, byte gamerID, InventoryItemIDs itemID)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				float num = Vector3.Distance(location, this._enemies[i].LocalPosition);
				float num2 = damageRange / 2f;
				if (num < damageRange)
				{
					float num3;
					if (num < num2)
					{
						num3 = 12f;
					}
					else
					{
						num3 = 12f * (1f - (num - num2) / (damageRange - num2));
					}
					this._enemies[i].TakeExplosiveDamage(num3, gamerID, itemID);
				}
			}
		}

		protected override void OnParentChanged(Entity oldParent, Entity newParent)
		{
			if (newParent == null)
			{
				EnemyManager.Instance = null;
			}
			else if (newParent != oldParent)
			{
				EnemyManager.Instance = this;
			}
			base.OnParentChanged(oldParent, newParent);
		}

		public void HandleMessage(CastleMinerZMessage message)
		{
			if (message is UpdateDragonMessage)
			{
				this.HandleUpdateDragonMessage((UpdateDragonMessage)message);
				return;
			}
			if (message is DragonAttackMessage)
			{
				this.HandleDragonAttackMessage((DragonAttackMessage)message);
				return;
			}
			if (message is DetonateFireballMessage)
			{
				this.HandleDetonateFireballMessage((DetonateFireballMessage)message);
				return;
			}
			if (message is SpawnDragonMessage)
			{
				this.HandleSpawnDragonMessage((SpawnDragonMessage)message);
				return;
			}
			if (message is SpawnEnemyMessage)
			{
				this.HandleSpawnEnemyMessage((SpawnEnemyMessage)message);
				return;
			}
			if (message is KillEnemyMessage)
			{
				this.HandleKillEnemyMessage((KillEnemyMessage)message);
				return;
			}
			if (message is SpeedUpEnemyMessage)
			{
				this.HandleSpeedUpEnemyMessage((SpeedUpEnemyMessage)message);
				return;
			}
			if (message is RemoveDragonMessage)
			{
				this.HandleRemoveDragonMessage((RemoveDragonMessage)message);
				return;
			}
			if (message is KillDragonMessage)
			{
				this.HandleKillDragonMessage((KillDragonMessage)message);
				return;
			}
			if (message is RequestDragonMessage)
			{
				this.HandleRequestDragonMessage((RequestDragonMessage)message);
				return;
			}
			if (message is MigrateDragonMessage)
			{
				this.HandleMigrateDragonMessage((MigrateDragonMessage)message);
				return;
			}
			if (message is ExistingDragonMessage)
			{
				this.HandleExistingDragonMessage((ExistingDragonMessage)message);
				return;
			}
			if (message is EnemyGiveUpMessage)
			{
				this.HandleEnemyGiveUpMessage((EnemyGiveUpMessage)message);
			}
		}

		public bool DragonControlledLocally
		{
			get
			{
				return this._dragon != null;
			}
		}

		public void ResetFarthestDistance()
		{
			this.ClearedDistance = 50f;
			Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			worldPosition.Y = 0f;
			float num = worldPosition.Length();
			this.dragonDistanceIndex = Math.Min(this.dragonDistanceIndex, this.dragonDistances.Length);
			while (this.dragonDistanceIndex > 0 && num <= this.dragonDistances[this.dragonDistanceIndex - 1])
			{
				this.dragonDistanceIndex--;
			}
		}

		public float CalculateMidnight(float distance, float playerDepth)
		{
			if (playerDepth < -40f)
			{
				return 0.85f;
			}
			if (this.ZombieFestIsOn)
			{
				return 1f;
			}
			float num = BlockTerrain.Instance.PercentMidnight;
			float num2 = 1f - Math.Min(distance / 5000f, 1f);
			num2 *= 0.5f;
			num = Math.Max(0f, num - num2);
			num /= 1f - num2;
			return num.Clamp(0f, 0.79f);
		}

		public float CalculatePlayerDistance()
		{
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			float num = 0f;
			if (localPlayer != null)
			{
				Vector3 worldPosition = localPlayer.WorldPosition;
				worldPosition.Y = 0f;
				num = worldPosition.Length();
				if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
				{
					num += (CastleMinerZGame.Instance.GameScreen.Day - 0.41f) * 120f;
				}
			}
			return num;
		}

		public void RegisterGunShot(Vector3 location)
		{
			if (this._dragon != null)
			{
				this._dragon.RegisterGunshot(location);
			}
		}

		public float GetMinEnemySpawnTime(float d)
		{
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance || CastleMinerZGame.Instance.GameMode == GameModeTypes.DragonEndurance || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARD || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARDCORE)
			{
				return MathHelper.Lerp(5f, 1f, d / 3450f);
			}
			return MathHelper.Lerp(45f, 20f, d / 3450f);
		}

		public void ResetFelgardTimer()
		{
			EnemyManager._spawnFelgardTimer.Reset();
		}

		private void HandleSpawnEnemyMessage(SpawnEnemyMessage msg)
		{
			if (msg.EnemyTypeID == EnemyTypeEnum.FELGUARD)
			{
				EnemyManager._spawnFelgardTimer.Reset();
			}
			if (msg.EnemyTypeID == EnemyTypeEnum.ALIEN && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
			{
				CastleMinerZGame.Instance.PlayerStats.AlienEncounters++;
			}
			Player player = (Player)msg.Sender.Tag;
			if (!string.IsNullOrEmpty(msg.PlayerName))
			{
				player = this.GetNetworkPlayerByName(msg.PlayerName);
			}
			bool flag = false;
			if (player != null)
			{
				if (player.IsLocal)
				{
					flag = true;
				}
				else if (this._enemies.Count < 45)
				{
					flag = true;
				}
				else if (this._enemies.Count < 50)
				{
					this._enemyCounter++;
					if ((this._enemyCounter & 3) == 1)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					DebugUtils.Log("Spawn Failed. EnemyCount: " + this._enemies.Count);
					return;
				}
				BaseZombie baseZombie = new BaseZombie(this, msg.EnemyTypeID, player, msg.SpawnPosition, msg.EnemyID, msg.RandomSeed, msg.InitPkg);
				this.AddZombie(baseZombie);
				baseZombie.SpawnValue = msg.SpawnValue;
				DebugUtils.Log(string.Concat(new object[]
				{
					"Enemy ID ",
					msg.EnemyID,
					" ",
					msg.SpawnValue,
					" Name: ",
					msg.PlayerName,
					" actual: ",
					player.Gamer.Gamertag
				}));
				if (msg.SpawnerPosition != Vector3.Zero)
				{
					baseZombie.SpawnSource = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(IntVector3.FromVector3(msg.SpawnerPosition), false, BlockTypeEnum.Empty);
					baseZombie.SetDistanceLimit();
					return;
				}
			}
			else
			{
				DebugUtils.Log("Target Player is null.");
			}
		}

		public void ForceDragonSpawn(DragonTypeEnum dragonType)
		{
			this.SpawnDragon(dragonType, false);
		}

		private void SpawnDragon(DragonTypeEnum type, bool forBiome)
		{
			if (!this.DragonPending && this._dragonClient == null)
			{
				this.DragonPending = true;
				if (CastleMinerZGame.Instance.IsGameHost)
				{
					SpawnDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, CastleMinerZGame.Instance.LocalPlayer.Gamer.Id, type, forBiome, -1f);
					return;
				}
				RequestDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, type, forBiome);
			}
		}

		private void AskForDragon(bool forBiome, DragonTypeEnum dragonType)
		{
			if (CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime < this.NextDragonAllowedTime)
			{
				return;
			}
			if (this._dragon == null && this._dragonClient == null)
			{
				Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				worldPosition.Y = 0f;
				if (worldPosition.LengthSquared() > 12960000f)
				{
					dragonType = DragonTypeEnum.SKELETON;
				}
				if (dragonType != DragonTypeEnum.COUNT)
				{
					this.SpawnDragon(dragonType, forBiome);
				}
			}
		}

		private void CheckDragonBox(GameTime time)
		{
			if (CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidGamer)
			{
				if (this.InitializeDragonBox || this.DragonBox.Contains(CastleMinerZGame.Instance.LocalPlayer.WorldPosition) == ContainmentType.Disjoint)
				{
					this.NextTimedDragonType = DragonTypeEnum.FIRE;
					this.RecalculateDragonBox(time);
					if (this.InitializeDragonBox)
					{
						this.NextDragonAllowedTime = CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime + TimeSpan.FromSeconds(5.0);
					}
					this.InitializeDragonBox = false;
					return;
				}
				if (time.TotalGameTime > this.NextSpawnDragonTime)
				{
					this.NextSpawnDragonTime += TimeSpan.FromMinutes(1.0);
					Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
					Vector3 worldPosition = localPlayer.WorldPosition;
					worldPosition.Y = 0f;
					if (this.NextTimedDragonType < DragonTypeEnum.COUNT || worldPosition.LengthSquared() > 12960000f)
					{
						this.AskForDragon(false, this.NextTimedDragonType);
						this.NextTimedDragonType = (DragonTypeEnum)Math.Min((int)(this.NextTimedDragonType + 1), 5);
					}
				}
			}
		}

		private void RecalculateDragonBox(GameTime t)
		{
			Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			this.DragonBox = new BoundingBox(new Vector3(worldPosition.X - 500f, -100f, worldPosition.Z - 500f), new Vector3(worldPosition.X + 500f, 100f, worldPosition.Z + 500f));
			float num = 16f;
			switch (this.NextTimedDragonType)
			{
			case DragonTypeEnum.FIRE:
				num = 32f;
				break;
			case DragonTypeEnum.FOREST:
				num = 48f;
				break;
			case DragonTypeEnum.LIZARD:
				num = 64f;
				break;
			case DragonTypeEnum.ICE:
				num = 80f;
				break;
			case DragonTypeEnum.SKELETON:
				num = 80f;
				break;
			case DragonTypeEnum.COUNT:
				num = 80f;
				break;
			}
			this.NextSpawnDragonTime = t.TotalGameTime + TimeSpan.FromMinutes((double)(num + 16f * MathTools.RandomFloat(-0.25f, 0.25f)));
		}

		public void BroadcastExistingDragonMessage(byte newClientID)
		{
			if (this._dragon != null)
			{
				float num = -1f;
				if (this._dragonClient != null)
				{
					num = this._dragonClient.Health;
				}
				ExistingDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, newClientID, this._dragon.EType.EType, this._dragon.ForBiome, num);
			}
		}

		private void SetCurrentTimedDragonType(DragonTypeEnum dragonType)
		{
			if (dragonType != DragonTypeEnum.COUNT)
			{
				this.NextTimedDragonType = dragonType + 1;
			}
		}

		public void ApplyExplosiveDamageToDragon(Vector3 position, float damageAmount, byte shooterID, InventoryItemIDs itemID)
		{
			if (this._dragonClient != null)
			{
				this._dragonClient.TakeExplosiveDamage(position, damageAmount, shooterID, itemID);
			}
		}

		private void HandleExistingDragonMessage(ExistingDragonMessage msg)
		{
			if (CastleMinerZGame.Instance.IsLocalPlayerId(msg.NewClientID))
			{
				this.DragonPending = false;
				if (this._dragonClient != null)
				{
					this._dragonClient.RemoveFromParent();
				}
				this._dragonClient = new DragonClientEntity(msg.EnemyTypeID, msg.CurrentHealth);
				if (!msg.ForBiome)
				{
					this.SetCurrentTimedDragonType(msg.EnemyTypeID);
				}
				Scene scene = base.Scene;
				if (scene != null && scene.Children != null)
				{
					scene.Children.Add(this._dragonClient);
				}
				this.RecalculateDragonBox(CastleMinerZGame.Instance.CurrentGameTime);
			}
		}

		private void HandleSpawnDragonMessage(SpawnDragonMessage msg)
		{
			this.DragonPending = false;
			Scene scene = base.Scene;
			if (CastleMinerZGame.Instance.IsLocalPlayerId(msg.SpawnerID))
			{
				this._dragon = new DragonEntity(msg.EnemyTypeID, msg.ForBiome, null);
				if (scene != null && scene.Children != null)
				{
					scene.Children.Add(this._dragon);
				}
			}
			if (this._dragonClient != null)
			{
				this._dragonClient.RemoveFromParent();
			}
			this._dragonClient = new DragonClientEntity(msg.EnemyTypeID, msg.Health);
			if (!msg.ForBiome)
			{
				this.SetCurrentTimedDragonType(msg.EnemyTypeID);
			}
			if (scene != null && scene.Children != null)
			{
				scene.Children.Add(this._dragonClient);
			}
			this.RecalculateDragonBox(CastleMinerZGame.Instance.CurrentGameTime);
		}

		private void HandleRequestDragonMessage(RequestDragonMessage msg)
		{
			if (CastleMinerZGame.Instance.IsGameHost && !this.DragonPending)
			{
				this.DragonPending = true;
				SpawnDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, msg.Sender.Id, msg.EnemyTypeID, msg.ForBiome, -1f);
			}
		}

		private void HandleMigrateDragonMessage(MigrateDragonMessage msg)
		{
			if (this._dragonClient != null && CastleMinerZGame.Instance.IsLocalPlayerId(msg.TargetID))
			{
				this._dragon = new DragonEntity(msg.MigrationInfo.EType, msg.MigrationInfo.ForBiome, msg.MigrationInfo);
				Scene scene = base.Scene;
				if (scene != null && scene.Children != null)
				{
					scene.Children.Add(this._dragon);
				}
			}
		}

		public void MigrateDragon(Player target, DragonHostMigrationInfo miginfo)
		{
			if (CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidGamer)
			{
				MigrateDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, target.Gamer.Id, miginfo);
			}
			this._dragon.RemoveFromParent();
			this._dragon = null;
		}

		public void DragonHasBeenHit()
		{
			if (this._dragon != null)
			{
				this._dragon.ChancesToNotAttack = 0;
			}
		}

		public void RemoveDragonEntity()
		{
			if (this._dragon != null)
			{
				this._dragon.RemoveFromParent();
				this._dragon = null;
			}
			if (this._dragonClient != null)
			{
				if (CastleMinerZGame.Instance.GameMode == GameModeTypes.DragonEndurance)
				{
					if (CastleMinerZGame.Instance.IsGameHost)
					{
						this._nextEnduranceDragonTime = CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime + TimeSpan.FromSeconds((double)MathTools.RandomFloat(10f, 30f));
					}
				}
				else
				{
					this.NextDragonAllowedTime = CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime + TimeSpan.FromSeconds(8.0);
					this.RecalculateDragonBox(CastleMinerZGame.Instance.CurrentGameTime);
				}
				this._dragonClient.RemoveFromParent();
				this._dragonClient = null;
			}
		}

		public void HandleRemoveDragonMessage(RemoveDragonMessage msg)
		{
			this.RemoveDragonEntity();
		}

		public void SpawnDragonPickups(Vector3 location)
		{
			int num = MathTools.RandomInt(1, 4) + MathTools.RandomInt(1, 5);
			float num2 = location.Length();
			float num3 = (num2 / 5000f).Clamp(0f, 1f);
			int num4 = MathTools.RandomInt(1, 3 + (int)(num3 * 5f)) + MathTools.RandomInt(1, 3 + (int)(num3 * 5f));
			for (int i = 0; i < num4; i++)
			{
				InventoryItem inventoryItem = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1);
				PickupManager.Instance.CreateUpwardPickup(inventoryItem, location + new Vector3(0f, 1f, 0f), 3f, false);
			}
			for (int j = 0; j < num; j++)
			{
				float num5 = MathTools.RandomFloat(num3, 1f);
				float y = base.LocalPosition.Y;
				InventoryItem inventoryItem2;
				if ((double)num5 < 0.5)
				{
					inventoryItem2 = InventoryItem.CreateItem(InventoryItemIDs.Copper, 1);
				}
				else if ((double)num5 < 0.6)
				{
					inventoryItem2 = InventoryItem.CreateItem(InventoryItemIDs.Iron, 1);
				}
				else if ((double)num5 < 0.8)
				{
					inventoryItem2 = InventoryItem.CreateItem(InventoryItemIDs.Gold, 1);
				}
				else if ((double)num5 < 0.9)
				{
					inventoryItem2 = InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1);
				}
				else
				{
					inventoryItem2 = InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 1);
				}
				if (inventoryItem2 != null)
				{
					PickupManager.Instance.CreateUpwardPickup(inventoryItem2, location + new Vector3(0f, 1f, 0f), 3f, false);
				}
			}
		}

		public void HandleKillDragonMessage(KillDragonMessage msg)
		{
			if (this._dragon != null)
			{
				this._dragon.RemoveFromParent();
				this._dragon = null;
			}
			if (this._dragonClient != null && !this._dragonClient.Dead)
			{
				if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
				{
					switch (this._dragonClient.EType.EType)
					{
					case DragonTypeEnum.FIRE:
						CastleMinerZGame.Instance.PlayerStats.FireDragonKills++;
						break;
					case DragonTypeEnum.FOREST:
						CastleMinerZGame.Instance.PlayerStats.ForestDragonKills++;
						break;
					case DragonTypeEnum.LIZARD:
						CastleMinerZGame.Instance.PlayerStats.SandDragonKills++;
						break;
					case DragonTypeEnum.ICE:
						CastleMinerZGame.Instance.PlayerStats.IceDragonKills++;
						break;
					case DragonTypeEnum.SKELETON:
						CastleMinerZGame.Instance.PlayerStats.UndeadDragonKills++;
						break;
					}
				}
				else if (CastleMinerZGame.Instance.GameMode == GameModeTypes.DragonEndurance && CastleMinerZGame.Instance.IsGameHost)
				{
					this._nextEnduranceDragonHealth += -15f;
				}
				if (CastleMinerZGame.Instance.IsLocalPlayerId(msg.KillerID))
				{
					if (CastleMinerZGame.Instance.IsOnlineGame)
					{
						string text = string.Concat(new string[]
						{
							CastleMinerZGame.Instance.LocalPlayer.Gamer.Gamertag,
							" ",
							Strings.Has_Killed_The,
							" ",
							this._dragonClient.EType.GetDragonName()
						});
						BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, text);
					}
					CastleMinerZGame.Instance.PlayerStats.GetItemStats(msg.WeaponID);
					if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
					{
						CastleMinerZGame.Instance.PlayerStats.TotalKills++;
					}
					this._dragonClient.Kill(true);
					return;
				}
				this._dragonClient.Kill(false);
			}
		}

		public void HandleUpdateDragonMessage(UpdateDragonMessage msg)
		{
			if (this._dragonClient != null)
			{
				this._dragonClient.HandleUpdateDragonMessage(msg);
			}
		}

		public void HandleDragonAttackMessage(DragonAttackMessage msg)
		{
			if (this._dragonClient != null)
			{
				this._dragonClient.HandleDragonAttackMessage(msg);
			}
		}

		public void RemoveDragon()
		{
			RemoveDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer);
		}

		public void AddFireball(FireballEntity fb)
		{
			this._fireballs.Add(fb);
		}

		public void RemoveFireball(FireballEntity fb)
		{
			this._fireballs.Remove(fb);
			fb.RemoveFromParent();
		}

		public void HandleDetonateFireballMessage(DetonateFireballMessage msg)
		{
			for (int i = 0; i < this._fireballs.Count; i++)
			{
				if (this._fireballs[i].FireballIndex == msg.Index)
				{
					this._fireballs[i].Detonate(msg.Location);
					break;
				}
			}
			DragonType dragonType = DragonType.GetDragonType(msg.EType);
			if (CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				worldPosition.Y += 1f;
				float num = Vector3.DistanceSquared(worldPosition, msg.Location);
				if (num < 25f)
				{
					float num2 = Math.Max((float)Math.Sqrt((double)num) - 1f, 0f);
					this._damageLOSProbe.Init(msg.Location, worldPosition);
					this._damageLOSProbe.DragonTypeIndex = (int)dragonType.EType;
					BlockTerrain.Instance.Trace(this._damageLOSProbe);
					float num3 = Math.Min(this._damageLOSProbe.TotalDamageMultiplier * (1f - num2 / 5f), 1f);
					if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Survival && CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.EASY)
					{
						num3 *= 1f;
					}
					num3 *= dragonType.FireballDamage;
					InGameHUD.Instance.ApplyDamage(num3, msg.Location);
				}
			}
			DragonDamageType damageType = dragonType.DamageType;
			BlockTypeEnum blockTypeEnum;
			if (damageType == DragonDamageType.ICE)
			{
				blockTypeEnum = BlockTypeEnum.Ice;
			}
			else
			{
				blockTypeEnum = BlockTypeEnum.Empty;
			}
			for (byte b = 0; b < msg.NumBlocks; b += 1)
			{
				BlockTerrain.Instance.SetBlock(msg.BlocksToRemove[(int)b], blockTypeEnum);
			}
		}

		private int RememberDependentObjects(IntVector3 worldIndex, int numDependents)
		{
			for (BlockFace blockFace = BlockFace.POSX; blockFace < BlockFace.NUM_FACES; blockFace++)
			{
				IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(worldIndex, blockFace);
				BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(neighborIndex);
				if (BlockType.GetType(blockWithChanges).Facing == blockFace)
				{
					this._dependentItemsToRemoveBuffer[numDependents] = neighborIndex;
					this._dependItemTypes[numDependents++] = blockWithChanges;
				}
			}
			return numDependents;
		}

		private bool VectorWillBeDamaged(IntVector3 tester, int numBlocks)
		{
			for (int i = 0; i < numBlocks; i++)
			{
				if (this._fireballDamageBuffer[i] == tester)
				{
					return true;
				}
			}
			return false;
		}

		public void DetonateFireball(Vector3 position, int index, DragonType dragonType)
		{
			Vector3 vector = new Vector3((float)Math.Floor((double)position.X) + 0.5f, (float)Math.Floor((double)position.Y) + 0.5f, (float)Math.Floor((double)position.Z) + 0.5f);
			Vector3 zero = Vector3.Zero;
			Vector3 zero2 = Vector3.Zero;
			int num = 0;
			int num2 = 0;
			IntVector3 zero3 = IntVector3.Zero;
			DragonTypeEnum etype = dragonType.EType;
			bool flag = dragonType.DamageType == DragonDamageType.DESTRUCTION;
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance || CastleMinerZGame.Instance.GameMode == GameModeTypes.DragonEndurance || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARD || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARDCORE)
			{
				zero.X = -3f;
				while (zero.X <= 3f)
				{
					zero2.X = zero.X + vector.X;
					zero.Y = -3f;
					while (zero.Y <= 3f)
					{
						zero2.Y = zero.Y + vector.Y;
						zero.Z = -3f;
						while (zero.Z <= 3f)
						{
							zero2.Z = zero.Z + vector.Z;
							if (Vector3.DistanceSquared(zero2, position) <= 9f)
							{
								IntVector3 intVector = (IntVector3)zero2;
								IntVector3 localIndex = BlockTerrain.Instance.GetLocalIndex(intVector);
								if (BlockTerrain.Instance.IsIndexValid(localIndex))
								{
									BlockTypeEnum typeIndex = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(localIndex));
									if (!DragonType.BreakLookup[(int)etype, (int)typeIndex] && !BlockType.IsUpperDoor(typeIndex))
									{
										this._fireballDamageItemTypes[num] = typeIndex;
										this._fireballDamageBuffer[num++] = intVector;
										if (flag)
										{
											num2 = this.RememberDependentObjects(intVector, num2);
										}
										if (typeIndex == BlockTypeEnum.NormalLowerDoorOpenX || typeIndex == BlockTypeEnum.NormalLowerDoorOpenZ || typeIndex == BlockTypeEnum.NormalLowerDoorClosedX || typeIndex == BlockTypeEnum.NormalLowerDoorClosedZ)
										{
											intVector.Y++;
											this._fireballDamageItemTypes[num] = BlockTypeEnum.NormalUpperDoorOpen;
											this._fireballDamageBuffer[num++] = intVector;
											if (flag)
											{
												num2 = this.RememberDependentObjects(intVector, num2);
											}
										}
										if (typeIndex == BlockTypeEnum.StrongLowerDoorOpenX || typeIndex == BlockTypeEnum.StrongLowerDoorOpenZ || typeIndex == BlockTypeEnum.StrongLowerDoorClosedX || typeIndex == BlockTypeEnum.StrongLowerDoorClosedZ)
										{
											intVector.Y++;
											this._fireballDamageItemTypes[num] = BlockTypeEnum.StrongUpperDoorOpen;
											this._fireballDamageBuffer[num++] = intVector;
											if (flag)
											{
												num2 = this.RememberDependentObjects(intVector, num2);
											}
										}
									}
								}
							}
							zero.Z += 1f;
						}
						zero.Y += 1f;
					}
					zero.X += 1f;
				}
			}
			int num3 = num;
			for (int i = 0; i < num2; i++)
			{
				if (!this.VectorWillBeDamaged(this._dependentItemsToRemoveBuffer[i], num3))
				{
					InventoryItem.InventoryItemClass inventoryItemClass = BlockInventoryItemClass.BlockClasses[BlockType.GetType(this._dependItemTypes[i]).ParentBlockType];
					PickupManager.Instance.CreatePickup(inventoryItemClass.CreateItem(1), IntVector3.ToVector3(this._dependentItemsToRemoveBuffer[i]) + new Vector3(0.5f, 0.5f, 0.5f), false, false);
					this._fireballDamageItemTypes[num] = this._dependItemTypes[i];
					this._fireballDamageBuffer[num++] = this._dependentItemsToRemoveBuffer[i];
					if (this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorOpenX || this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorOpenZ || this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorClosedX || this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorClosedZ)
					{
						this._fireballDamageItemTypes[num] = BlockTypeEnum.NormalUpperDoorOpen;
						this._fireballDamageBuffer[num++] = this._dependentItemsToRemoveBuffer[i] + new IntVector3(0, 1, 0);
					}
				}
			}
			for (int j = 0; j < num3; j++)
			{
				if (BlockType.IsContainer(this._fireballDamageItemTypes[j]))
				{
					DestroyCrateMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this._fireballDamageBuffer[j]);
					Crate crate;
					if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(this._fireballDamageBuffer[j], out crate))
					{
						crate.EjectContents();
					}
				}
				if (BlockType.IsDoor(this._fireballDamageItemTypes[j]))
				{
					DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, this._fireballDamageBuffer[j], this._fireballDamageItemTypes[j]);
				}
			}
			DetonateFireballMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, position, index, num, this._fireballDamageBuffer, dragonType.EType);
		}

		public void HandleEnemyGiveUpMessage(EnemyGiveUpMessage msg)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				BaseZombie baseZombie = this._enemies[i];
				if (!baseZombie.IsDead && (int)baseZombie.Target.Gamer.Id == msg.TargetID && baseZombie.EnemyID == msg.EnemyID)
				{
					baseZombie.GiveUp();
					if (baseZombie.SpawnSource != null)
					{
						baseZombie.SpawnSource.HandleEnemyRemoved(baseZombie.SpawnValue);
						baseZombie.SpawnSource = null;
					}
					return;
				}
			}
		}

		public void HandleKillEnemyMessage(KillEnemyMessage msg)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				BaseZombie baseZombie = this._enemies[i];
				if (!baseZombie.IsDead && (int)baseZombie.Target.Gamer.Id == msg.TargetID && baseZombie.EnemyID == msg.EnemyID)
				{
					if (CastleMinerZGame.Instance.IsLocalPlayerId(msg.KillerID))
					{
						CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(msg.WeaponID);
						baseZombie.CreatePickup();
						CastleMinerZGame.Instance.PlayerStats.AddStat(baseZombie.EType.Category);
						itemStats.AddStat(baseZombie.EType.Category);
						if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
						{
							CastleMinerZGame.Instance.PlayerStats.TotalKills++;
						}
					}
					if (baseZombie.SpawnSource != null)
					{
						baseZombie.SpawnSource.HandleEnemyDefeated(baseZombie.SpawnValue, msg.KillerID);
						baseZombie.SpawnSource = null;
					}
					baseZombie.Kill();
					return;
				}
			}
		}

		public void HandleSpeedUpEnemyMessage(SpeedUpEnemyMessage msg)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				if ((int)this._enemies[i].Target.Gamer.Id == msg.TargetID && this._enemies[i].EnemyID == msg.EnemyID)
				{
					if (!this._enemies[i].IsMovingFast && !this._enemies[i].IsDead)
					{
						this._enemies[i].SpeedUp();
					}
					return;
				}
			}
		}

		public IShootableEnemy Trace(TraceProbe tp, bool meleeWeapon)
		{
			IShootableEnemy shootableEnemy = null;
			BlockTerrain.Instance.Trace(tp);
			if (this._enemies.Count != 0)
			{
				BaseZombie baseZombie = null;
				if (tp._collides)
				{
					this.zombieProbe.Init(tp._start, tp.GetIntersection());
				}
				else
				{
					this.zombieProbe.Init(tp._start, tp._end);
				}
				Vector3 vector = this.zombieProbe._end - this.zombieProbe._start;
				if (vector.LengthSquared() <= 0.0001f)
				{
					return baseZombie;
				}
				vector.Normalize();
				float num = Vector3.Dot(this.zombieProbe._start, vector);
				float num2 = Vector3.Dot(this.zombieProbe._end, vector);
				float num3 = this.zombieProbe._inT;
				foreach (BaseZombie baseZombie2 in this._enemies)
				{
					if (baseZombie2.IsHittable && tp.TestThisEnemy(baseZombie2))
					{
						Vector3 worldPosition = baseZombie2.WorldPosition;
						float num4 = Vector3.Dot(worldPosition, vector);
						if (num - num4 <= 3f && num4 - num2 <= 3f)
						{
							Vector3 vector2 = worldPosition - this.zombieProbe._start;
							Vector3 vector3 = Vector3.Cross(vector, vector2);
							if (vector3.LengthSquared() > 0.0001f)
							{
								vector3.Normalize();
								vector3 = Vector3.Cross(vector3, vector);
								if (Math.Abs(Vector3.Dot(worldPosition, vector3) - Vector3.Dot(this.zombieProbe._start, vector3)) > 3f)
								{
									continue;
								}
							}
							BoundingBox playerAABB = baseZombie2.PlayerAABB;
							playerAABB.Min += worldPosition;
							playerAABB.Max += worldPosition;
							this.zombieProbe.TestBoundBox(playerAABB);
							if (this.zombieProbe._collides && num3 != this.zombieProbe._inT)
							{
								float chanceOfBulletStrike = baseZombie2.EType.ChanceOfBulletStrike;
								Vector3 intersection = this.zombieProbe.GetIntersection();
								bool flag = chanceOfBulletStrike == 1f || meleeWeapon || baseZombie2.IsHeadshot(intersection) || (float)this._rnd.NextDouble() <= chanceOfBulletStrike;
								if (flag)
								{
									baseZombie = baseZombie2;
									num2 = Vector3.Dot(intersection, vector);
									num3 = this.zombieProbe._inT;
								}
							}
						}
					}
				}
				if (baseZombie != null)
				{
					tp._collides = true;
					tp._end = this.zombieProbe._end;
					tp._inT = this.zombieProbe._inT;
					tp._inNormal = this.zombieProbe._inNormal;
					tp._inFace = this.zombieProbe._inFace;
					shootableEnemy = baseZombie;
				}
			}
			if (shootableEnemy == null && !tp._collides && this._dragonClient != null)
			{
				tp.Reset();
				if (this._dragonClient.Trace(tp))
				{
					shootableEnemy = this._dragonClient;
				}
			}
			return shootableEnemy;
		}

		public void AddZombie(BaseZombie z)
		{
			this._enemies.Add(z);
			Scene scene = base.Scene;
			if (scene != null && scene.Children != null)
			{
				scene.Children.Add(z);
			}
		}

		public void RemoveZombie(BaseZombie z)
		{
			this._enemies.Remove(z);
			if (z.Target == CastleMinerZGame.Instance.LocalPlayer)
			{
				if (z.EType.EType == EnemyTypeEnum.ALIEN)
				{
					this._localAlienCount--;
				}
				else if (z.EType.FoundIn == EnemyType.FoundInEnum.ABOVEGROUND)
				{
					this._localSurfaceEnemyCount--;
				}
				else
				{
					this._localCaveEnemyCount--;
				}
			}
			z.RemoveFromParent();
		}

		public bool TouchesZombies(BoundingBox box)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				if (this._enemies[i].Touches(box))
				{
					return true;
				}
			}
			return false;
		}

		private int MakeNextEnemyID()
		{
			return this._nextLocalEnemyID++;
		}

		private void SpawnRandomZombies(Vector3 plrpos)
		{
			Vector3 vector = plrpos;
			vector.Y += 1f;
			float num = this.CalculatePlayerDistance();
			float num2 = this.CalculateMidnight(num, plrpos.Y);
			Vector3 vector2 = CastleMinerZGame.Instance.LocalPlayer.PlayerPhysics.WorldVelocity;
			vector2 *= 5f;
			EnemyTypeEnum zombie = EnemyType.GetZombie(num);
			int spawnRadius = EnemyType.Types[(int)zombie].SpawnRadius;
			vector.X += vector2.X + (float)this._rnd.Next(-spawnRadius, spawnRadius + 1);
			vector.Z += vector2.Z + (float)this._rnd.Next(-spawnRadius, spawnRadius + 1);
			if (BlockTerrain.Instance.RegionIsLoaded(vector))
			{
				vector = BlockTerrain.Instance.FindTopmostGroundLocation(vector);
				IntVector3 intVector = new IntVector3((int)vector.X, (int)vector.Y - 1, (int)vector.Z);
				int num3 = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(intVector);
				int num4 = BlockTerrain.Instance._blocks[num3];
				BlockType type = Block.GetType(num4);
				this._distanceEnemiesLeftToSpawn--;
				if (type._type == BlockTypeEnum.SpaceRock || type._type == BlockTypeEnum.SpaceRockInventory)
				{
					return;
				}
				SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, vector, zombie, num2, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
				if (zombie == EnemyTypeEnum.ALIEN)
				{
					this._localAlienCount++;
					return;
				}
				this._localSurfaceEnemyCount++;
			}
		}

		private Player GetNetworkPlayerByName(string name)
		{
			for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null && name.Equals(networkGamer.Gamertag))
				{
					return (Player)networkGamer.Tag;
				}
			}
			return null;
		}

		public int SpawnEnemy(Vector3 newpos, EnemyTypeEnum etype, Vector3 spawnerPos, int spawnValue, string targetPlayerName = null)
		{
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			float num = 0f;
			int num2 = this.MakeNextEnemyID();
			SpawnEnemyMessage.Send((LocalNetworkGamer)localPlayer.Gamer, newpos, etype, num, num2, this._rnd.Next(), spawnerPos, spawnValue, targetPlayerName);
			if (etype == EnemyTypeEnum.ALIEN)
			{
				this._localAlienCount++;
			}
			else
			{
				this._localSurfaceEnemyCount++;
			}
			if (etype == EnemyTypeEnum.HELL_LORD)
			{
				BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, Strings.Hell_Lord_Spawned);
			}
			return num2;
		}

		private void SpawnAbovegroundEnemy(Vector3 plrpos)
		{
			Vector3 vector = plrpos;
			vector.Y += 1f;
			float num = this.CalculatePlayerDistance();
			float num2 = this.CalculateMidnight(num, plrpos.Y);
			if (num2 <= 0.0001f)
			{
				this._timeSinceLastSurfaceEnemy = 0f;
				return;
			}
			float num3 = MathHelper.Lerp(60f, this.GetMinEnemySpawnTime(num), (float)Math.Pow((double)num2, 0.25));
			if (this._timeSinceLastSurfaceEnemy > num3 * (1f + (float)this._rnd.NextDouble() * 0.5f))
			{
				Vector3 vector2 = CastleMinerZGame.Instance.LocalPlayer.PlayerPhysics.WorldVelocity;
				vector2 *= 5f;
				EnemyTypeEnum abovegroundEnemy = EnemyType.GetAbovegroundEnemy(num2, num);
				int spawnRadius = EnemyType.Types[(int)abovegroundEnemy].SpawnRadius;
				vector.X += vector2.X + (float)this._rnd.Next(-spawnRadius, spawnRadius + 1);
				vector.Z += vector2.Z + (float)this._rnd.Next(-spawnRadius, spawnRadius + 1);
				if (BlockTerrain.Instance.RegionIsLoaded(vector))
				{
					if (plrpos.Y > -40f)
					{
						vector = BlockTerrain.Instance.FindTopmostGroundLocation(vector);
					}
					else
					{
						vector = BlockTerrain.Instance.FindSafeStartLocation(vector);
					}
					IntVector3 intVector = new IntVector3((int)vector.X, (int)vector.Y - 1, (int)vector.Z);
					int num4 = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(intVector);
					int num5 = BlockTerrain.Instance._blocks[num4];
					BlockType type = Block.GetType(num5);
					Vector3 vector3 = new Vector3(vector.X, vector.Y + 0.5f, vector.Z);
					float simpleTorchlightAtPoint = BlockTerrain.Instance.GetSimpleTorchlightAtPoint(vector3);
					if (simpleTorchlightAtPoint < 0.4f || !ItemBlockEntityManager.Instance.NearLantern(vector3, 7.2f))
					{
						this._timeSinceLastSurfaceEnemy = 0f;
						if (type._type == BlockTypeEnum.SpaceRock || type._type == BlockTypeEnum.SpaceRockInventory)
						{
							return;
						}
						SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, vector, abovegroundEnemy, num2, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
						this._localSurfaceEnemyCount++;
					}
				}
			}
		}

		private void SpawnBelowgroundEnemy(Vector3 plrpos, float gametime)
		{
			Vector3 vector = plrpos;
			vector.Y += 1f;
			int num = (int)(-(plrpos.Y - 20f)).Clamp(0f, 50f);
			float num2 = (float)num / 50f;
			float num3 = (float)Math.Sin((double)(gametime / 60f % 2f * 3.1415927f));
			if (num3 > 0f)
			{
				num3 = (float)Math.Sqrt((double)num3);
			}
			else
			{
				num3 = 0f;
			}
			num2 *= num3;
			float num4 = ((plrpos.Y < -40f) ? 3500f : this.CalculatePlayerDistance());
			float num5 = MathHelper.Lerp(60f, this.GetMinEnemySpawnTime(num4), num2);
			if (this._timeSinceLastCaveEnemy > num5 * (1f + (float)this._rnd.NextDouble() * 0.5f))
			{
				EnemyTypeEnum belowgroundEnemy = EnemyType.GetBelowgroundEnemy((float)num, num4);
				int spawnRadius = EnemyType.Types[(int)belowgroundEnemy].SpawnRadius;
				int num6 = this._rnd.Next(-spawnRadius, spawnRadius);
				if (num6 <= 0)
				{
					num6 -= 5;
				}
				else
				{
					num6 += 5;
				}
				vector.X += (float)num6;
				num6 = this._rnd.Next(-spawnRadius, spawnRadius);
				if (num6 <= 0)
				{
					num6 -= 5;
				}
				else
				{
					num6 += 5;
				}
				vector.Z += (float)num6;
				if (BlockTerrain.Instance.RegionIsLoaded(vector))
				{
					vector = BlockTerrain.Instance.FindClosestCeiling(vector);
					if (vector.LengthSquared() != 0f)
					{
						IntVector3 intVector = new IntVector3((int)vector.X, (int)vector.Y, (int)vector.Z);
						int num7 = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(intVector);
						int num8 = BlockTerrain.Instance._blocks[num7];
						BlockType type = Block.GetType(num8);
						Vector3 vector2 = vector;
						vector2.Y -= 1f;
						Vector2 simpleLightAtPoint = BlockTerrain.Instance.GetSimpleLightAtPoint(vector2);
						if (simpleLightAtPoint.X <= 0.4f && simpleLightAtPoint.Y <= 0.4f)
						{
							this._timeSinceLastCaveEnemy = 0f;
							if (type._type == BlockTypeEnum.SpaceRock || type._type == BlockTypeEnum.SpaceRockInventory)
							{
								return;
							}
							SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, vector, belowgroundEnemy, 0f, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
							this._localCaveEnemyCount++;
						}
					}
				}
			}
		}

		private void SetNextAlienSpawnTime()
		{
			if (!this._aliensAreAroused)
			{
				this._nextAlienTime = MathTools.RandomFloat(10f, 20f);
				return;
			}
			this._nextAlienTime = MathTools.RandomFloat(3f, 7f);
		}

		private void SpawnAlien(Vector3 plrpos, bool inAsteroid, float gametime)
		{
			if (this._timeSinceLastAlien > this._nextAlienTime)
			{
				EnemyTypeEnum enemyTypeEnum = EnemyTypeEnum.ALIEN;
				int spawnRadius = EnemyType.Types[(int)enemyTypeEnum].SpawnRadius;
				Vector3 vector = Vector3.Zero;
				if (this._aliensAreAroused && BlockTerrain.Instance.RegionIsLoaded(plrpos))
				{
					vector = BlockTerrain.Instance.FindNearbySpawnPoint(plrpos, spawnRadius * 2, spawnRadius);
				}
				else
				{
					int num;
					int num2;
					if (!inAsteroid)
					{
						num = spawnRadius * 3;
						num2 = 5;
					}
					else
					{
						num = spawnRadius;
						num2 = 1;
					}
					vector = plrpos;
					vector.Y += 1f;
					int num3 = this._rnd.Next(-num, num);
					if (num3 <= 0)
					{
						num3 -= num2;
					}
					else
					{
						num3 += num2;
					}
					vector.X += (float)num3;
					num3 = this._rnd.Next(-num, num);
					if (num3 <= 0)
					{
						num3 -= num2;
					}
					else
					{
						num3 += num2;
					}
					vector.Z += (float)num3;
					if (BlockTerrain.Instance.RegionIsLoaded(vector))
					{
						vector = BlockTerrain.Instance.FindAlienSpawnPoint(vector, this._closestSpaceRock > (float)spawnRadius);
					}
					else
					{
						vector = Vector3.Zero;
					}
				}
				if (vector.LengthSquared() != 0f)
				{
					Vector3 vector2 = new Vector3(vector.X, vector.Y + 0.5f, vector.Z);
					IntVector3 intVector = new IntVector3((int)vector.X, (int)vector.Y - 1, (int)vector.Z);
					int num4 = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(intVector);
					BlockType blockType = Block.GetType(BlockTerrain.Instance._blocks[num4]);
					if (!blockType.BlockPlayer)
					{
						num4--;
						blockType = Block.GetType(BlockTerrain.Instance._blocks[num4]);
					}
					if (blockType.ParentBlockType == BlockTypeEnum.SpaceRock || this._closestSpaceRock > (float)(spawnRadius * 2))
					{
						this._timeSinceLastAlien = 0f;
						this.SetNextAlienSpawnTime();
						SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, vector2, enemyTypeEnum, 0f, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
						this._localAlienCount++;
					}
				}
			}
		}

		private void SpawnTestEnemy(Vector3 plrpos)
		{
			BaseZombie baseZombie = new BaseZombie(this, EnemyTypeEnum.ALIEN, CastleMinerZGame.Instance.LocalPlayer, plrpos, 52, 1, EnemyType.Types[52].CreateInitPackage(0.5f));
			this.AddZombie(baseZombie);
		}

		public float AttentuateVelocity(Player plr, Vector3 fwd, Vector3 worldPos)
		{
			float num = 1f;
			for (int i = 0; i < this._enemies.Count; i++)
			{
				if (this._enemies[i].Target == plr && this._enemies[i].IsBlocking)
				{
					Vector3 vector = this._enemies[i].WorldPosition - worldPos;
					float num2 = vector.LengthSquared();
					float num3 = 1f;
					if (num2 < 4f && Math.Abs(vector.Y) < 1.5f)
					{
						num3 = 0.5f;
						if (num2 > 0.0001f)
						{
							Vector3 vector2 = Vector3.Normalize(vector);
							float num4 = Vector3.Dot(vector2, fwd);
							if (num4 > 0f)
							{
								num3 *= Math.Min(1f, 2f * (1f - num4));
							}
						}
					}
					num *= num3;
				}
			}
			return num;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.NOENEMIES)
			{
				return;
			}
			this.AddToSoundLevel((float)gameTime.ElapsedGameTime.TotalSeconds * -2f);
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			EnemyManager._spawnFelgardTimer.Update(gameTime.ElapsedGameTime);
			if (localPlayer != null && this._enemies.Count < 50)
			{
				this._timeSinceLastSurfaceEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
				this._timeSinceLastCaveEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
				Vector3 worldPosition = localPlayer.WorldPosition;
				Vector3 vector = worldPosition;
				vector.Y += 1f;
				float simpleSunlightAtPoint = BlockTerrain.Instance.GetSimpleSunlightAtPoint(vector);
				Vector2 vector2 = new Vector2(worldPosition.X, worldPosition.Z);
				float num = vector2.Length();
				if (CastleMinerZGame.Instance.GameMode != GameModeTypes.DragonEndurance)
				{
					if (CastleMinerZGame.Instance.IsGameHost && this.dragonDistanceIndex < this.dragonDistances.Length && num > this.dragonDistances[this.dragonDistanceIndex])
					{
						this.AskForDragon(true, (DragonTypeEnum)this.dragonDistanceIndex);
						this.dragonDistanceIndex++;
					}
					this.CheckDragonBox(gameTime);
					bool flag = BlockTerrain.Instance.PercentMidnight > 0.9f;
					if (flag)
					{
						if (!this.ZombieFestIsOn)
						{
							if (this._timeLeftTilFrenzy == -1f)
							{
								this._timeLeftTilFrenzy = 3f;
							}
							else if (this._timeLeftTilFrenzy > 0f)
							{
								this._timeLeftTilFrenzy -= (float)gameTime.ElapsedGameTime.TotalSeconds;
								if (this._timeLeftTilFrenzy < 0f)
								{
									this._timeLeftTilFrenzy = -1f;
									this.ZombieFestIsOn = true;
								}
							}
						}
					}
					else
					{
						this.ZombieFestIsOn = false;
					}
					if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARD || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARDCORE)
					{
						if (num > this.ClearedDistance)
						{
							this._timeToFirstContact = -1f;
							if (simpleSunlightAtPoint > 0.01f)
							{
								if (this._distanceEnemiesLeftToSpawn == 0)
								{
									this._nextDistanceEnemyTimer = MathTools.RandomFloat(2f);
								}
								this._distanceEnemiesLeftToSpawn += MathTools.RandomInt(2, 5);
							}
							float num2 = (float)Math.Floor(1.5 + (double)(num / 40f)) * 40f;
							this.ClearedDistance = MathTools.RandomFloat(num2 - 10f, num2 + 10f);
						}
						if (this._timeToFirstContact > 0f)
						{
							this._timeToFirstContact -= (float)gameTime.ElapsedGameTime.TotalSeconds;
							if (this._timeToFirstContact < 0f)
							{
								this._nextDistanceEnemyTimer = MathTools.RandomFloat(2f);
								this._distanceEnemiesLeftToSpawn += MathTools.RandomInt(2, 5);
							}
						}
						if ((float)this._distanceEnemiesLeftToSpawn > 0f)
						{
							if (CastleMinerZGame.Instance.LocalPlayer.Dead)
							{
								this._distanceEnemiesLeftToSpawn = 0;
							}
							else
							{
								this._nextDistanceEnemyTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
								if (this._nextDistanceEnemyTimer <= 0f)
								{
									this.SpawnRandomZombies(worldPosition);
									if (this._distanceEnemiesLeftToSpawn > 0)
									{
										this._nextDistanceEnemyTimer += MathTools.RandomFloat(2f, 5f);
									}
								}
							}
						}
					}
				}
				else if (CastleMinerZGame.Instance.IsGameHost && !this.DragonPending && this._dragonClient == null && gameTime.TotalGameTime > this._nextEnduranceDragonTime)
				{
					this.DragonPending = true;
					SpawnDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, CastleMinerZGame.Instance.LocalPlayer.Gamer.Id, this._nextEnduranceDragonType, false, this._nextEnduranceDragonHealth);
					this._nextEnduranceDragonHealth += 100f;
					this._nextEnduranceDragonType++;
					if (this._nextEnduranceDragonType == DragonTypeEnum.COUNT)
					{
						this._nextEnduranceDragonType = DragonTypeEnum.FIRE;
					}
				}
				if (CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers != null)
				{
					int count = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count;
					if (count > 0)
					{
						if (CastleMinerZGame.Instance.GameMode != GameModeTypes.DragonEndurance && this._localSurfaceEnemyCount < 5 + 12 / count)
						{
							this._timeSinceLastSurfaceEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
							this.SpawnAbovegroundEnemy(worldPosition);
						}
						if (!this._lookingForSpaceRock && (this._timeOfLastSpaceRockScan == -1f || (CastleMinerZGame.Instance != null && (float)CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime.TotalSeconds - this._timeOfLastSpaceRockScan > 3f)))
						{
							this._lookingForSpaceRock = true;
							this._spaceRockScanPosition = worldPosition;
							TaskDispatcher.Instance.AddTask(this._searchForSpaceRockDelegate, null);
						}
						if (this._spaceRockNearby && this._localAlienCount < 10)
						{
							this._timeSinceLastAlien += (float)gameTime.ElapsedGameTime.TotalSeconds;
							this.SpawnAlien(worldPosition, this._playerInAsteroid, (float)gameTime.TotalGameTime.TotalSeconds);
						}
						if (simpleSunlightAtPoint != -1f && simpleSunlightAtPoint <= 0.4f && this._localCaveEnemyCount < 8 / count)
						{
							this._timeSinceLastCaveEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
							this.SpawnBelowgroundEnemy(worldPosition, (float)gameTime.TotalGameTime.TotalSeconds);
						}
					}
				}
			}
			base.OnUpdate(gameTime);
		}

		public void SearchForSpaceRock(BaseTask task, object context)
		{
			BlockTerrain instance = BlockTerrain.Instance;
			bool flag = false;
			bool flag2 = false;
			float maxValue = float.MaxValue;
			if (instance != null)
			{
				flag = instance.ContainsBlockType(this._spaceRockScanPosition, EnemyType.Types[52].SpawnRadius * 3, BlockTypeEnum.SpaceRock, ref maxValue);
				if (flag)
				{
					flag2 = instance.PointIsInAsteroid(this._spaceRockScanPosition);
				}
			}
			if (flag)
			{
				this._closestSpaceRock = maxValue;
			}
			this._playerInAsteroid = flag2;
			this._spaceRockNearby = flag;
			CastleMinerZGame instance2 = CastleMinerZGame.Instance;
			if (instance2 != null)
			{
				this._timeOfLastSpaceRockScan = (float)instance2.CurrentGameTime.TotalGameTime.TotalSeconds;
			}
			this._lookingForSpaceRock = false;
		}

		public void AddToSoundLevel(float level)
		{
			this._soundLevel += level;
			if (this._soundLevel > 20f)
			{
				this._soundLevel = 20f;
				if (!this._aliensAreAroused)
				{
					this._aliensAreAroused = true;
					this.SetNextAlienSpawnTime();
					return;
				}
			}
			else if (this._soundLevel < 0f)
			{
				this._soundLevel = 0f;
				if (!this._playerInAsteroid && this._aliensAreAroused)
				{
					this._aliensAreAroused = false;
					this.SetNextAlienSpawnTime();
				}
			}
		}

		public void HandleMessage(GameMessageType type, object data, object sender)
		{
			switch (type)
			{
			case GameMessageType.LocalPlayerMinedBlock:
				if ((BlockTypeEnum)data == BlockTypeEnum.SpaceRock)
				{
					this.AddToSoundLevel(10f);
					return;
				}
				break;
			case GameMessageType.LocalPlayerPickedAtBlock:
				if (((InventoryItem)sender).ItemClass.ID != InventoryItemIDs.BareHands && (BlockTypeEnum)data == BlockTypeEnum.SpaceRock)
				{
					this.AddToSoundLevel(1f);
					return;
				}
				break;
			case GameMessageType.LocalPlayerFiredGun:
				if (this._playerInAsteroid)
				{
					this.AddToSoundLevel(5f);
				}
				break;
			default:
				return;
			}
		}

		public void ResetSpawnerEnemies(Spawner spawnSource)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				if (this._enemies[i].SpawnSource == spawnSource)
				{
					this._enemies[i].GiveUp();
				}
			}
		}

		private const int MIN_LOCAL_SURFACE_ENEMY_LIMIT = 5;

		private const int MAX_LOCAL_SURFACE_ENEMIES = 12;

		private const int MAX_LOCAL_CAVE_ENEMIES = 8;

		private const int MAX_LOCAL_ALIENS = 10;

		private const int MAX_TOTAL_ENEMIES = 50;

		private const float ZOMBIE_STICKY_DISTANCE = 2f;

		private const float ZOMBIE_STICKY_DISTANCE_SQ = 4f;

		private const float ZOMBIEFEST_DISTANCE = 5000f;

		private const float ONE_DAY_IN_METERS = 120f;

		private const float DISTANCE_BOUNDARY_WIDTH = 40f;

		private const float DISTANCE_BOUNDARY_VARIANCE = 10f;

		private const float MIN_TIME_BETWEEN_RANDOM_SPAWNS = 2f;

		private const float MAX_TIME_BETWEEN_RANDOM_SPAWNS = 5f;

		private const int MIN_ZOMBIES_PER_RANDOM_SPAWN = 2;

		private const int MAX_ZOMBIES_PER_RANDOM_SPAWN = 5;

		private const float MINIMUM_SUN_FOR_SKELETON = 0.4f;

		private const float MINIMUM_TORCH_FOR_SKELETON = 0.4f;

		private const float MINIMUM_LANTERN_FOR_ZOMBIE = 0.4f;

		private const float MINIMUM_LANTERN_DIST_FOR_ZOMBIE = 7.2f;

		private const float FIREBALL_BLOCK_DAMAGE_RADIUS = 3f;

		private const float FIREBALL_BLOCK_DAMAGE_RADIUS_SQ = 9f;

		public const float ZOMBIEFEST_PERCENT_MIDNIGHT = 0.9f;

		public const float MAX_NORMAL_PERCENT_MIDNIGHT = 0.8f;

		private const float IN_HELL_MIDNIGHT_VALUE = 0.85f;

		private const float HELL_DEPTH = -40f;

		private const float FIREBALL_RANGE = 5f;

		private const float ENDURANCE_DRAGON_HEALTH_INCREMENT = 100f;

		private const float ENDURANCE_DRAGON_KILLED_HEALTH_INCREMENT = -15f;

		private const float ENDURANCE_DRAGON_MIN_PAUSE_SECONDS = 10f;

		private const float ENDURACE_DRAGON_MAX_PAUSE_SECONDS = 30f;

		private const float DRAGON_MINIMUM_INTERVAL = 8f;

		private const float DRAGON_INTERVAL = 16f;

		private const float DRAGON_BOX_RADIUS = 500f;

		private const float EASY_DAMAGE_MULTIPLIER = 1f;

		private const float ZOMBIE_TEST_DENSITY = 0.5f;

		private const float SOUND_OF_GUNSHOT_LEVEL = 5f;

		private const float SOUND_OF_PICKING_LEVEL = 1f;

		private const float SOUND_OF_MINED_BLOCK = 10f;

		private const float SOUND_REDUCTION_PER_SECOND = 2f;

		private const float SOUND_AROUSES_ALIENS_THRESHOLD = 20f;

		private const float MIN_TIME_BETWEEN_ALIENS = 10f;

		private const float MAX_TIME_BETWEEN_ALIENS = 20f;

		private const float MIN_TIME_BETWEEN_ALIENS_AROUSED = 3f;

		private const float MAX_TIME_BETWEEN_ALIENS_AROUSED = 7f;

		public static EnemyManager Instance;

		private IntVector3[] _fireballDamageBuffer;

		private BlockTypeEnum[] _fireballDamageItemTypes;

		private IntVector3[] _dependentItemsToRemoveBuffer;

		private BlockTypeEnum[] _dependItemTypes;

		private float _timeSinceLastSurfaceEnemy;

		private float _timeSinceLastCaveEnemy;

		private float _timeSinceLastAlien;

		private int _nextLocalEnemyID;

		private int _enemyCounter;

		private bool _spaceRockNearby;

		private bool _playerInAsteroid;

		private bool _lookingForSpaceRock;

		private float _closestSpaceRock = float.MaxValue;

		private Vector3 _spaceRockScanPosition = Vector3.Zero;

		private float _timeOfLastSpaceRockScan = -1f;

		private float _soundLevel;

		private bool _aliensAreAroused;

		private float _nextAlienTime = 10f;

		private TaskDelegate _searchForSpaceRockDelegate;

		private ContentManager _contentManager;

		private GraphicsDevice _graphicsDevice;

		private List<BaseZombie> _enemies;

		private DragonEntity _dragon;

		private DragonClientEntity _dragonClient;

		private List<FireballEntity> _fireballs;

		private int _localSurfaceEnemyCount;

		private int _localCaveEnemyCount;

		private int _localAlienCount;

		public float ClearedDistance = 50f;

		public int _distanceEnemiesLeftToSpawn;

		public float _nextDistanceEnemyTimer;

		public bool ZombieFestIsOn;

		public float _timeLeftTilFrenzy;

		public float _timeToFirstContact = 20f;

		public bool DragonPending;

		public BoundingBox DragonBox;

		public TimeSpan NextSpawnDragonTime;

		public TimeSpan NextDragonAllowedTime;

		public bool InitializeDragonBox;

		public int dragonDistanceIndex;

		private TimeSpan _nextEnduranceDragonTime;

		private DragonTypeEnum _nextEnduranceDragonType;

		private float _nextEnduranceDragonHealth = 25f;

		private float[] dragonDistances = new float[] { 100f, 500f, 1000f, 2600f, 4000f };

		public DragonTypeEnum NextTimedDragonType;

		private Random _rnd;

		private static OneShotTimer _spawnFelgardTimer = new OneShotTimer(TimeSpan.FromMinutes(5.0));

		public static bool FelguardSpawned = false;

		private DamageLOSProbe _damageLOSProbe = new DamageLOSProbe();

		private TraceProbe zombieProbe = new TraceProbe();
	}
}
