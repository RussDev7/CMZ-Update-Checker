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
			int trayBounds = this.Trays.GetUpperBound(0);
			int itemBounds = this.Trays.GetUpperBound(1);
			return trayIndex > trayBounds || itemIndex > itemBounds;
		}

		public InventoryItem GetItemFromCurrentTray(int itemIndex)
		{
			return this.Trays[this._currentTrayIndex, itemIndex];
		}

		public InventoryItem GetItemFromNextTray(int itemIndex)
		{
			int index = this.GetNextTrayIndex(this._currentTrayIndex);
			return this.Trays[index, itemIndex];
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
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					this.Trays[trayNum, itemNum];
					if (this.Trays[trayNum, itemNum] == targetItem)
					{
						this.Trays[trayNum, itemNum] = null;
						return true;
					}
				}
			}
			return false;
		}

		public bool CanConsume(InventoryItem.InventoryItemClass itemClass, int amount)
		{
			int result = this.ForAllItems(delegate(InventoryItem trayItem)
			{
				if (trayItem != null && trayItem.CanConsume(itemClass, amount))
				{
					return 1;
				}
				return 0;
			});
			return result == 1;
		}

		internal void Update(GameTime gameTime)
		{
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					InventoryItem item = this.Trays[trayNum, itemNum];
					if (item != null)
					{
						item.Update(gameTime);
					}
				}
			}
		}

		internal int ForAllItems(TrayItemAction itemAction)
		{
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					InventoryItem item = this.Trays[trayNum, itemNum];
					if (item != null)
					{
						int returnValue = itemAction(item);
						if (returnValue != 0)
						{
							return returnValue;
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
					InventoryItem item = trays[i, j];
					if (item != null)
					{
						itemAction(item);
					}
				}
			}
		}

		private int GetNextTrayIndex(int currentIndex)
		{
			int index = currentIndex + 1;
			if (index >= 2)
			{
				index = 0;
			}
			return index;
		}

		internal int GetItemClassCount(InventoryItem.InventoryItemClass itemClass)
		{
			int total = 0;
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					InventoryItem item = this.Trays[trayNum, itemNum];
					if (item != null && itemClass == item.ItemClass)
					{
						total += item.StackCount;
					}
				}
			}
			return total;
		}

		internal void RemoveAllItems(bool onlyRemoveEmptyItems = false)
		{
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					if (this.Trays[trayNum, itemNum] != null && (!onlyRemoveEmptyItems || this.Trays[trayNum, itemNum].StackCount <= 0))
					{
						this.Trays[trayNum, itemNum] = null;
					}
				}
			}
		}

		internal bool CanAdd(InventoryItem item)
		{
			int result = this.ForAllItems(delegate(InventoryItem trayItem)
			{
				if (trayItem != null && trayItem.CanStack(item))
				{
					return 1;
				}
				return 0;
			});
			return result == 1;
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
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					if (this.Trays[trayNum, itemNum] == null)
					{
						this.Trays[trayNum, itemNum] = item;
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
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				writer.Write(8);
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					InventoryItem item = this.Trays[trayNum, itemNum];
					if (item == null)
					{
						writer.Write(false);
					}
					else
					{
						writer.Write(true);
						item.Write(writer);
					}
				}
			}
		}

		internal void Read(BinaryReader reader)
		{
			int maxTrayCount = reader.ReadInt32();
			for (int trayNum = 0; trayNum < maxTrayCount; trayNum++)
			{
				int maxTrayItems = reader.ReadInt32();
				for (int itemNum = 0; itemNum < maxTrayItems; itemNum++)
				{
					this.Trays[trayNum, itemNum];
					this.Trays[trayNum, itemNum] = null;
					if (reader.ReadBoolean())
					{
						InventoryItem item = InventoryItem.Create(reader);
						if (item != null && item.IsValid())
						{
							this.Trays[trayNum, itemNum] = item;
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
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					InventoryItem trayItem = this.Trays[trayNum, itemNum];
					if (trayItem != null)
					{
						InventoryItem selectedItem = sourceItem;
						trayItem.Stack(selectedItem);
						sourceItem = selectedItem;
					}
				}
			}
			return sourceItem;
		}

		internal bool PlaceInEmptySlot(InventoryItem SelectedItem)
		{
			for (int trayNum = 0; trayNum < 2; trayNum++)
			{
				for (int itemNum = 0; itemNum < 8; itemNum++)
				{
					if (this.Trays[trayNum, itemNum] == null)
					{
						this.Trays[trayNum, itemNum] = SelectedItem;
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
					InventoryItem item = trays[i, j];
					if (item == sourceItem)
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
