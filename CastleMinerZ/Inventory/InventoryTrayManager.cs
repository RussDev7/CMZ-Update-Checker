using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class InventoryTrayManager
	{
		public int Version
		{
			get
			{
				return 0;
			}
		}

		private bool IsOutOfBounds(int trayIndex, int itemIndex)
		{
			int upperBound = this.Trays.GetUpperBound(0);
			int upperBound2 = this.Trays.GetUpperBound(1);
			return trayIndex > upperBound || itemIndex > upperBound2;
		}

		public InventoryItem GetItemFromCurrentTray(int itemIndex)
		{
			return this.Trays[this._currentTrayIndex, itemIndex];
		}

		public InventoryItem GetItemFromNextTray(int itemIndex)
		{
			int nextTrayIndex = this.GetNextTrayIndex(this._currentTrayIndex);
			return this.Trays[nextTrayIndex, itemIndex];
		}

		public InventoryItem GetTrayItem(int trayIndex, int itemIndex)
		{
			if (this.IsOutOfBounds(trayIndex, itemIndex))
			{
				return null;
			}
			return this.Trays[trayIndex, itemIndex];
		}

		public void SetTrayItem(int trayIndex, int itemIndex, InventoryItem item)
		{
			if (this.IsOutOfBounds(trayIndex, itemIndex))
			{
				return;
			}
			this.Trays[trayIndex, itemIndex] = item;
		}

		public void SetItem(int itemIndex, InventoryItem item)
		{
			this.Trays[this._currentTrayIndex, itemIndex] = item;
		}

		public bool RemoveItem(InventoryItem targetItem)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					this.Trays[i, j];
					if (this.Trays[i, j] == targetItem)
					{
						this.Trays[i, j] = null;
						return true;
					}
				}
			}
			return false;
		}

		public bool CanConsume(InventoryItem.InventoryItemClass itemClass, int amount)
		{
			int num = this.ForAllItems(delegate(InventoryItem trayItem)
			{
				if (trayItem != null && trayItem.CanConsume(itemClass, amount))
				{
					return 1;
				}
				return 0;
			});
			return num == 1;
		}

		internal void Update(GameTime gameTime)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = this.Trays[i, j];
					if (inventoryItem != null)
					{
						inventoryItem.Update(gameTime);
					}
				}
			}
		}

		internal int ForAllItems(TrayItemAction itemAction)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = this.Trays[i, j];
					if (inventoryItem != null)
					{
						int num = itemAction(inventoryItem);
						if (num != 0)
						{
							return num;
						}
					}
				}
			}
			return 0;
		}

		internal void ActOnEachItem(Action<InventoryItem> itemAction)
		{
			InventoryItem[,] trays = this.Trays;
			int upperBound = trays.GetUpperBound(0);
			int upperBound2 = trays.GetUpperBound(1);
			for (int i = trays.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = trays.GetLowerBound(1); j <= upperBound2; j++)
				{
					InventoryItem inventoryItem = trays[i, j];
					if (inventoryItem != null)
					{
						itemAction(inventoryItem);
					}
				}
			}
		}

		private int GetNextTrayIndex(int currentIndex)
		{
			int num = currentIndex + 1;
			if (num >= 2)
			{
				num = 0;
			}
			return num;
		}

		internal int GetItemClassCount(InventoryItem.InventoryItemClass itemClass)
		{
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = this.Trays[i, j];
					if (inventoryItem != null && itemClass == inventoryItem.ItemClass)
					{
						num += inventoryItem.StackCount;
					}
				}
			}
			return num;
		}

		internal void RemoveAllItems(bool onlyRemoveEmptyItems = false)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (this.Trays[i, j] != null && (!onlyRemoveEmptyItems || this.Trays[i, j].StackCount <= 0))
					{
						this.Trays[i, j] = null;
					}
				}
			}
		}

		internal bool CanAdd(InventoryItem item)
		{
			int num = this.ForAllItems(delegate(InventoryItem trayItem)
			{
				if (trayItem != null && trayItem.CanStack(item))
				{
					return 1;
				}
				return 0;
			});
			return num == 1;
		}

		internal void AddItemToExisting(InventoryItem item)
		{
			this.ActOnEachItem(delegate(InventoryItem trayItem)
			{
				if (trayItem != null)
				{
					trayItem.Stack(item);
				}
			});
		}

		internal bool AddItemToEmpty(InventoryItem item)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (this.Trays[i, j] == null)
					{
						this.Trays[i, j] = item;
						return true;
					}
				}
			}
			return false;
		}

		internal int AddItemToTray(InventoryItem item)
		{
			this.AddItemToExisting(item);
			if (item.StackCount <= 0)
			{
				return 0;
			}
			if (this.AddItemToEmpty(item))
			{
				return 0;
			}
			return item.StackCount;
		}

		internal void Write(BinaryWriter writer)
		{
			writer.Write(2);
			for (int i = 0; i < 2; i++)
			{
				writer.Write(8);
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = this.Trays[i, j];
					if (inventoryItem == null)
					{
						writer.Write(false);
					}
					else
					{
						writer.Write(true);
						inventoryItem.Write(writer);
					}
				}
			}
		}

		internal void Read(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					this.Trays[i, j];
					this.Trays[i, j] = null;
					if (reader.ReadBoolean())
					{
						InventoryItem inventoryItem = InventoryItem.Create(reader);
						if (inventoryItem != null && inventoryItem.IsValid())
						{
							this.Trays[i, j] = inventoryItem;
						}
					}
				}
			}
		}

		internal void ReadLegacy(BinaryReader reader)
		{
			for (int i = 0; i < 8; i++)
			{
				if (reader.ReadBoolean())
				{
					this.Trays[0, i] = InventoryItem.Create(reader);
					if (this.Trays[0, i] != null && !this.Trays[0, i].IsValid())
					{
						this.Trays[0, i] = null;
					}
				}
				else
				{
					this.Trays[0, i] = null;
				}
			}
		}

		internal InventoryItem Stack(InventoryItem sourceItem)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = this.Trays[i, j];
					if (inventoryItem != null)
					{
						InventoryItem inventoryItem2 = sourceItem;
						inventoryItem.Stack(inventoryItem2);
						sourceItem = inventoryItem2;
					}
				}
			}
			return sourceItem;
		}

		internal bool PlaceInEmptySlot(InventoryItem SelectedItem)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (this.Trays[i, j] == null)
					{
						this.Trays[i, j] = SelectedItem;
						return true;
					}
				}
			}
			return false;
		}

		public int CurrentTrayLength
		{
			get
			{
				return 8;
			}
		}

		internal bool Contains(InventoryItem sourceItem)
		{
			InventoryItem[,] trays = this.Trays;
			int upperBound = trays.GetUpperBound(0);
			int upperBound2 = trays.GetUpperBound(1);
			for (int i = trays.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = trays.GetLowerBound(1); j <= upperBound2; j++)
				{
					InventoryItem inventoryItem = trays[i, j];
					if (inventoryItem == sourceItem)
					{
						return true;
					}
				}
			}
			return false;
		}

		internal void SwitchCurrentTray()
		{
			this._currentTrayIndex++;
			if (this._currentTrayIndex >= 2)
			{
				this._currentTrayIndex = 0;
			}
		}

		public const int MaxTrayItems = 8;

		public const int MaxTrayCount = 2;

		public InventoryItem[,] Trays = new InventoryItem[2, 8];

		public InventoryItem[] InventoryTray = new InventoryItem[8];

		public InventoryItem[] InventoryTray2 = new InventoryItem[8];

		private int _currentTrayIndex;

		private enum InventoryTrayVersion
		{
			CurrentVersion
		}
	}
}
