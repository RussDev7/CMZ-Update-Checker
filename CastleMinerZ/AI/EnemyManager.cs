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
			int damagebuffersize = 8;
			this._fireballDamageBuffer = new IntVector3[(damagebuffersize + 1) * damagebuffersize * damagebuffersize];
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
				float distance = Vector3.Distance(location, this._enemies[i].LocalPosition);
				float maxOutDamageRange = damageRange / 2f;
				if (distance < damageRange)
				{
					float damage;
					if (distance < maxOutDamageRange)
					{
						damage = 12f;
					}
					else
					{
						damage = 12f * (1f - (distance - maxOutDamageRange) / (damageRange - maxOutDamageRange));
					}
					this._enemies[i].TakeExplosiveDamage(damage, gamerID, itemID);
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
			Vector3 plrpos = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			plrpos.Y = 0f;
			float d = plrpos.Length();
			this.dragonDistanceIndex = Math.Min(this.dragonDistanceIndex, this.dragonDistances.Length);
			while (this.dragonDistanceIndex > 0 && d <= this.dragonDistances[this.dragonDistanceIndex - 1])
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
			float pm = BlockTerrain.Instance.PercentMidnight;
			float off = 1f - Math.Min(distance / 5000f, 1f);
			off *= 0.5f;
			pm = Math.Max(0f, pm - off);
			pm /= 1f - off;
			return pm.Clamp(0f, 0.79f);
		}

		public float CalculatePlayerDistance()
		{
			Player plr = CastleMinerZGame.Instance.LocalPlayer;
			float d = 0f;
			if (plr != null)
			{
				Vector3 plrpos = plr.WorldPosition;
				plrpos.Y = 0f;
				d = plrpos.Length();
				if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
				{
					d += (CastleMinerZGame.Instance.GameScreen.Day - 0.41f) * 120f;
				}
			}
			return d;
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
			bool spawnit = false;
			if (player != null)
			{
				if (player.IsLocal)
				{
					spawnit = true;
				}
				else if (this._enemies.Count < 45)
				{
					spawnit = true;
				}
				else if (this._enemies.Count < 50)
				{
					this._enemyCounter++;
					if ((this._enemyCounter & 3) == 1)
					{
						spawnit = true;
					}
				}
				if (!spawnit)
				{
					DebugUtils.Log("Spawn Failed. EnemyCount: " + this._enemies.Count);
					return;
				}
				BaseZombie zombie = new BaseZombie(this, msg.EnemyTypeID, player, msg.SpawnPosition, msg.EnemyID, msg.RandomSeed, msg.InitPkg);
				this.AddZombie(zombie);
				zombie.SpawnValue = msg.SpawnValue;
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
					zombie.SpawnSource = CastleMinerZGame.Instance.CurrentWorld.GetSpawner(IntVector3.FromVector3(msg.SpawnerPosition), false, BlockTypeEnum.Empty);
					zombie.SetDistanceLimit();
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
				Vector3 plrpos = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				plrpos.Y = 0f;
				if (plrpos.LengthSquared() > 12960000f)
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
					Player plr = CastleMinerZGame.Instance.LocalPlayer;
					Vector3 plrpos = plr.WorldPosition;
					plrpos.Y = 0f;
					if (this.NextTimedDragonType < DragonTypeEnum.COUNT || plrpos.LengthSquared() > 12960000f)
					{
						this.AskForDragon(false, this.NextTimedDragonType);
						this.NextTimedDragonType = (DragonTypeEnum)Math.Min((int)(this.NextTimedDragonType + 1), 5);
					}
				}
			}
		}

		private void RecalculateDragonBox(GameTime t)
		{
			Vector3 pos = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			this.DragonBox = new BoundingBox(new Vector3(pos.X - 500f, -100f, pos.Z - 500f), new Vector3(pos.X + 500f, 100f, pos.Z + 500f));
			float minutesToWait = 16f;
			switch (this.NextTimedDragonType)
			{
			case DragonTypeEnum.FIRE:
				minutesToWait = 32f;
				break;
			case DragonTypeEnum.FOREST:
				minutesToWait = 48f;
				break;
			case DragonTypeEnum.LIZARD:
				minutesToWait = 64f;
				break;
			case DragonTypeEnum.ICE:
				minutesToWait = 80f;
				break;
			case DragonTypeEnum.SKELETON:
				minutesToWait = 80f;
				break;
			case DragonTypeEnum.COUNT:
				minutesToWait = 80f;
				break;
			}
			this.NextSpawnDragonTime = t.TotalGameTime + TimeSpan.FromMinutes((double)(minutesToWait + 16f * MathTools.RandomFloat(-0.25f, 0.25f)));
		}

		public void BroadcastExistingDragonMessage(byte newClientID)
		{
			if (this._dragon != null)
			{
				float health = -1f;
				if (this._dragonClient != null)
				{
					health = this._dragonClient.Health;
				}
				ExistingDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, newClientID, this._dragon.EType.EType, this._dragon.ForBiome, health);
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
			int dropcount = MathTools.RandomInt(1, 4) + MathTools.RandomInt(1, 5);
			float dist = location.Length();
			float blender = (dist / 5000f).Clamp(0f, 1f);
			int explosiveDrop = MathTools.RandomInt(1, 3 + (int)(blender * 5f)) + MathTools.RandomInt(1, 3 + (int)(blender * 5f));
			for (int i = 0; i < explosiveDrop; i++)
			{
				InventoryItem item = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1);
				PickupManager.Instance.CreateUpwardPickup(item, location + new Vector3(0f, 1f, 0f), 3f, false);
			}
			for (int j = 0; j < dropcount; j++)
			{
				float dval = MathTools.RandomFloat(blender, 1f);
				float y = base.LocalPosition.Y;
				InventoryItem item2;
				if ((double)dval < 0.5)
				{
					item2 = InventoryItem.CreateItem(InventoryItemIDs.Copper, 1);
				}
				else if ((double)dval < 0.6)
				{
					item2 = InventoryItem.CreateItem(InventoryItemIDs.Iron, 1);
				}
				else if ((double)dval < 0.8)
				{
					item2 = InventoryItem.CreateItem(InventoryItemIDs.Gold, 1);
				}
				else if ((double)dval < 0.9)
				{
					item2 = InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1);
				}
				else
				{
					item2 = InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 1);
				}
				if (item2 != null)
				{
					PickupManager.Instance.CreateUpwardPickup(item2, location + new Vector3(0f, 1f, 0f), 3f, false);
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
						string str = string.Concat(new string[]
						{
							CastleMinerZGame.Instance.LocalPlayer.Gamer.Gamertag,
							" ",
							Strings.Has_Killed_The,
							" ",
							this._dragonClient.EType.GetDragonName()
						});
						BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, str);
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
				Vector3 playerPos = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				playerPos.Y += 1f;
				float dsq = Vector3.DistanceSquared(playerPos, msg.Location);
				if (dsq < 25f)
				{
					float d = Math.Max((float)Math.Sqrt((double)dsq) - 1f, 0f);
					this._damageLOSProbe.Init(msg.Location, playerPos);
					this._damageLOSProbe.DragonTypeIndex = (int)dragonType.EType;
					BlockTerrain.Instance.Trace(this._damageLOSProbe);
					float damage = Math.Min(this._damageLOSProbe.TotalDamageMultiplier * (1f - d / 5f), 1f);
					if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Survival && CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.EASY)
					{
						damage *= 1f;
					}
					damage *= dragonType.FireballDamage;
					InGameHUD.Instance.ApplyDamage(damage, msg.Location);
				}
			}
			DragonDamageType damageType = dragonType.DamageType;
			BlockTypeEnum newType;
			if (damageType == DragonDamageType.ICE)
			{
				newType = BlockTypeEnum.Ice;
			}
			else
			{
				newType = BlockTypeEnum.Empty;
			}
			for (byte j = 0; j < msg.NumBlocks; j += 1)
			{
				BlockTerrain.Instance.SetBlock(msg.BlocksToRemove[(int)j], newType);
			}
		}

		private int RememberDependentObjects(IntVector3 worldIndex, int numDependents)
		{
			for (BlockFace bf = BlockFace.POSX; bf < BlockFace.NUM_FACES; bf++)
			{
				IntVector3 nb = BlockTerrain.Instance.GetNeighborIndex(worldIndex, bf);
				BlockTypeEnum bbt = BlockTerrain.Instance.GetBlockWithChanges(nb);
				if (BlockType.GetType(bbt).Facing == bf)
				{
					this._dependentItemsToRemoveBuffer[numDependents] = nb;
					this._dependItemTypes[numDependents++] = bbt;
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
			Vector3 basePosition = new Vector3((float)Math.Floor((double)position.X) + 0.5f, (float)Math.Floor((double)position.Y) + 0.5f, (float)Math.Floor((double)position.Z) + 0.5f);
			Vector3 walker = Vector3.Zero;
			Vector3 testLocation = Vector3.Zero;
			int numBlocks = 0;
			int numDependentsToRemove = 0;
			IntVector3 zero = IntVector3.Zero;
			DragonTypeEnum denum = dragonType.EType;
			bool checkRemoveDependentItems = dragonType.DamageType == DragonDamageType.DESTRUCTION;
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance || CastleMinerZGame.Instance.GameMode == GameModeTypes.DragonEndurance || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARD || CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.HARDCORE)
			{
				walker.X = -3f;
				while (walker.X <= 3f)
				{
					testLocation.X = walker.X + basePosition.X;
					walker.Y = -3f;
					while (walker.Y <= 3f)
					{
						testLocation.Y = walker.Y + basePosition.Y;
						walker.Z = -3f;
						while (walker.Z <= 3f)
						{
							testLocation.Z = walker.Z + basePosition.Z;
							if (Vector3.DistanceSquared(testLocation, position) <= 9f)
							{
								IntVector3 vec = (IntVector3)testLocation;
								IntVector3 local = BlockTerrain.Instance.GetLocalIndex(vec);
								if (BlockTerrain.Instance.IsIndexValid(local))
								{
									BlockTypeEnum blockType = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(local));
									if (!DragonType.BreakLookup[(int)denum, (int)blockType] && !BlockType.IsUpperDoor(blockType))
									{
										this._fireballDamageItemTypes[numBlocks] = blockType;
										this._fireballDamageBuffer[numBlocks++] = vec;
										if (checkRemoveDependentItems)
										{
											numDependentsToRemove = this.RememberDependentObjects(vec, numDependentsToRemove);
										}
										if (blockType == BlockTypeEnum.NormalLowerDoorOpenX || blockType == BlockTypeEnum.NormalLowerDoorOpenZ || blockType == BlockTypeEnum.NormalLowerDoorClosedX || blockType == BlockTypeEnum.NormalLowerDoorClosedZ)
										{
											vec.Y++;
											this._fireballDamageItemTypes[numBlocks] = BlockTypeEnum.NormalUpperDoorOpen;
											this._fireballDamageBuffer[numBlocks++] = vec;
											if (checkRemoveDependentItems)
											{
												numDependentsToRemove = this.RememberDependentObjects(vec, numDependentsToRemove);
											}
										}
										if (blockType == BlockTypeEnum.StrongLowerDoorOpenX || blockType == BlockTypeEnum.StrongLowerDoorOpenZ || blockType == BlockTypeEnum.StrongLowerDoorClosedX || blockType == BlockTypeEnum.StrongLowerDoorClosedZ)
										{
											vec.Y++;
											this._fireballDamageItemTypes[numBlocks] = BlockTypeEnum.StrongUpperDoorOpen;
											this._fireballDamageBuffer[numBlocks++] = vec;
											if (checkRemoveDependentItems)
											{
												numDependentsToRemove = this.RememberDependentObjects(vec, numDependentsToRemove);
											}
										}
									}
								}
							}
							walker.Z += 1f;
						}
						walker.Y += 1f;
					}
					walker.X += 1f;
				}
			}
			int oldNumBlocks = numBlocks;
			for (int i = 0; i < numDependentsToRemove; i++)
			{
				if (!this.VectorWillBeDamaged(this._dependentItemsToRemoveBuffer[i], oldNumBlocks))
				{
					InventoryItem.InventoryItemClass bic = BlockInventoryItemClass.BlockClasses[BlockType.GetType(this._dependItemTypes[i]).ParentBlockType];
					PickupManager.Instance.CreatePickup(bic.CreateItem(1), IntVector3.ToVector3(this._dependentItemsToRemoveBuffer[i]) + new Vector3(0.5f, 0.5f, 0.5f), false, false);
					this._fireballDamageItemTypes[numBlocks] = this._dependItemTypes[i];
					this._fireballDamageBuffer[numBlocks++] = this._dependentItemsToRemoveBuffer[i];
					if (this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorOpenX || this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorOpenZ || this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorClosedX || this._dependItemTypes[i] == BlockTypeEnum.NormalLowerDoorClosedZ)
					{
						this._fireballDamageItemTypes[numBlocks] = BlockTypeEnum.NormalUpperDoorOpen;
						this._fireballDamageBuffer[numBlocks++] = this._dependentItemsToRemoveBuffer[i] + new IntVector3(0, 1, 0);
					}
				}
			}
			for (int j = 0; j < oldNumBlocks; j++)
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
			DetonateFireballMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, position, index, numBlocks, this._fireballDamageBuffer, dragonType.EType);
		}

		public void HandleEnemyGiveUpMessage(EnemyGiveUpMessage msg)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				BaseZombie enemy = this._enemies[i];
				if (!enemy.IsDead && (int)enemy.Target.Gamer.Id == msg.TargetID && enemy.EnemyID == msg.EnemyID)
				{
					enemy.GiveUp();
					if (enemy.SpawnSource != null)
					{
						enemy.SpawnSource.HandleEnemyRemoved(enemy.SpawnValue);
						enemy.SpawnSource = null;
					}
					return;
				}
			}
		}

		public void HandleKillEnemyMessage(KillEnemyMessage msg)
		{
			for (int i = 0; i < this._enemies.Count; i++)
			{
				BaseZombie enemy = this._enemies[i];
				if (!enemy.IsDead && (int)enemy.Target.Gamer.Id == msg.TargetID && enemy.EnemyID == msg.EnemyID)
				{
					if (CastleMinerZGame.Instance.IsLocalPlayerId(msg.KillerID))
					{
						CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(msg.WeaponID);
						enemy.CreatePickup();
						CastleMinerZGame.Instance.PlayerStats.AddStat(enemy.EType.Category);
						itemStats.AddStat(enemy.EType.Category);
						if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance)
						{
							CastleMinerZGame.Instance.PlayerStats.TotalKills++;
						}
					}
					if (enemy.SpawnSource != null)
					{
						enemy.SpawnSource.HandleEnemyDefeated(enemy.SpawnValue, msg.KillerID);
						enemy.SpawnSource = null;
					}
					enemy.Kill();
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
			IShootableEnemy result = null;
			BlockTerrain.Instance.Trace(tp);
			if (this._enemies.Count != 0)
			{
				BaseZombie closestZombie = null;
				if (tp._collides)
				{
					this.zombieProbe.Init(tp._start, tp.GetIntersection());
				}
				else
				{
					this.zombieProbe.Init(tp._start, tp._end);
				}
				Vector3 nml = this.zombieProbe._end - this.zombieProbe._start;
				if (nml.LengthSquared() <= 0.0001f)
				{
					return closestZombie;
				}
				nml.Normalize();
				float startD = Vector3.Dot(this.zombieProbe._start, nml);
				float endD = Vector3.Dot(this.zombieProbe._end, nml);
				float prevT = this.zombieProbe._inT;
				foreach (BaseZombie z in this._enemies)
				{
					if (z.IsHittable && tp.TestThisEnemy(z))
					{
						Vector3 wp = z.WorldPosition;
						float d = Vector3.Dot(wp, nml);
						if (startD - d <= 3f && d - endD <= 3f)
						{
							Vector3 vtoz = wp - this.zombieProbe._start;
							Vector3 cr = Vector3.Cross(nml, vtoz);
							float tol2 = ((z.EType.EType == EnemyTypeEnum.FELGUARD || z.EType.EType == EnemyTypeEnum.HELL_LORD) ? 0.001f : 0.0001f);
							if (cr.LengthSquared() > tol2)
							{
								cr.Normalize();
								cr = Vector3.Cross(cr, nml);
								float tol3 = ((z.EType.EType == EnemyTypeEnum.FELGUARD || z.EType.EType == EnemyTypeEnum.HELL_LORD) ? 9f : 3f);
								if (Math.Abs(Vector3.Dot(wp, cr) - Vector3.Dot(this.zombieProbe._start, cr)) > tol3)
								{
									continue;
								}
							}
							if (z.EType.EType == EnemyTypeEnum.FELGUARD || z.EType.EType == EnemyTypeEnum.HELL_LORD)
							{
								BoundingBox bb = z.PlayerAABB;
								bb.Min.X = -1.5f;
								bb.Min.Z = -1.5f;
								bb.Max.X = 1.5f;
								bb.Max.Z = 1.5f;
								bb.Min.Y = 0f;
								bb.Max.Y = 6f;
								bb.Min += wp;
								bb.Max += wp;
								this.zombieProbe.TestBoundBox(bb);
							}
							else
							{
								BoundingBox bb2 = z.PlayerAABB;
								bb2.Min += wp;
								bb2.Max += wp;
								this.zombieProbe.TestBoundBox(bb2);
							}
							if (this.zombieProbe._collides && prevT != this.zombieProbe._inT)
							{
								float f = z.EType.ChanceOfBulletStrike;
								Vector3 intersection = this.zombieProbe.GetIntersection();
								bool countIt = f == 1f || meleeWeapon || z.IsHeadshot(intersection) || (float)this._rnd.NextDouble() <= f;
								if (countIt)
								{
									closestZombie = z;
									endD = Vector3.Dot(intersection, nml);
									prevT = this.zombieProbe._inT;
								}
							}
						}
					}
				}
				if (closestZombie != null)
				{
					tp._collides = true;
					tp._end = this.zombieProbe._end;
					tp._inT = this.zombieProbe._inT;
					tp._inNormal = this.zombieProbe._inNormal;
					tp._inFace = this.zombieProbe._inFace;
					result = closestZombie;
				}
			}
			if (result == null && !tp._collides && this._dragonClient != null)
			{
				tp.Reset();
				if (this._dragonClient.Trace(tp))
				{
					result = this._dragonClient;
				}
			}
			return result;
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
			Vector3 newpos = plrpos;
			newpos.Y += 1f;
			float distance = this.CalculatePlayerDistance();
			float pm = this.CalculateMidnight(distance, plrpos.Y);
			Vector3 vel = CastleMinerZGame.Instance.LocalPlayer.PlayerPhysics.WorldVelocity;
			vel *= 5f;
			EnemyTypeEnum etype = EnemyType.GetZombie(distance);
			int radius = EnemyType.Types[(int)etype].SpawnRadius;
			newpos.X += vel.X + (float)this._rnd.Next(-radius, radius + 1);
			newpos.Z += vel.Z + (float)this._rnd.Next(-radius, radius + 1);
			if (BlockTerrain.Instance.RegionIsLoaded(newpos))
			{
				newpos = BlockTerrain.Instance.FindTopmostGroundLocation(newpos);
				IntVector3 worldPos = new IntVector3((int)newpos.X, (int)newpos.Y - 1, (int)newpos.Z);
				int index = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(worldPos);
				int block = BlockTerrain.Instance._blocks[index];
				BlockType blockType = Block.GetType(block);
				this._distanceEnemiesLeftToSpawn--;
				if (blockType._type == BlockTypeEnum.SpaceRock || blockType._type == BlockTypeEnum.SpaceRockInventory)
				{
					return;
				}
				SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, newpos, etype, pm, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
				if (etype == EnemyTypeEnum.ALIEN)
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
				NetworkGamer gamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
				if (gamer.Tag != null && name.Equals(gamer.Gamertag))
				{
					return (Player)gamer.Tag;
				}
			}
			return null;
		}

		public int SpawnEnemy(Vector3 newpos, EnemyTypeEnum etype, Vector3 spawnerPos, int spawnValue, string targetPlayerName = null)
		{
			Player player = CastleMinerZGame.Instance.LocalPlayer;
			float pm = 0f;
			int nextEnemyID = this.MakeNextEnemyID();
			SpawnEnemyMessage.Send((LocalNetworkGamer)player.Gamer, newpos, etype, pm, nextEnemyID, this._rnd.Next(), spawnerPos, spawnValue, targetPlayerName);
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
			return nextEnemyID;
		}

		private void SpawnAbovegroundEnemy(Vector3 plrpos)
		{
			Vector3 newpos = plrpos;
			newpos.Y += 1f;
			float distance = this.CalculatePlayerDistance();
			float pm = this.CalculateMidnight(distance, plrpos.Y);
			if (pm <= 0.0001f)
			{
				this._timeSinceLastSurfaceEnemy = 0f;
				return;
			}
			float interval = MathHelper.Lerp(60f, this.GetMinEnemySpawnTime(distance), (float)Math.Pow((double)pm, 0.25));
			if (this._timeSinceLastSurfaceEnemy > interval * (1f + (float)this._rnd.NextDouble() * 0.5f))
			{
				Vector3 vel = CastleMinerZGame.Instance.LocalPlayer.PlayerPhysics.WorldVelocity;
				vel *= 5f;
				EnemyTypeEnum etype = EnemyType.GetAbovegroundEnemy(pm, distance);
				int radius = EnemyType.Types[(int)etype].SpawnRadius;
				newpos.X += vel.X + (float)this._rnd.Next(-radius, radius + 1);
				newpos.Z += vel.Z + (float)this._rnd.Next(-radius, radius + 1);
				if (BlockTerrain.Instance.RegionIsLoaded(newpos))
				{
					if (plrpos.Y > -40f)
					{
						newpos = BlockTerrain.Instance.FindTopmostGroundLocation(newpos);
					}
					else
					{
						newpos = BlockTerrain.Instance.FindSafeStartLocation(newpos);
					}
					IntVector3 worldPos = new IntVector3((int)newpos.X, (int)newpos.Y - 1, (int)newpos.Z);
					int index = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(worldPos);
					int block = BlockTerrain.Instance._blocks[index];
					BlockType blockType = Block.GetType(block);
					Vector3 np = new Vector3(newpos.X, newpos.Y + 0.5f, newpos.Z);
					float torchlight = BlockTerrain.Instance.GetSimpleTorchlightAtPoint(np);
					if (torchlight < 0.4f || !ItemBlockEntityManager.Instance.NearLantern(np, 7.2f))
					{
						this._timeSinceLastSurfaceEnemy = 0f;
						if (blockType._type == BlockTypeEnum.SpaceRock || blockType._type == BlockTypeEnum.SpaceRockInventory)
						{
							return;
						}
						SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, newpos, etype, pm, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
						this._localSurfaceEnemyCount++;
					}
				}
			}
		}

		private void SpawnBelowgroundEnemy(Vector3 plrpos, float gametime)
		{
			Vector3 newpos = plrpos;
			newpos.Y += 1f;
			int depth = (int)(-(plrpos.Y - 20f)).Clamp(0f, 50f);
			float lerper = (float)depth / 50f;
			float cycle = (float)Math.Sin((double)(gametime / 60f % 2f * 3.1415927f));
			if (cycle > 0f)
			{
				cycle = (float)Math.Sqrt((double)cycle);
			}
			else
			{
				cycle = 0f;
			}
			lerper *= cycle;
			float d = ((plrpos.Y < -40f) ? 3500f : this.CalculatePlayerDistance());
			float interval = MathHelper.Lerp(60f, this.GetMinEnemySpawnTime(d), lerper);
			if (this._timeSinceLastCaveEnemy > interval * (1f + (float)this._rnd.NextDouble() * 0.5f))
			{
				EnemyTypeEnum etype = EnemyType.GetBelowgroundEnemy((float)depth, d);
				int radius = EnemyType.Types[(int)etype].SpawnRadius;
				int offset = this._rnd.Next(-radius, radius);
				if (offset <= 0)
				{
					offset -= 5;
				}
				else
				{
					offset += 5;
				}
				newpos.X += (float)offset;
				offset = this._rnd.Next(-radius, radius);
				if (offset <= 0)
				{
					offset -= 5;
				}
				else
				{
					offset += 5;
				}
				newpos.Z += (float)offset;
				if (BlockTerrain.Instance.RegionIsLoaded(newpos))
				{
					newpos = BlockTerrain.Instance.FindClosestCeiling(newpos);
					if (newpos.LengthSquared() != 0f)
					{
						IntVector3 worldPos = new IntVector3((int)newpos.X, (int)newpos.Y, (int)newpos.Z);
						int index = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(worldPos);
						int block = BlockTerrain.Instance._blocks[index];
						BlockType blockType = Block.GetType(block);
						Vector3 lightchecker = newpos;
						lightchecker.Y -= 1f;
						Vector2 light = BlockTerrain.Instance.GetSimpleLightAtPoint(lightchecker);
						if (light.X <= 0.4f && light.Y <= 0.4f)
						{
							this._timeSinceLastCaveEnemy = 0f;
							if (blockType._type == BlockTypeEnum.SpaceRock || blockType._type == BlockTypeEnum.SpaceRockInventory)
							{
								return;
							}
							SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, newpos, etype, 0f, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
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
				EnemyTypeEnum etype = EnemyTypeEnum.ALIEN;
				int originalRadius = EnemyType.Types[(int)etype].SpawnRadius;
				Vector3 newpos = Vector3.Zero;
				if (this._aliensAreAroused && BlockTerrain.Instance.RegionIsLoaded(plrpos))
				{
					newpos = BlockTerrain.Instance.FindNearbySpawnPoint(plrpos, originalRadius * 2, originalRadius);
				}
				else
				{
					int radius;
					int minOffset;
					if (!inAsteroid)
					{
						radius = originalRadius * 3;
						minOffset = 5;
					}
					else
					{
						radius = originalRadius;
						minOffset = 1;
					}
					newpos = plrpos;
					newpos.Y += 1f;
					int offset = this._rnd.Next(-radius, radius);
					if (offset <= 0)
					{
						offset -= minOffset;
					}
					else
					{
						offset += minOffset;
					}
					newpos.X += (float)offset;
					offset = this._rnd.Next(-radius, radius);
					if (offset <= 0)
					{
						offset -= minOffset;
					}
					else
					{
						offset += minOffset;
					}
					newpos.Z += (float)offset;
					if (BlockTerrain.Instance.RegionIsLoaded(newpos))
					{
						newpos = BlockTerrain.Instance.FindAlienSpawnPoint(newpos, this._closestSpaceRock > (float)originalRadius);
					}
					else
					{
						newpos = Vector3.Zero;
					}
				}
				if (newpos.LengthSquared() != 0f)
				{
					Vector3 np = new Vector3(newpos.X, newpos.Y + 0.5f, newpos.Z);
					IntVector3 worldPos = new IntVector3((int)newpos.X, (int)newpos.Y - 1, (int)newpos.Z);
					int index = BlockTerrain.Instance.MakeIndexFromWorldIndexVector(worldPos);
					BlockType bt = Block.GetType(BlockTerrain.Instance._blocks[index]);
					if (!bt.BlockPlayer)
					{
						index--;
						bt = Block.GetType(BlockTerrain.Instance._blocks[index]);
					}
					if (bt.ParentBlockType == BlockTypeEnum.SpaceRock || this._closestSpaceRock > (float)(originalRadius * 2))
					{
						this._timeSinceLastAlien = 0f;
						this.SetNextAlienSpawnTime();
						SpawnEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, np, etype, 0f, this.MakeNextEnemyID(), this._rnd.Next(), Vector3.Zero, 0, null);
						this._localAlienCount++;
					}
				}
			}
		}

		private void SpawnTestEnemy(Vector3 plrpos)
		{
			BaseZombie zombie = new BaseZombie(this, EnemyTypeEnum.ALIEN, CastleMinerZGame.Instance.LocalPlayer, plrpos, 52, 1, EnemyType.Types[52].CreateInitPackage(0.5f));
			this.AddZombie(zombie);
		}

		public float AttentuateVelocity(Player plr, Vector3 fwd, Vector3 worldPos)
		{
			float result = 1f;
			for (int i = 0; i < this._enemies.Count; i++)
			{
				if (this._enemies[i].Target == plr && this._enemies[i].IsBlocking)
				{
					Vector3 delta = this._enemies[i].WorldPosition - worldPos;
					float lsq = delta.LengthSquared();
					float att = 1f;
					if (lsq < 4f && Math.Abs(delta.Y) < 1.5f)
					{
						att = 0.5f;
						if (lsq > 0.0001f)
						{
							Vector3 nml = Vector3.Normalize(delta);
							float dot = Vector3.Dot(nml, fwd);
							if (dot > 0f)
							{
								att *= Math.Min(1f, 2f * (1f - dot));
							}
						}
					}
					result *= att;
				}
			}
			return result;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (CastleMinerZGame.Instance.Difficulty == GameDifficultyTypes.NOENEMIES)
			{
				return;
			}
			this.AddToSoundLevel((float)gameTime.ElapsedGameTime.TotalSeconds * -2f);
			Player plr = CastleMinerZGame.Instance.LocalPlayer;
			EnemyManager._spawnFelgardTimer.Update(gameTime.ElapsedGameTime);
			if (plr != null && this._enemies.Count < 50)
			{
				this._timeSinceLastSurfaceEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
				this._timeSinceLastCaveEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
				Vector3 plrpos = plr.WorldPosition;
				Vector3 newpos = plrpos;
				newpos.Y += 1f;
				float sunlight = BlockTerrain.Instance.GetSimpleSunlightAtPoint(newpos);
				Vector2 pp = new Vector2(plrpos.X, plrpos.Z);
				float d = pp.Length();
				if (CastleMinerZGame.Instance.GameMode != GameModeTypes.DragonEndurance)
				{
					if (CastleMinerZGame.Instance.IsGameHost && this.dragonDistanceIndex < this.dragonDistances.Length && d > this.dragonDistances[this.dragonDistanceIndex])
					{
						this.AskForDragon(true, (DragonTypeEnum)this.dragonDistanceIndex);
						this.dragonDistanceIndex++;
					}
					this.CheckDragonBox(gameTime);
					bool zfOn = BlockTerrain.Instance.PercentMidnight > 0.9f;
					if (zfOn)
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
						if (d > this.ClearedDistance)
						{
							this._timeToFirstContact = -1f;
							if (sunlight > 0.01f)
							{
								if (this._distanceEnemiesLeftToSpawn == 0)
								{
									this._nextDistanceEnemyTimer = MathTools.RandomFloat(2f);
								}
								this._distanceEnemiesLeftToSpawn += MathTools.RandomInt(2, 5);
							}
							float nextD = (float)Math.Floor(1.5 + (double)(d / 40f)) * 40f;
							this.ClearedDistance = MathTools.RandomFloat(nextD - 10f, nextD + 10f);
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
									this.SpawnRandomZombies(plrpos);
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
					int gamerCount = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count;
					if (gamerCount > 0)
					{
						if (CastleMinerZGame.Instance.GameMode != GameModeTypes.DragonEndurance && this._localSurfaceEnemyCount < 5 + 12 / gamerCount)
						{
							this._timeSinceLastSurfaceEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
							this.SpawnAbovegroundEnemy(plrpos);
						}
						if (!this._lookingForSpaceRock && (this._timeOfLastSpaceRockScan == -1f || (CastleMinerZGame.Instance != null && (float)CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime.TotalSeconds - this._timeOfLastSpaceRockScan > 3f)))
						{
							this._lookingForSpaceRock = true;
							this._spaceRockScanPosition = plrpos;
							TaskDispatcher.Instance.AddTask(this._searchForSpaceRockDelegate, null);
						}
						if (this._spaceRockNearby && this._localAlienCount < 10)
						{
							this._timeSinceLastAlien += (float)gameTime.ElapsedGameTime.TotalSeconds;
							this.SpawnAlien(plrpos, this._playerInAsteroid, (float)gameTime.TotalGameTime.TotalSeconds);
						}
						if (sunlight != -1f && sunlight <= 0.4f && this._localCaveEnemyCount < 8 / gamerCount)
						{
							this._timeSinceLastCaveEnemy += (float)gameTime.ElapsedGameTime.TotalSeconds;
							this.SpawnBelowgroundEnemy(plrpos, (float)gameTime.TotalGameTime.TotalSeconds);
						}
					}
				}
			}
			base.OnUpdate(gameTime);
		}

		public void SearchForSpaceRock(BaseTask task, object context)
		{
			BlockTerrain t = BlockTerrain.Instance;
			bool blockNearby = false;
			bool insideAsteroid = false;
			float nearestBlock = float.MaxValue;
			if (t != null)
			{
				blockNearby = t.ContainsBlockType(this._spaceRockScanPosition, EnemyType.Types[52].SpawnRadius * 3, BlockTypeEnum.SpaceRock, ref nearestBlock);
				if (blockNearby)
				{
					insideAsteroid = t.PointIsInAsteroid(this._spaceRockScanPosition);
				}
			}
			if (blockNearby)
			{
				this._closestSpaceRock = nearestBlock;
			}
			this._playerInAsteroid = insideAsteroid;
			this._spaceRockNearby = blockNearby;
			CastleMinerZGame i = CastleMinerZGame.Instance;
			if (i != null)
			{
				this._timeOfLastSpaceRockScan = (float)i.CurrentGameTime.TotalGameTime.TotalSeconds;
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
