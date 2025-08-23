using System;
using System.Collections.Generic;
using System.Diagnostics;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class ItemBlockEntityManager : Entity
	{
		public ItemBlockEntityManager()
		{
			this.Collidee = false;
			this.Collider = false;
			ItemBlockEntityManager.Instance = this;
			this._torchCloud = new TorchCloud(CastleMinerZGame.Instance);
			base.Children.Add(this._torchCloud);
		}

		private int SearchListForItem(Vector3 pos, List<ItemBlockEntityHolder> list)
		{
			int c = list.Count;
			for (int i = 0; i < c; i++)
			{
				float d = Vector3.DistanceSquared(list[i].Position, pos);
				if (d < 0.01f)
				{
					return i;
				}
			}
			return -1;
		}

		protected void HandleOtherCommand(BlockTerrain.ItemBlockCommand ibc)
		{
			Vector3 sl = IntVector3.ToVector3(ibc.WorldPosition);
			sl += new Vector3(0.5f, 0.5f, 0.5f);
			if (ibc.AddItem)
			{
				if (BlockTerrain.Instance.IsInsideWorld(sl) && this.SearchListForItem(sl, this.BlockEntities) == -1)
				{
					ItemBlockEntityHolder ibh = ItemBlockEntityHolder.Alloc();
					ibh.Position = sl;
					ibh.BlockType = ibc.BlockType;
					this.BlockEntities.Add(ibh);
					return;
				}
			}
			else
			{
				int pos = this.SearchListForItem(sl, this.BlockEntities);
				if (pos != -1)
				{
					ItemBlockEntityHolder ibh2 = this.BlockEntities[pos];
					if (ibh2.InWorldEntity != null)
					{
						ibh2.InWorldEntity.RemoveFromParent();
					}
					ibh2.Release();
					int c = this.BlockEntities.Count - 1;
					if (pos < c)
					{
						this.BlockEntities[pos] = this.BlockEntities[c];
					}
					this.BlockEntities.RemoveAt(c);
				}
			}
		}

		protected void HandleTorchCommand(BlockTerrain.ItemBlockCommand ibc)
		{
			if (this._torchCloud != null)
			{
				Vector3 sl = IntVector3.ToVector3(ibc.WorldPosition);
				sl += new Vector3(0.5f, 0.5f, 0.5f);
				if (ibc.AddItem)
				{
					this._torchCloud.AddTorch(sl, BlockType.GetType(ibc.BlockType).Facing);
					return;
				}
				this._torchCloud.RemoveTorch(sl);
			}
		}

		protected void HandleLanternCommand(BlockTerrain.ItemBlockCommand ibc)
		{
			Vector3 sl = IntVector3.ToVector3(ibc.WorldPosition);
			sl += new Vector3(0.5f, 0.5f, 0.5f);
			int c = this.LanternLocations.Count;
			bool foundIt = false;
			float furthestDist = -1f;
			int furthestEntityIndex = -1;
			if (c >= 500 && ibc.AddItem)
			{
				for (int i = 0; i < c; i++)
				{
					float d = Vector3.DistanceSquared(this.LanternLocations[i], sl);
					if (d < 0.01f)
					{
						foundIt = true;
						break;
					}
					if (d > furthestDist)
					{
						furthestDist = d;
						furthestEntityIndex = i;
					}
				}
			}
			else
			{
				int j = 0;
				while (j < c)
				{
					if (Vector3.DistanceSquared(this.LanternLocations[j], sl) < 0.01f)
					{
						foundIt = true;
						if (!ibc.AddItem)
						{
							c--;
							if (j < c)
							{
								this.LanternLocations[j] = this.LanternLocations[c];
							}
							this.LanternLocations.RemoveAt(c);
							break;
						}
						break;
					}
					else
					{
						j++;
					}
				}
			}
			if (ibc.AddItem && !foundIt && BlockTerrain.Instance.IsInsideWorld(sl))
			{
				if (furthestEntityIndex >= 0)
				{
					this.LanternLocations[furthestEntityIndex] = sl;
					return;
				}
				this.LanternLocations.Add(sl);
			}
		}

		private void RemoveOutOfBounders(List<ItemBlockEntityHolder> list)
		{
			int c = list.Count;
			int i = 0;
			while (i < c)
			{
				if (!BlockTerrain.Instance.IsInsideWorld(list[i].Position))
				{
					if (list[i].InWorldEntity != null)
					{
						list[i].InWorldEntity.RemoveFromParent();
					}
					list[i].Release();
					c--;
					if (i < c)
					{
						list[i] = list[c];
					}
					list.RemoveAt(c);
				}
				else
				{
					i++;
				}
			}
		}

		private void CreateEntity(ItemBlockEntityHolder h)
		{
			ItemBlockInventoryItemClass ic = (ItemBlockInventoryItemClass)BlockInventoryItemClass.BlockClasses[BlockType.GetType(h.BlockType).ParentBlockType];
			Door door = CastleMinerZGame.Instance.CurrentWorld.GetDoor(IntVector3.FromVector3(h.Position));
			DoorEntity.ModelNameEnum modelName = ((door != null) ? door.ModelName : DoorEntity.ModelNameEnum.None);
			Entity entity = ic.CreateWorldEntity(false, h.BlockType, modelName);
			entity.LocalPosition = h.Position;
			entity.DrawPriority = 100;
			Scene scene = base.Scene;
			if (scene != null && scene.Children != null)
			{
				scene.Children.Add(entity);
			}
			h.InWorldEntity = entity;
		}

		private void UpdateNearby(float dt)
		{
			Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			this._queueTimer.Reset();
			this._queueTimer.Start();
			if (this._listWalkState == ItemBlockEntityManager.ListWalkState.PROCESS_QUEUES)
			{
				while (!BlockTerrain.Instance.ItemBlockCommandQueue.Empty)
				{
					BlockTerrain.ItemBlockCommand ibc = BlockTerrain.Instance.ItemBlockCommandQueue.Dequeue();
					BlockTypeEnum bt = BlockType.GetType(ibc.BlockType).ParentBlockType;
					BlockTypeEnum blockTypeEnum = bt;
					if (blockTypeEnum != BlockTypeEnum.Lantern)
					{
						if (blockTypeEnum != BlockTypeEnum.Torch)
						{
							if (blockTypeEnum == BlockTypeEnum.LanternFancy)
							{
								this.HandleLanternCommand(ibc);
							}
							else
							{
								this.HandleOtherCommand(ibc);
							}
						}
						else
						{
							this.HandleTorchCommand(ibc);
						}
					}
					else
					{
						this.HandleLanternCommand(ibc);
					}
					if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
					{
						return;
					}
				}
				this._currentWalkLocation = 0;
				this._listWalkState = ItemBlockEntityManager.ListWalkState.SORT;
				this.RemoveOutOfBounders(this.BlockEntities);
				if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
				{
					return;
				}
			}
			if (this._listWalkState == ItemBlockEntityManager.ListWalkState.SORT)
			{
				int c = this.BlockEntities.Count;
				while (this._currentWalkLocation < c)
				{
					ItemBlockEntityHolder h = this.BlockEntities[this._currentWalkLocation++];
					h.Distance = Vector3.DistanceSquared(h.Position, worldPosition);
					int i = this.Accumulator.Count;
					if (i == 0)
					{
						this.Accumulator.Add(h);
					}
					else
					{
						i--;
						while (i >= 0 && h.Distance < this.Accumulator[i].Distance)
						{
							i--;
						}
						this.Accumulator.Insert(i + 1, h);
					}
					if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
					{
						return;
					}
				}
				this.BlockEntities.Clear();
				this._currentWalkLocation = 0;
				this._listWalkState = ItemBlockEntityManager.ListWalkState.PRUNEANDSPAWN;
			}
			if (this._listWalkState == ItemBlockEntityManager.ListWalkState.PRUNEANDSPAWN)
			{
				int c2 = this.Accumulator.Count;
				int last = Math.Min(c2, 100);
				while (this._currentWalkLocation < last)
				{
					ItemBlockEntityHolder h2 = this.Accumulator[this._currentWalkLocation++];
					if (h2.InWorldEntity == null)
					{
						this.CreateEntity(h2);
					}
					h2.TorchFlame = true;
					h2.TimeUntilFlameRemovalAllowed = 1f;
					h2.TimeUntilTorchRemovalAllowed = 1f;
					if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
					{
						return;
					}
				}
				if (c2 > 100)
				{
					last = Math.Min(c2, 500);
					while (this._currentWalkLocation < last)
					{
						ItemBlockEntityHolder h3 = this.Accumulator[this._currentWalkLocation++];
						if (h3.InWorldEntity == null)
						{
							this.CreateEntity(h3);
						}
						h3.TimeUntilTorchRemovalAllowed = 1f;
						if (h3.TorchFlame)
						{
							h3.TimeUntilFlameRemovalAllowed -= dt;
							if (h3.TimeUntilFlameRemovalAllowed <= 0f)
							{
								h3.TorchFlame = false;
							}
						}
						if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
						{
							return;
						}
					}
					if (c2 > 500)
					{
						while (this._currentWalkLocation < c2)
						{
							ItemBlockEntityHolder h4 = this.Accumulator[this._currentWalkLocation++];
							if (h4.InWorldEntity != null)
							{
								h4.TimeUntilTorchRemovalAllowed -= dt;
								if (h4.TimeUntilTorchRemovalAllowed <= 0f)
								{
									h4.InWorldEntity = null;
								}
							}
							if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
							{
								return;
							}
						}
					}
				}
				List<ItemBlockEntityHolder> temp = this.Accumulator;
				this.Accumulator = this.BlockEntities;
				this.BlockEntities = temp;
				this._currentWalkLocation = 0;
				this._listWalkState = ItemBlockEntityManager.ListWalkState.PROCESS_QUEUES;
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this.UpdateNearby((float)gameTime.ElapsedGameTime.TotalSeconds);
			int c = this.LanternLocations.Count;
			int i = 0;
			while (i < c)
			{
				if (!BlockTerrain.Instance.IsInsideWorld(this.LanternLocations[i]))
				{
					c--;
					if (i < c)
					{
						this.LanternLocations[i] = this.LanternLocations[c];
					}
					this.LanternLocations.RemoveAt(c);
				}
				else
				{
					i++;
				}
			}
			base.OnUpdate(gameTime);
		}

		public bool NearLantern(Vector3 pos, float minDist)
		{
			int c = this.LanternLocations.Count;
			float dsq = minDist * minDist;
			for (int i = 0; i < c; i++)
			{
				if (Vector3.DistanceSquared(pos, this.LanternLocations[i]) <= dsq)
				{
					return true;
				}
			}
			return false;
		}

		private const int MAX_TORCHES = 500;

		private const int MAX_FLAMES = 100;

		private const int MAX_LANTERNS = 500;

		public static ItemBlockEntityManager Instance;

		public Random Rnd = new Random();

		public List<ItemBlockEntityHolder> BlockEntities = new List<ItemBlockEntityHolder>();

		public List<ItemBlockEntityHolder> Accumulator = new List<ItemBlockEntityHolder>();

		private TorchCloud _torchCloud;

		private int _currentWalkLocation;

		private ItemBlockEntityManager.ListWalkState _listWalkState;

		private Stopwatch _queueTimer = Stopwatch.StartNew();

		public List<Vector3> LanternLocations = new List<Vector3>();

		private long _maxMillis = 10L;

		private enum ListWalkState
		{
			PROCESS_QUEUES,
			SORT,
			DISTANT,
			PRUNEANDSPAWN
		}
	}
}
