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
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				float num = Vector3.DistanceSquared(list[i].Position, pos);
				if (num < 0.01f)
				{
					return i;
				}
			}
			return -1;
		}

		protected void HandleOtherCommand(BlockTerrain.ItemBlockCommand ibc)
		{
			Vector3 vector = IntVector3.ToVector3(ibc.WorldPosition);
			vector += new Vector3(0.5f, 0.5f, 0.5f);
			if (ibc.AddItem)
			{
				if (BlockTerrain.Instance.IsInsideWorld(vector) && this.SearchListForItem(vector, this.BlockEntities) == -1)
				{
					ItemBlockEntityHolder itemBlockEntityHolder = ItemBlockEntityHolder.Alloc();
					itemBlockEntityHolder.Position = vector;
					itemBlockEntityHolder.BlockType = ibc.BlockType;
					this.BlockEntities.Add(itemBlockEntityHolder);
					return;
				}
			}
			else
			{
				int num = this.SearchListForItem(vector, this.BlockEntities);
				if (num != -1)
				{
					ItemBlockEntityHolder itemBlockEntityHolder2 = this.BlockEntities[num];
					if (itemBlockEntityHolder2.InWorldEntity != null)
					{
						itemBlockEntityHolder2.InWorldEntity.RemoveFromParent();
					}
					itemBlockEntityHolder2.Release();
					int num2 = this.BlockEntities.Count - 1;
					if (num < num2)
					{
						this.BlockEntities[num] = this.BlockEntities[num2];
					}
					this.BlockEntities.RemoveAt(num2);
				}
			}
		}

		protected void HandleTorchCommand(BlockTerrain.ItemBlockCommand ibc)
		{
			if (this._torchCloud != null)
			{
				Vector3 vector = IntVector3.ToVector3(ibc.WorldPosition);
				vector += new Vector3(0.5f, 0.5f, 0.5f);
				if (ibc.AddItem)
				{
					this._torchCloud.AddTorch(vector, BlockType.GetType(ibc.BlockType).Facing);
					return;
				}
				this._torchCloud.RemoveTorch(vector);
			}
		}

		protected void HandleLanternCommand(BlockTerrain.ItemBlockCommand ibc)
		{
			Vector3 vector = IntVector3.ToVector3(ibc.WorldPosition);
			vector += new Vector3(0.5f, 0.5f, 0.5f);
			int num = this.LanternLocations.Count;
			bool flag = false;
			float num2 = -1f;
			int num3 = -1;
			if (num >= 500 && ibc.AddItem)
			{
				for (int i = 0; i < num; i++)
				{
					float num4 = Vector3.DistanceSquared(this.LanternLocations[i], vector);
					if (num4 < 0.01f)
					{
						flag = true;
						break;
					}
					if (num4 > num2)
					{
						num2 = num4;
						num3 = i;
					}
				}
			}
			else
			{
				int j = 0;
				while (j < num)
				{
					if (Vector3.DistanceSquared(this.LanternLocations[j], vector) < 0.01f)
					{
						flag = true;
						if (!ibc.AddItem)
						{
							num--;
							if (j < num)
							{
								this.LanternLocations[j] = this.LanternLocations[num];
							}
							this.LanternLocations.RemoveAt(num);
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
			if (ibc.AddItem && !flag && BlockTerrain.Instance.IsInsideWorld(vector))
			{
				if (num3 >= 0)
				{
					this.LanternLocations[num3] = vector;
					return;
				}
				this.LanternLocations.Add(vector);
			}
		}

		private void RemoveOutOfBounders(List<ItemBlockEntityHolder> list)
		{
			int num = list.Count;
			int i = 0;
			while (i < num)
			{
				if (!BlockTerrain.Instance.IsInsideWorld(list[i].Position))
				{
					if (list[i].InWorldEntity != null)
					{
						list[i].InWorldEntity.RemoveFromParent();
					}
					list[i].Release();
					num--;
					if (i < num)
					{
						list[i] = list[num];
					}
					list.RemoveAt(num);
				}
				else
				{
					i++;
				}
			}
		}

		private void CreateEntity(ItemBlockEntityHolder h)
		{
			ItemBlockInventoryItemClass itemBlockInventoryItemClass = (ItemBlockInventoryItemClass)BlockInventoryItemClass.BlockClasses[BlockType.GetType(h.BlockType).ParentBlockType];
			Door door = CastleMinerZGame.Instance.CurrentWorld.GetDoor(IntVector3.FromVector3(h.Position));
			DoorEntity.ModelNameEnum modelNameEnum = ((door != null) ? door.ModelName : DoorEntity.ModelNameEnum.None);
			Entity entity = itemBlockInventoryItemClass.CreateWorldEntity(false, h.BlockType, modelNameEnum);
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
					BlockTerrain.ItemBlockCommand itemBlockCommand = BlockTerrain.Instance.ItemBlockCommandQueue.Dequeue();
					BlockTypeEnum parentBlockType = BlockType.GetType(itemBlockCommand.BlockType).ParentBlockType;
					BlockTypeEnum blockTypeEnum = parentBlockType;
					if (blockTypeEnum != BlockTypeEnum.Lantern)
					{
						if (blockTypeEnum != BlockTypeEnum.Torch)
						{
							if (blockTypeEnum == BlockTypeEnum.LanternFancy)
							{
								this.HandleLanternCommand(itemBlockCommand);
							}
							else
							{
								this.HandleOtherCommand(itemBlockCommand);
							}
						}
						else
						{
							this.HandleTorchCommand(itemBlockCommand);
						}
					}
					else
					{
						this.HandleLanternCommand(itemBlockCommand);
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
				int count = this.BlockEntities.Count;
				while (this._currentWalkLocation < count)
				{
					ItemBlockEntityHolder itemBlockEntityHolder = this.BlockEntities[this._currentWalkLocation++];
					itemBlockEntityHolder.Distance = Vector3.DistanceSquared(itemBlockEntityHolder.Position, worldPosition);
					int num = this.Accumulator.Count;
					if (num == 0)
					{
						this.Accumulator.Add(itemBlockEntityHolder);
					}
					else
					{
						num--;
						while (num >= 0 && itemBlockEntityHolder.Distance < this.Accumulator[num].Distance)
						{
							num--;
						}
						this.Accumulator.Insert(num + 1, itemBlockEntityHolder);
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
				int count2 = this.Accumulator.Count;
				int num2 = Math.Min(count2, 100);
				while (this._currentWalkLocation < num2)
				{
					ItemBlockEntityHolder itemBlockEntityHolder2 = this.Accumulator[this._currentWalkLocation++];
					if (itemBlockEntityHolder2.InWorldEntity == null)
					{
						this.CreateEntity(itemBlockEntityHolder2);
					}
					itemBlockEntityHolder2.TorchFlame = true;
					itemBlockEntityHolder2.TimeUntilFlameRemovalAllowed = 1f;
					itemBlockEntityHolder2.TimeUntilTorchRemovalAllowed = 1f;
					if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
					{
						return;
					}
				}
				if (count2 > 100)
				{
					num2 = Math.Min(count2, 500);
					while (this._currentWalkLocation < num2)
					{
						ItemBlockEntityHolder itemBlockEntityHolder3 = this.Accumulator[this._currentWalkLocation++];
						if (itemBlockEntityHolder3.InWorldEntity == null)
						{
							this.CreateEntity(itemBlockEntityHolder3);
						}
						itemBlockEntityHolder3.TimeUntilTorchRemovalAllowed = 1f;
						if (itemBlockEntityHolder3.TorchFlame)
						{
							itemBlockEntityHolder3.TimeUntilFlameRemovalAllowed -= dt;
							if (itemBlockEntityHolder3.TimeUntilFlameRemovalAllowed <= 0f)
							{
								itemBlockEntityHolder3.TorchFlame = false;
							}
						}
						if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
						{
							return;
						}
					}
					if (count2 > 500)
					{
						while (this._currentWalkLocation < count2)
						{
							ItemBlockEntityHolder itemBlockEntityHolder4 = this.Accumulator[this._currentWalkLocation++];
							if (itemBlockEntityHolder4.InWorldEntity != null)
							{
								itemBlockEntityHolder4.TimeUntilTorchRemovalAllowed -= dt;
								if (itemBlockEntityHolder4.TimeUntilTorchRemovalAllowed <= 0f)
								{
									itemBlockEntityHolder4.InWorldEntity = null;
								}
							}
							if (this._queueTimer.ElapsedMilliseconds > this._maxMillis)
							{
								return;
							}
						}
					}
				}
				List<ItemBlockEntityHolder> accumulator = this.Accumulator;
				this.Accumulator = this.BlockEntities;
				this.BlockEntities = accumulator;
				this._currentWalkLocation = 0;
				this._listWalkState = ItemBlockEntityManager.ListWalkState.PROCESS_QUEUES;
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this.UpdateNearby((float)gameTime.ElapsedGameTime.TotalSeconds);
			int num = this.LanternLocations.Count;
			int i = 0;
			while (i < num)
			{
				if (!BlockTerrain.Instance.IsInsideWorld(this.LanternLocations[i]))
				{
					num--;
					if (i < num)
					{
						this.LanternLocations[i] = this.LanternLocations[num];
					}
					this.LanternLocations.RemoveAt(num);
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
			int count = this.LanternLocations.Count;
			float num = minDist * minDist;
			for (int i = 0; i < count; i++)
			{
				if (Vector3.DistanceSquared(pos, this.LanternLocations[i]) <= num)
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
