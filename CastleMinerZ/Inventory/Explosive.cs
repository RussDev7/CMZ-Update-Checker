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
				BlockTypeEnum blockType = BlockTerrain.Instance.GetBlockWithChanges(this.Position);
				BlockTypeEnum explosiveBlock = ((this.ExplosiveType == ExplosiveTypes.TNT) ? BlockTypeEnum.TNT : BlockTypeEnum.C4);
				if (this.Timer.Expired && blockType == explosiveBlock)
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
				Vector3 playerPos = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				playerPos.Y += 1f;
				float distance = Vector3.Distance(playerPos, location);
				float killRange = Explosive.cKillRanges[(int)explosiveType];
				float damageRange = Explosive.cDamageRanges[(int)explosiveType];
				if (distance < damageRange)
				{
					DamageLOSProbe _damageLOSProbe = new DamageLOSProbe();
					_damageLOSProbe.Init(location, playerPos);
					_damageLOSProbe.DragonTypeIndex = 0;
					BlockTerrain.Instance.Trace(_damageLOSProbe);
					float damage;
					if (distance < killRange)
					{
						damage = 1f;
					}
					else
					{
						damage = _damageLOSProbe.TotalDamageMultiplier * (1f - (distance - killRange) / (damageRange - killRange));
					}
					InGameHUD.Instance.ApplyDamage(damage, location);
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
			for (BlockFace bf = BlockFace.POSX; bf < BlockFace.NUM_FACES; bf++)
			{
				IntVector3 nb = BlockTerrain.Instance.GetNeighborIndex(worldIndex, bf);
				if (!dependentsToRemove.ContainsKey(nb))
				{
					BlockTypeEnum bbt = BlockTerrain.Instance.GetBlockWithChanges(nb);
					if (BlockType.GetType(bbt).Facing == bf)
					{
						dependentsToRemove.Add(nb, bbt);
					}
				}
			}
		}

		private static void ProcessOneExplosion(Queue<Explosive> tntToExplode, Set<IntVector3> blocksToRemove, Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove, ref bool explosionFlashNotYetShown)
		{
			Explosive currentTNT = tntToExplode.Dequeue();
			if (explosionFlashNotYetShown && (currentTNT.ExplosiveType == ExplosiveTypes.C4 || currentTNT.ExplosiveType == ExplosiveTypes.TNT))
			{
				AddExplosionEffectsMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, currentTNT.Position);
				explosionFlashNotYetShown = false;
			}
			int blowExplosiveRange = Explosive.cDestructionRanges[(int)currentTNT.ExplosiveType];
			IntVector3 offset = IntVector3.Zero;
			offset.X = -blowExplosiveRange;
			while (offset.X <= blowExplosiveRange)
			{
				IntVector3 currentBlockPos;
				currentBlockPos.X = currentTNT.Position.X + offset.X;
				IntVector3 local;
				local.X = currentBlockPos.X - BlockTerrain.Instance._worldMin.X;
				if (local.X >= 0 && local.X < 384)
				{
					offset.Z = -blowExplosiveRange;
					while (offset.Z <= blowExplosiveRange)
					{
						currentBlockPos.Z = currentTNT.Position.Z + offset.Z;
						local.Z = currentBlockPos.Z - BlockTerrain.Instance._worldMin.Z;
						if (local.Z >= 0 && local.Z < 384)
						{
							offset.Y = -blowExplosiveRange;
							while (offset.Y <= blowExplosiveRange)
							{
								currentBlockPos.Y = currentTNT.Position.Y + offset.Y;
								local.Y = currentBlockPos.Y - BlockTerrain.Instance._worldMin.Y;
								if (local.Y >= 0 && local.Y < 128 && !blocksToRemove.Contains(currentBlockPos))
								{
									BlockTypeEnum blockType = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(local));
									if (blockType == BlockTypeEnum.TNT || blockType == BlockTypeEnum.C4)
									{
										ExplosiveTypes explosiveType = ((blockType == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
										tntToExplode.Enqueue(new Explosive(currentBlockPos, explosiveType));
										DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, currentBlockPos, false, explosiveType);
										blocksToRemove.Add(currentBlockPos);
									}
									else if (!Explosive.BreakLookup[(int)currentTNT.ExplosiveType, (int)blockType] && Explosive.BlockWithinLevelBlastRange(offset, blockType, currentTNT.ExplosiveType) && !BlockType.IsUpperDoor(blockType))
									{
										blocksToRemove.Add(currentBlockPos);
										if (BlockType.IsContainer(blockType))
										{
											DestroyCrateMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, currentBlockPos);
											Crate crate;
											if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(currentBlockPos, out crate))
											{
												crate.EjectContents();
											}
										}
										if (BlockType.IsDoor(blockType))
										{
											DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, currentBlockPos, blockType);
										}
										Explosive.RememberDependentObjects(currentBlockPos, dependentsToRemove);
										if (BlockType.IsLowerDoor(blockType))
										{
											IntVector3 upperDoor = currentBlockPos;
											upperDoor.Y++;
											if (!blocksToRemove.Contains(upperDoor))
											{
												blocksToRemove.Add(upperDoor);
												Explosive.RememberDependentObjects(upperDoor, dependentsToRemove);
											}
										}
										if (currentTNT.ExplosiveType == ExplosiveTypes.Harvest)
										{
											Explosive.ProcessHarvestExplosion(blockType, currentBlockPos);
										}
										if (BlockType.ShouldDropLoot(blockType))
										{
											PossibleLootType.ProcessLootBlockOutput(blockType, currentBlockPos);
										}
									}
								}
								offset.Y++;
							}
						}
						offset.Z++;
					}
				}
				offset.X++;
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
			foreach (IntVector3 depPos in dependentsToRemove.Keys)
			{
				if (!blocksToRemove.Contains(depPos))
				{
					BlockTypeEnum bt = dependentsToRemove[depPos];
					InventoryItem.InventoryItemClass bic = BlockInventoryItemClass.BlockClasses[BlockType.GetType(bt).ParentBlockType];
					PickupManager.Instance.CreatePickup(bic.CreateItem(1), IntVector3.ToVector3(depPos) + new Vector3(0.5f, 0.5f, 0.5f), false, false);
					blocksToRemove.Add(depPos);
					if (BlockType.IsLowerDoor(bt))
					{
						blocksToRemove.Add(depPos + new IntVector3(0, 1, 0));
					}
				}
			}
		}

		public static void FindBlocksToRemove(IntVector3 pos, ExplosiveTypes extype, bool showExplosionFlash)
		{
			Queue<Explosive> tntToExplode = new Queue<Explosive>();
			Set<IntVector3> blocksToRemove = new Set<IntVector3>();
			Dictionary<IntVector3, BlockTypeEnum> dependentsToRemove = new Dictionary<IntVector3, BlockTypeEnum>();
			tntToExplode.Enqueue(new Explosive(pos, extype));
			if (extype == ExplosiveTypes.C4 || extype == ExplosiveTypes.TNT)
			{
				blocksToRemove.Add(pos);
			}
			bool explosionFlashNotYetShown = showExplosionFlash;
			while (tntToExplode.Count > 0)
			{
				Explosive.ProcessOneExplosion(tntToExplode, blocksToRemove, dependentsToRemove, ref explosionFlashNotYetShown);
			}
			Explosive.ProcessExplosionDependents(blocksToRemove, dependentsToRemove);
			IntVector3[] blocks = new IntVector3[blocksToRemove.Count];
			blocksToRemove.CopyTo(blocks);
			RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, blocks.Length, blocks, false);
		}

		public static Explosive.EnemyBreakBlocksResult EnemyBreakBlocks(IntVector3 minCorner, IntVector3 maxCorner, int hits, int maxHardness, bool enemyIsLocallyOwned)
		{
			IntVector3 localMinCorner = IntVector3.Subtract(minCorner, BlockTerrain.Instance._worldMin);
			IntVector3 localMaxCorner = IntVector3.Subtract(maxCorner, BlockTerrain.Instance._worldMin);
			localMinCorner = IntVector3.Clamp(localMinCorner, IntVector3.Zero, Explosive._sMaxBufferBounds);
			localMaxCorner = IntVector3.Clamp(localMaxCorner, IntVector3.Zero, Explosive._sMaxBufferBounds);
			bool breakableBlocks = false;
			bool blocksBroken = false;
			bool foundABlock = false;
			IntVector3 w;
			w.Z = localMinCorner.Z;
			while (w.Z <= localMaxCorner.Z)
			{
				w.X = localMinCorner.X;
				while (w.X <= localMaxCorner.X)
				{
					w.Y = localMinCorner.Y;
					while (w.Y <= localMaxCorner.Y)
					{
						BlockTypeEnum bte = Block.GetTypeIndex(BlockTerrain.Instance.GetBlockAt(w));
						if (bte != BlockTypeEnum.Empty && bte != BlockTypeEnum.NumberOfBlocks)
						{
							BlockType bt = BlockType.GetType(bte);
							if (bt.BlockPlayer)
							{
								foundABlock = true;
							}
							if (bt.Hardness <= maxHardness)
							{
								breakableBlocks = true;
								if (hits > bt.Hardness && MathTools.RandomBool())
								{
									blocksBroken = true;
									IntVector3 currentBlock = w + BlockTerrain.Instance._worldMin;
									if (bte == BlockTypeEnum.TNT || bte == BlockTypeEnum.C4)
									{
										ExplosiveTypes explosiveType = ((bte == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
										Explosive._sEnemyDiggingTNTToExplode.Enqueue(new Explosive(currentBlock, explosiveType));
										DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, currentBlock, false, explosiveType);
										Explosive._sEnemyDiggingBlocksToRemove.Add(currentBlock);
									}
									else if (!BlockType.IsUpperDoor(bte))
									{
										Explosive._sEnemyDiggingBlocksToRemove.Add(currentBlock);
										if (BlockType.IsContainer(bte) && enemyIsLocallyOwned)
										{
											DestroyCrateMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, currentBlock);
											Crate crate;
											if (CastleMinerZGame.Instance.CurrentWorld.Crates.TryGetValue(currentBlock, out crate))
											{
												crate.EjectContents();
											}
										}
										if (BlockType.IsDoor(bte))
										{
											DestroyCustomBlockMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, currentBlock, bte);
										}
										Explosive.RememberDependentObjects(currentBlock, Explosive._sEnemyDiggingDependentsToRemove);
										if (BlockType.IsLowerDoor(bte))
										{
											IntVector3 upperDoor = currentBlock;
											upperDoor.Y++;
											if (!Explosive._sEnemyDiggingBlocksToRemove.Contains(upperDoor))
											{
												Explosive._sEnemyDiggingBlocksToRemove.Add(upperDoor);
												Explosive.RememberDependentObjects(upperDoor, Explosive._sEnemyDiggingDependentsToRemove);
											}
										}
									}
								}
								else if (enemyIsLocallyOwned && (bte == BlockTypeEnum.TNT || bte == BlockTypeEnum.C4))
								{
									InGameHUD.Instance.SetFuseForExplosive(w + BlockTerrain.Instance._worldMin, (bte == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
								}
							}
						}
						w.Y++;
					}
					w.X++;
				}
				w.Z++;
			}
			if (enemyIsLocallyOwned && Explosive._sEnemyDiggingBlocksToRemove.Count != 0)
			{
				Explosive.ProcessExplosionDependents(Explosive._sEnemyDiggingBlocksToRemove, Explosive._sEnemyDiggingDependentsToRemove);
				IntVector3[] blocks = new IntVector3[Explosive._sEnemyDiggingBlocksToRemove.Count];
				Explosive._sEnemyDiggingBlocksToRemove.CopyTo(blocks);
				RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, blocks.Length, blocks, true);
				if (Explosive._sEnemyDiggingTNTToExplode.Count != 0)
				{
					bool explosionFlashNotYetShown = true;
					while (Explosive._sEnemyDiggingTNTToExplode.Count > 0)
					{
						Explosive.ProcessOneExplosion(Explosive._sEnemyDiggingTNTToExplode, Explosive._sEnemyDiggingBlocksToRemove, Explosive._sEnemyDiggingDependentsToRemove, ref explosionFlashNotYetShown);
					}
					Explosive.ProcessExplosionDependents(Explosive._sEnemyDiggingBlocksToRemove, Explosive._sEnemyDiggingDependentsToRemove);
					blocks = new IntVector3[Explosive._sEnemyDiggingBlocksToRemove.Count];
					Explosive._sEnemyDiggingBlocksToRemove.CopyTo(blocks);
					RemoveBlocksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, blocks.Length, blocks, false);
				}
			}
			Explosive._sEnemyDiggingTNTToExplode.Clear();
			Explosive._sEnemyDiggingDependentsToRemove.Clear();
			Explosive._sEnemyDiggingBlocksToRemove.Clear();
			if (blocksBroken)
			{
				return Explosive.EnemyBreakBlocksResult.BlocksBroken;
			}
			if (!foundABlock)
			{
				return Explosive.EnemyBreakBlocksResult.RegionIsEmpty;
			}
			if (breakableBlocks)
			{
				return Explosive.EnemyBreakBlocksResult.BlocksWillBreak;
			}
			return Explosive.EnemyBreakBlocksResult.BlocksWillNotBreak;
		}

		private static bool BlockWithinLevelBlastRange(IntVector3 offset, BlockTypeEnum block, ExplosiveTypes explosiveType)
		{
			int level1Range = ((explosiveType == ExplosiveTypes.TNT || explosiveType == ExplosiveTypes.Rocket) ? 1 : 2);
			int level2Range = ((explosiveType == ExplosiveTypes.TNT) ? 1 : 1);
			int range;
			if (Explosive.Level2Hardness[(int)block])
			{
				range = level2Range;
			}
			else
			{
				if (!Explosive.Level1Hardness[(int)block])
				{
					return false;
				}
				range = level1Range;
			}
			return offset.X >= -range && offset.X <= range && offset.Y >= -range && offset.Y <= range && offset.Z >= -range && offset.Z <= range;
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
			if (TracerManager.Instance != null && CastleMinerZGame.Instance.IsActive)
			{
				Scene scene = TracerManager.Instance.Scene;
				if (scene == null || scene.Children == null)
				{
					return;
				}
				AudioEmitter soundEmitter = new AudioEmitter();
				soundEmitter.Position = position;
				SoundManager.Instance.PlayInstance("GroundCrash", soundEmitter);
				ParticleEmitter smokeEmitter = Explosive._digSmokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				smokeEmitter.Reset();
				smokeEmitter.Emitting = true;
				smokeEmitter.DrawPriority = 900;
				scene.Children.Add(smokeEmitter);
				ParticleEmitter rockEmitter = Explosive._digRocksEffect.CreateEmitter(CastleMinerZGame.Instance);
				rockEmitter.Reset();
				rockEmitter.Emitting = true;
				rockEmitter.DrawPriority = 900;
				scene.Children.Add(rockEmitter);
				Vector3 dir = position - CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
				float lsq = dir.LengthSquared();
				if ((double)lsq > 1E-06)
				{
					dir.Normalize();
				}
				else
				{
					dir = Vector3.Forward;
				}
				Vector3 axis = Vector3.Cross(Vector3.Forward, dir);
				Quaternion rot = Quaternion.CreateFromAxisAngle(axis, Vector3.Forward.AngleBetween(dir).Radians);
				Entity entity = rockEmitter;
				smokeEmitter.LocalPosition = position;
				entity.LocalPosition = position;
				rockEmitter.LocalRotation = (smokeEmitter.LocalRotation = rot);
			}
		}

		public static void AddEffects(Vector3 Position, bool wantRockChunks)
		{
			AudioEmitter SoundEmitter = new AudioEmitter();
			SoundEmitter.Position = Position;
			SoundManager.Instance.PlayInstance("Explosion", SoundEmitter);
			if (TracerManager.Instance != null && CastleMinerZGame.Instance.IsActive)
			{
				Scene scene = TracerManager.Instance.Scene;
				if (scene == null || scene.Children == null)
				{
					return;
				}
				ParticleEmitter emitter = Explosive._flashEffect.CreateEmitter(CastleMinerZGame.Instance);
				emitter.Reset();
				emitter.Emitting = true;
				emitter.LocalPosition = Position;
				emitter.DrawPriority = 900;
				scene.Children.Add(emitter);
				emitter = Explosive._firePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
				emitter.Reset();
				emitter.Emitting = true;
				emitter.LocalPosition = Position;
				emitter.DrawPriority = 900;
				scene.Children.Add(emitter);
				emitter = Explosive._smokePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
				emitter.Reset();
				emitter.Emitting = true;
				emitter.LocalPosition = Position;
				emitter.DrawPriority = 900;
				scene.Children.Add(emitter);
				if (wantRockChunks)
				{
					emitter = Explosive._rockBlastEffect.CreateEmitter(CastleMinerZGame.Instance);
					emitter.Reset();
					emitter.Emitting = true;
					emitter.LocalPosition = Position;
					emitter.DrawPriority = 900;
					scene.Children.Add(emitter);
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
