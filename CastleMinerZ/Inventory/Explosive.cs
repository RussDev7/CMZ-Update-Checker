using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Collections;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DNA.CastleMinerZ.Inventory
{
	public class Explosive : IEquatable<Explosive>
	{
		static Explosive()
		{
			for (int i = 0; i < 6; i++)
			{
				Explosive.BreakLookup[i, 0] = true;
				Explosive.BreakLookup[i, 5] = true;
				Explosive.BreakLookup[i, 14] = true;
				Explosive.BreakLookup[i, 94] = true;
				Explosive.BreakLookup[i, 43] = true;
				Explosive.BreakLookup[i, 21] = true;
				Explosive.BreakLookup[i, 44] = true;
			}
			Explosive.BreakLookup[0, 4] = true;
			Explosive.BreakLookup[0, 67] = true;
			Explosive.BreakLookup[0, 7] = true;
			Explosive.BreakLookup[0, 8] = true;
			Explosive.BreakLookup[0, 9] = true;
			Explosive.BreakLookup[0, 10] = true;
			Explosive.BreakLookup[0, 11] = true;
			Explosive.BreakLookup[0, 22] = true;
			Explosive.BreakLookup[0, 23] = true;
			Explosive.BreakLookup[0, 24] = true;
			Explosive.BreakLookup[0, 25] = true;
			Explosive.BreakLookup[0, 20] = true;
			Explosive.BreakLookup[0, 45] = true;
			Explosive.BreakLookup[0, 46] = true;
			Explosive.BreakLookup[0, 48] = true;
			Explosive.BreakLookup[0, 47] = true;
			Explosive.BreakLookup[0, 56] = true;
			Explosive.BreakLookup[0, 57] = true;
			Explosive.BreakLookup[0, 58] = true;
			Explosive.BreakLookup[0, 59] = true;
			Explosive.BreakLookup[3, 4] = true;
			Explosive.BreakLookup[3, 67] = true;
			Explosive.BreakLookup[3, 7] = true;
			Explosive.BreakLookup[3, 8] = true;
			Explosive.BreakLookup[3, 9] = true;
			Explosive.BreakLookup[3, 10] = true;
			Explosive.BreakLookup[3, 11] = true;
			Explosive.BreakLookup[3, 22] = true;
			Explosive.BreakLookup[3, 23] = true;
			Explosive.BreakLookup[3, 24] = true;
			Explosive.BreakLookup[3, 25] = true;
			Explosive.BreakLookup[3, 20] = true;
			Explosive.BreakLookup[3, 45] = true;
			Explosive.BreakLookup[3, 46] = true;
			Explosive.BreakLookup[3, 48] = true;
			Explosive.BreakLookup[3, 47] = true;
			Explosive.BreakLookup[3, 56] = true;
			Explosive.BreakLookup[3, 57] = true;
			Explosive.BreakLookup[3, 58] = true;
			Explosive.BreakLookup[3, 59] = true;
			Explosive.BreakLookup[4, 4] = true;
			Explosive.BreakLookup[4, 67] = true;
			Explosive.BreakLookup[4, 7] = true;
			Explosive.BreakLookup[4, 8] = true;
			Explosive.BreakLookup[4, 9] = true;
			Explosive.BreakLookup[4, 10] = true;
			Explosive.BreakLookup[4, 11] = true;
			Explosive.BreakLookup[4, 22] = true;
			Explosive.BreakLookup[4, 23] = true;
			Explosive.BreakLookup[4, 24] = true;
			Explosive.BreakLookup[4, 25] = true;
			Explosive.BreakLookup[4, 20] = true;
			Explosive.BreakLookup[4, 45] = true;
			Explosive.BreakLookup[4, 46] = true;
			Explosive.BreakLookup[4, 48] = true;
			Explosive.BreakLookup[4, 47] = true;
			Explosive.BreakLookup[4, 56] = true;
			Explosive.BreakLookup[4, 57] = true;
			Explosive.BreakLookup[4, 58] = true;
			Explosive.BreakLookup[4, 59] = true;
			Explosive.BreakLookup[5, 4] = true;
			Explosive.BreakLookup[5, 67] = true;
			Explosive.BreakLookup[5, 22] = true;
			Explosive.BreakLookup[5, 23] = true;
			Explosive.BreakLookup[5, 24] = true;
			Explosive.BreakLookup[5, 25] = true;
			Explosive.BreakLookup[5, 20] = true;
			Explosive.BreakLookup[5, 45] = true;
			Explosive.BreakLookup[5, 46] = true;
			Explosive.BreakLookup[5, 48] = true;
			Explosive.BreakLookup[5, 47] = true;
			Explosive.BreakLookup[5, 56] = true;
			Explosive.BreakLookup[5, 57] = true;
			Explosive.BreakLookup[5, 58] = true;
			Explosive.BreakLookup[5, 59] = true;
			for (int j = 0; j < 95; j++)
			{
				switch (BlockType.GetType((BlockTypeEnum)j).Hardness)
				{
				case 1:
					Explosive.Level1Hardness[j] = true;
					break;
				case 2:
					Explosive.Level1Hardness[j] = true;
					break;
				case 3:
					Explosive.Level2Hardness[j] = true;
					break;
				case 4:
					Explosive.Level2Hardness[j] = true;
					break;
				}
			}
		}

		public Explosive(IntVector3 position, ExplosiveTypes explosiveType)
		{
			this.Position = position;
			this.ExplosiveType = explosiveType;
		}

		public bool Equals(Explosive other)
		{
			return other.Position == this.Position && other.ExplosiveType == this.ExplosiveType;
		}

		public void Update(TimeSpan gameTime)
		{
			if (!this.Timer.Expired)
			{
				this.Timer.Update(gameTime);
				BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(this.Position);
				BlockTypeEnum blockTypeEnum = ((this.ExplosiveType == ExplosiveTypes.TNT) ? BlockTypeEnum.TNT : BlockTypeEnum.C4);
				if (this.Timer.Expired && blockWithChanges == blockTypeEnum)
				{
					DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, this.Position, true, this.ExplosiveType);
				}
			}
		}

		public static void HandleDetonateRocketMessage(DetonateRocketMessage msg)
		{
			Explosive.AddEffects(msg.Location, !msg.HitDragon);
			if (msg.HitDragon)
			{
				if (EnemyManager.Instance != null && EnemyManager.Instance.DragonIsActive)
				{
					EnemyManager.Instance.ApplyExplosiveDamageToDragon(msg.Location, 200f, msg.Sender.Id, msg.ItemType);
					return;
				}
			}
			else
			{
				Explosive.ApplySplashDamageToLocalPlayerAndZombies(msg.Location, msg.ExplosiveType, msg.ItemType, msg.Sender.Id);
			}
		}

		public static void DetonateGrenade(Vector3 position, ExplosiveTypes grenadeType, byte shooterID, bool wantRockChunks)
		{
			Explosive.AddEffects(position, wantRockChunks);
			Explosive.ApplySplashDamageToLocalPlayerAndZombies(position, grenadeType, InventoryItemIDs.Grenade, shooterID);
		}

		private static void ApplySplashDamageToLocalPlayerAndZombies(Vector3 location, ExplosiveTypes explosiveType, InventoryItemIDs itemType, byte shooterID)
		{
			if (CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				worldPosition.Y += 1f;
				float num = Vector3.Distance(worldPosition, location);
				float num2 = Explosive.cKillRanges[(int)explosiveType];
				float num3 = Explosive.cDamageRanges[(int)explosiveType];
				if (num < num3)
				{
					DamageLOSProbe damageLOSProbe = new DamageLOSProbe();
					damageLOSProbe.Init(location, worldPosition);
					damageLOSProbe.DragonTypeIndex = 0;
					BlockTerrain.Instance.Trace(damageLOSProbe);
					float num4;
					if (num < num2)
					{
						num4 = 1f;
					}
					else
					{
						num4 = damageLOSProbe.TotalDamageMultiplier * (1f - (num - num2) / (num3 - num2));
					}
					InGameHUD.Instance.ApplyDamage(num4, location);
				}
				if (EnemyManager.Instance != null)
				{
					EnemyManager.Instance.ApplyExplosiveDamageToZombies(location, Explosive.cEnemyDamageRanges[(int)explosiveType], shooterID, itemType);
				}
			}
		}

		public static void HandleDetonateExplosiveMessage(DetonateExplosiveMessage msg)
		{
			BlockTerrain.Instance.SetBlock(msg.Location, BlockTypeEnum.Empty);
			if (CastleMinerZGame.Instance.GameScreen != null)
			{
				CastleMinerZGame.Instance.GameScreen.RemoveExplosiveFlashModel(msg.Location);
			}
			if (msg.OriginalExplosion)
			{
				Explosive.AddEffects(msg.Location, true);
			}
			if (CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				Explosive.ApplySplashDamageToLocalPlayerAndZombies(msg.Location, msg.ExplosiveType, (msg.ExplosiveType == ExplosiveTypes.TNT) ? InventoryItemIDs.TNT : InventoryItemIDs.C4, msg.Sender.Id);
			}
			if (msg.Sender.IsLocal && msg.OriginalExplosion)
			{
				Explosive.FindBlocksToRemove(msg.Location, msg.ExplosiveType, false);
			}
		}

		private static void RememberDependentObjects(IntVector3 worldIndex, Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove)
		{
			for (BlockFace blockFace = BlockFace.POSX; blockFace < BlockFace.NUM_FACES; blockFace++)
			{
				IntVector3 neighborIndex = BlockTerrain.Instance.GetNeighborIndex(worldIndex, blockFace);
				if (!dependentsToRemove.ContainsKey(neighborIndex))
				{
					BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(neighborIndex);
					if (BlockType.GetType(blockWithChanges).Facing == blockFace)
					{
						dependentsToRemove.Add(neighborIndex, blockWithChanges);
					}
				}
			}
		}

		private static void ProcessOneExplosion(Queue<Explosive> tntToExplode, Set<IntVector3> blocksToRemove, Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove, ref bool explosionFlashNotYetShown)
		{
			Explosive explosive = tntToExplode.Dequeue();
			if (explosionFlashNotYetShown && (explosive.ExplosiveType == ExplosiveTypes.C4 || explosive.ExplosiveType == ExplosiveTypes.TNT))
			{
				AddExplosionEffectsMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, explosive.Position);
				explosionFlashNotYetShown = false;
			}
			int num = Explosive.cDestructionRanges[(int)explosive.ExplosiveType];
			IntVector3 zero = IntVector3.Zero;
			zero.X = -num;
			while (zero.X <= num)
			{
				IntVector3 intVector;
				intVector.X = explosive.Position.X + zero.X;
				IntVector3 intVector2;
				intVector2.X = intVector.X - BlockTerrain.Instance._worldMin.X;
				if (intVector2.X >= 0 && intVector2.X < 384)
				{
					zero.Z = -num;
					while (zero.Z <= num)
					{
						intVector.Z = explosive.Position.Z + zero.Z;
						intVector2.Z = intVector.Z - BlockTerrain.Instance._worldMin.Z;
						if (intVector2.Z >= 0 && intVector2.Z < 384)
						{
							zero.Y = -num;
							while (zero.Y <= num)
							{
								intVector.Y = explosive.Position.Y + zero.Y;
								intVector2.Y = intVector.Y - BlockTerrain.Instance._worldMin.Y;
								if (intVector2.Y >= 0 && intVector2.Y < 128 && !blocksToRemove.Contains(intVector))
								{
									BlockTypeEnum typeIndex = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(intVector2));
									if (typeIndex == BlockTypeEnum.TNT || typeIndex == BlockTypeEnum.C4)
									{
										ExplosiveTypes explosiveTypes = ((typeIndex == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
										tntToExplode.Enqueue(new Explosive(intVector, explosiveTypes));
										DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector, false, explosiveTypes);
										blocksToRemove.Add(intVector);
									}
									else if (!Explosive.BreakLookup[(int)explosive.ExplosiveType, (int)typeIndex] && Explosive.BlockWithinLevelBlastRange(zero, typeIndex, explosive.ExplosiveType) && !BlockType.IsUpperDoor(typeIndex))
									{
										blocksToRemove.Add(intVector);
										if (BlockType.IsContainer(typeIndex))
										{
											DestroyCrateMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector);
											Crate crate;
											if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(intVector, out crate))
											{
												crate.EjectContents();
											}
										}
										if (BlockType.IsDoor(typeIndex))
										{
											DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector, typeIndex);
										}
										Explosive.RememberDependentObjects(intVector, dependentsToRemove);
										if (BlockType.IsLowerDoor(typeIndex))
										{
											IntVector3 intVector3 = intVector;
											intVector3.Y++;
											if (!blocksToRemove.Contains(intVector3))
											{
												blocksToRemove.Add(intVector3);
												Explosive.RememberDependentObjects(intVector3, dependentsToRemove);
											}
										}
										if (explosive.ExplosiveType == ExplosiveTypes.Harvest)
										{
											Explosive.ProcessHarvestExplosion(typeIndex, intVector);
										}
										if (BlockType.ShouldDropLoot(typeIndex))
										{
											PossibleLootType.ProcessLootBlockOutput(typeIndex, intVector);
										}
									}
								}
								zero.Y++;
							}
						}
						zero.Z++;
					}
				}
				zero.X++;
			}
		}

		private static InventoryItem GetHarvestedItem(BlockTypeEnum blockType)
		{
			switch (blockType)
			{
			case BlockTypeEnum.GoldOre:
				return InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 1);
			case BlockTypeEnum.IronOre:
				return InventoryItem.CreateItem(InventoryItemIDs.IronOre, 1);
			case BlockTypeEnum.CopperOre:
				return InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 1);
			case BlockTypeEnum.CoalOre:
				return InventoryItem.CreateItem(InventoryItemIDs.Coal, 1);
			case BlockTypeEnum.DiamondOre:
				return InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1);
			default:
				switch (blockType)
				{
				case BlockTypeEnum.IronWall:
					return InventoryItem.CreateItem(InventoryItemIDs.IronWall, 1);
				case BlockTypeEnum.CopperWall:
					return InventoryItem.CreateItem(InventoryItemIDs.CopperWall, 1);
				case BlockTypeEnum.GoldenWall:
					return InventoryItem.CreateItem(InventoryItemIDs.GoldenWall, 1);
				case BlockTypeEnum.DiamondWall:
					return InventoryItem.CreateItem(InventoryItemIDs.DiamondWall, 1);
				default:
					return null;
				}
				break;
			}
		}

		private static void ProcessHarvestExplosion(BlockTypeEnum blockType, IntVector3 intPos)
		{
			PossibleLootType.PlaceWorldItem(Explosive.GetHarvestedItem(blockType), intPos);
		}

		private static void ProcessExplosionDependents(Set<IntVector3> blocksToRemove, Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove)
		{
			foreach (IntVector3 intVector in dependentsToRemove.Keys)
			{
				if (!blocksToRemove.Contains(intVector))
				{
					BlockTypeEnum blockTypeEnum = dependentsToRemove[intVector];
					InventoryItem.InventoryItemClass inventoryItemClass = BlockInventoryItemClass.BlockClasses[BlockType.GetType(blockTypeEnum).ParentBlockType];
					PickupManager.Instance.CreatePickup(inventoryItemClass.CreateItem(1), IntVector3.ToVector3(intVector) + new Vector3(0.5f, 0.5f, 0.5f), false, false);
					blocksToRemove.Add(intVector);
					if (BlockType.IsLowerDoor(blockTypeEnum))
					{
						blocksToRemove.Add(intVector + new IntVector3(0, 1, 0));
					}
				}
			}
		}

		public static void FindBlocksToRemove(IntVector3 pos, ExplosiveTypes extype, bool showExplosionFlash)
		{
			Queue<Explosive> queue = new Queue<Explosive>();
			Set<IntVector3> set = new Set<IntVector3>();
			Dictionary<IntVector3, BlockTypeEnum> dictionary = new Dictionary<IntVector3, BlockTypeEnum>();
			queue.Enqueue(new Explosive(pos, extype));
			if (extype == ExplosiveTypes.C4 || extype == ExplosiveTypes.TNT)
			{
				set.Add(pos);
			}
			bool flag = showExplosionFlash;
			while (queue.Count > 0)
			{
				Explosive.ProcessOneExplosion(queue, set, dictionary, ref flag);
			}
			Explosive.ProcessExplosionDependents(set, dictionary);
			IntVector3[] array = new IntVector3[set.Count];
			set.CopyTo(array);
			RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, array.Length, array, false);
		}

		public static Explosive.EnemyBreakBlocksResult EnemyBreakBlocks(IntVector3 minCorner, IntVector3 maxCorner, int hits, int maxHardness, bool enemyIsLocallyOwned)
		{
			IntVector3 intVector = IntVector3.Subtract(minCorner, BlockTerrain.Instance._worldMin);
			IntVector3 intVector2 = IntVector3.Subtract(maxCorner, BlockTerrain.Instance._worldMin);
			intVector = IntVector3.Clamp(intVector, IntVector3.Zero, Explosive._sMaxBufferBounds);
			intVector2 = IntVector3.Clamp(intVector2, IntVector3.Zero, Explosive._sMaxBufferBounds);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			IntVector3 intVector3;
			intVector3.Z = intVector.Z;
			while (intVector3.Z <= intVector2.Z)
			{
				intVector3.X = intVector.X;
				while (intVector3.X <= intVector2.X)
				{
					intVector3.Y = intVector.Y;
					while (intVector3.Y <= intVector2.Y)
					{
						BlockTypeEnum typeIndex = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(intVector3));
						if (typeIndex != BlockTypeEnum.Empty && typeIndex != BlockTypeEnum.NumberOfBlocks)
						{
							BlockType type = BlockType.GetType(typeIndex);
							if (type.BlockPlayer)
							{
								flag3 = true;
							}
							if (type.Hardness <= maxHardness)
							{
								flag = true;
								if (hits > type.Hardness && MathTools.RandomBool())
								{
									flag2 = true;
									IntVector3 intVector4 = intVector3 + BlockTerrain.Instance._worldMin;
									if (typeIndex == BlockTypeEnum.TNT || typeIndex == BlockTypeEnum.C4)
									{
										ExplosiveTypes explosiveTypes = ((typeIndex == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
										Explosive._sEnemyDiggingTNTToExplode.Enqueue(new Explosive(intVector4, explosiveTypes));
										DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector4, false, explosiveTypes);
										Explosive._sEnemyDiggingBlocksToRemove.Add(intVector4);
									}
									else if (!BlockType.IsUpperDoor(typeIndex))
									{
										Explosive._sEnemyDiggingBlocksToRemove.Add(intVector4);
										if (BlockType.IsContainer(typeIndex) && enemyIsLocallyOwned)
										{
											DestroyCrateMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector4);
											Crate crate;
											if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(intVector4, out crate))
											{
												crate.EjectContents();
											}
										}
										if (BlockType.IsDoor(typeIndex))
										{
											DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, intVector4, typeIndex);
										}
										Explosive.RememberDependentObjects(intVector4, Explosive._sEnemyDiggingDependentsToRemove);
										if (BlockType.IsLowerDoor(typeIndex))
										{
											IntVector3 intVector5 = intVector4;
											intVector5.Y++;
											if (!Explosive._sEnemyDiggingBlocksToRemove.Contains(intVector5))
											{
												Explosive._sEnemyDiggingBlocksToRemove.Add(intVector5);
												Explosive.RememberDependentObjects(intVector5, Explosive._sEnemyDiggingDependentsToRemove);
											}
										}
									}
								}
								else if (enemyIsLocallyOwned && (typeIndex == BlockTypeEnum.TNT || typeIndex == BlockTypeEnum.C4))
								{
									InGameHUD.Instance.SetFuseForExplosive(intVector3 + BlockTerrain.Instance._worldMin, (typeIndex == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
								}
							}
						}
						intVector3.Y++;
					}
					intVector3.X++;
				}
				intVector3.Z++;
			}
			if (enemyIsLocallyOwned && Explosive._sEnemyDiggingBlocksToRemove.Count != 0)
			{
				Explosive.ProcessExplosionDependents(Explosive._sEnemyDiggingBlocksToRemove, Explosive._sEnemyDiggingDependentsToRemove);
				IntVector3[] array = new IntVector3[Explosive._sEnemyDiggingBlocksToRemove.Count];
				Explosive._sEnemyDiggingBlocksToRemove.CopyTo(array);
				RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, array.Length, array, true);
				if (Explosive._sEnemyDiggingTNTToExplode.Count != 0)
				{
					bool flag4 = true;
					while (Explosive._sEnemyDiggingTNTToExplode.Count > 0)
					{
						Explosive.ProcessOneExplosion(Explosive._sEnemyDiggingTNTToExplode, Explosive._sEnemyDiggingBlocksToRemove, Explosive._sEnemyDiggingDependentsToRemove, ref flag4);
					}
					Explosive.ProcessExplosionDependents(Explosive._sEnemyDiggingBlocksToRemove, Explosive._sEnemyDiggingDependentsToRemove);
					array = new IntVector3[Explosive._sEnemyDiggingBlocksToRemove.Count];
					Explosive._sEnemyDiggingBlocksToRemove.CopyTo(array);
					RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, array.Length, array, false);
				}
			}
			Explosive._sEnemyDiggingTNTToExplode.Clear();
			Explosive._sEnemyDiggingDependentsToRemove.Clear();
			Explosive._sEnemyDiggingBlocksToRemove.Clear();
			if (flag2)
			{
				return Explosive.EnemyBreakBlocksResult.BlocksBroken;
			}
			if (!flag3)
			{
				return Explosive.EnemyBreakBlocksResult.RegionIsEmpty;
			}
			if (flag)
			{
				return Explosive.EnemyBreakBlocksResult.BlocksWillBreak;
			}
			return Explosive.EnemyBreakBlocksResult.BlocksWillNotBreak;
		}

		private static bool BlockWithinLevelBlastRange(IntVector3 offset, BlockTypeEnum block, ExplosiveTypes explosiveType)
		{
			int num = ((explosiveType == ExplosiveTypes.TNT || explosiveType == ExplosiveTypes.Rocket) ? 1 : 2);
			int num2 = ((explosiveType == ExplosiveTypes.TNT) ? 1 : 1);
			int num3;
			if (Explosive.Level2Hardness[(int)block])
			{
				num3 = num2;
			}
			else
			{
				if (!Explosive.Level1Hardness[(int)block])
				{
					return false;
				}
				num3 = num;
			}
			return offset.X >= -num3 && offset.X <= num3 && offset.Y >= -num3 && offset.Y <= num3 && offset.Z >= -num3 && offset.Z <= num3;
		}

		public static void HandleRemoveBlocksMessage(RemoveBlocksMessage msg)
		{
			if (msg.DoDigEffects)
			{
				for (int i = 0; i < msg.NumBlocks; i++)
				{
					BlockTerrain.Instance.SetBlock(msg.BlocksToRemove[i], BlockTypeEnum.Empty);
					Explosive.AddDigEffects(IntVector3.ToVector3(msg.BlocksToRemove[i]) + Explosive._sHalf);
				}
				return;
			}
			for (int j = 0; j < msg.NumBlocks; j++)
			{
				BlockTerrain.Instance.SetBlock(msg.BlocksToRemove[j], BlockTypeEnum.Empty);
			}
		}

		public static void AddDigEffects(Vector3 position)
		{
			if (TracerManager.Instance != null)
			{
				Scene scene = TracerManager.Instance.Scene;
				if (scene == null || scene.Children == null)
				{
					return;
				}
				AudioEmitter audioEmitter = new AudioEmitter();
				audioEmitter.Position = position;
				SoundManager.Instance.PlayInstance("GroundCrash", audioEmitter);
				ParticleEmitter particleEmitter = Explosive._digSmokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				ParticleEmitter particleEmitter2 = Explosive._digRocksEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter2.Reset();
				particleEmitter2.Emitting = true;
				particleEmitter2.DrawPriority = 900;
				scene.Children.Add(particleEmitter2);
				Vector3 vector = position - CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				float num = vector.LengthSquared();
				if ((double)num > 1E-06)
				{
					vector.Normalize();
				}
				else
				{
					vector = Vector3.Forward;
				}
				Vector3 vector2 = Vector3.Cross(Vector3.Forward, vector);
				Quaternion quaternion = Quaternion.CreateFromAxisAngle(vector2, Vector3.Forward.AngleBetween(vector).Radians);
				Entity entity = particleEmitter2;
				particleEmitter.LocalPosition = position;
				entity.LocalPosition = position;
				particleEmitter2.LocalRotation = (particleEmitter.LocalRotation = quaternion);
			}
		}

		public static void AddEffects(Vector3 Position, bool wantRockChunks)
		{
			AudioEmitter audioEmitter = new AudioEmitter();
			audioEmitter.Position = Position;
			SoundManager.Instance.PlayInstance("Explosion", audioEmitter);
			if (TracerManager.Instance != null)
			{
				Scene scene = TracerManager.Instance.Scene;
				if (scene == null || scene.Children == null)
				{
					return;
				}
				ParticleEmitter particleEmitter = Explosive._flashEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalPosition = Position;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				particleEmitter = Explosive._firePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalPosition = Position;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				particleEmitter = Explosive._smokePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalPosition = Position;
				particleEmitter.DrawPriority = 900;
				scene.Children.Add(particleEmitter);
				if (wantRockChunks)
				{
					particleEmitter = Explosive._rockBlastEffect.CreateEmitter(CastleMinerZGame.Instance);
					particleEmitter.Reset();
					particleEmitter.Emitting = true;
					particleEmitter.LocalPosition = Position;
					particleEmitter.DrawPriority = 900;
					scene.Children.Add(particleEmitter);
				}
			}
		}

		private const float BLOCK_DAMAGE_RADIUS = 5.5f;

		private static readonly int[] cDestructionRanges = new int[] { 2, 3, 3, 0, 1, 0 };

		private static readonly float[] cEnemyDamageRanges = new float[] { 12f, 24f, 15f, 1f, 8f, 1f };

		private static readonly float[] cDamageRanges = new float[] { 6f, 12f, 7f, 1f, 5f, 1f };

		private static readonly float[] cKillRanges = new float[] { 3f, 6f, 4f, 1f, 2.5f, 1f };

		public IntVector3 Position;

		public OneShotTimer Timer = new OneShotTimer(TimeSpan.FromSeconds(4.0));

		public ExplosiveTypes ExplosiveType;

		private static ParticleEffect _flashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FlashEffect");

		private static ParticleEffect _firePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FirePuff");

		private static ParticleEffect _smokePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigSmokePuff");

		private static ParticleEffect _rockBlastEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigRockBlast");

		private static ParticleEffect _digSmokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeEffect");

		private static ParticleEffect _digRocksEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\RocksEffect");

		private static bool[,] BreakLookup = new bool[6, 95];

		private static bool[] Level1Hardness = new bool[95];

		private static bool[] Level2Hardness = new bool[95];

		private static readonly IntVector3 _sMaxBufferBounds = new IntVector3(383, 127, 383);

		private static Queue<Explosive> _sEnemyDiggingTNTToExplode = new Queue<Explosive>();

		private static Set<IntVector3> _sEnemyDiggingBlocksToRemove = new Set<IntVector3>();

		private static Dictionary<IntVector3, BlockTypeEnum> _sEnemyDiggingDependentsToRemove = new Dictionary<IntVector3, BlockTypeEnum>();

		private static readonly Vector3 _sHalf = new Vector3(0.5f);

		public enum EnemyBreakBlocksResult
		{
			BlocksWillBreak,
			BlocksWillNotBreak,
			BlocksBroken,
			RegionIsEmpty
		}
	}
}
