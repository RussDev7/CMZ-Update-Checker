using System;
using System.Collections.Generic;
using System.IO;
using DNA.Audio;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class PlayerInventory
	{
		public void SaveToStorage(SaveDevice saveDevice, string path)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(path);
				if (!saveDevice.DirectoryExists(directoryName))
				{
					saveDevice.CreateDirectory(directoryName);
				}
				saveDevice.Save(path, true, true, delegate(Stream stream)
				{
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					this.Save(binaryWriter);
					binaryWriter.Flush();
				});
			}
			catch
			{
			}
		}

		public void LoadFromStorage(SaveDevice saveDevice, string path)
		{
			saveDevice.Load(path, delegate(Stream stream)
			{
				BinaryReader binaryReader = new BinaryReader(stream);
				this.Load(binaryReader);
			});
		}

		public int Version
		{
			get
			{
				return 4;
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write("PINV");
			writer.Write(this.Version);
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					this.Inventory[i].Write(writer);
				}
			}
			this.TrayManager.Write(writer);
			writer.Write(this.TeleportStationObjects.Count);
			for (int j = 0; j < this.TeleportStationObjects.Count; j++)
			{
				if (this.TeleportStationObjects[j] == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					this.TeleportStationObjects[j].Write(writer);
				}
			}
			if (this.InventorySpawnPointTeleport != null)
			{
				writer.Write(true);
				this.InventorySpawnPointTeleport.Write(writer);
				return;
			}
			writer.Write(false);
		}

		private PlayerInventory()
		{
		}

		public void Load(BinaryReader reader)
		{
			if (reader.ReadString() != "PINV")
			{
				throw new Exception("Invalid Inv File");
			}
			int num = reader.ReadInt32();
			PlayerInventory.PlayerInventoryVersion playerInventoryVersion = (PlayerInventory.PlayerInventoryVersion)num;
			if (num < 0 || playerInventoryVersion > PlayerInventory.PlayerInventoryVersion.CurrentVersion)
			{
				return;
			}
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (reader.ReadBoolean())
				{
					this.Inventory[i] = InventoryItem.Create(reader);
					if (this.Inventory[i] != null && !this.Inventory[i].IsValid())
					{
						this.Inventory[i] = null;
					}
				}
				else
				{
					this.Inventory[i] = null;
				}
			}
			if (playerInventoryVersion > PlayerInventory.PlayerInventoryVersion.MultiTray)
			{
				this.TrayManager.Read(reader);
			}
			else
			{
				this.TrayManager.ReadLegacy(reader);
			}
			if (playerInventoryVersion > PlayerInventory.PlayerInventoryVersion.TeleportStations)
			{
				int num2 = reader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					if (reader.ReadBoolean())
					{
						BlockInventoryItem blockInventoryItem = InventoryItem.Create(reader) as BlockInventoryItem;
						this.TeleportStationObjects.Add(blockInventoryItem);
						if (this.TeleportStationObjects[j] != null && !this.TeleportStationObjects[j].IsValid())
						{
							this.TeleportStationObjects[j] = null;
						}
						else
						{
							AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, IntVector3.FromVector3(blockInventoryItem.PointToLocation), blockInventoryItem.BlockTypeID);
						}
					}
				}
			}
			if (playerInventoryVersion > PlayerInventory.PlayerInventoryVersion.RespawnBeacon)
			{
				if (reader.ReadBoolean())
				{
					this.InventorySpawnPointTeleport = InventoryItem.Create(reader) as BlockInventoryItem;
					if (this.InventorySpawnPointTeleport != null && !this.InventorySpawnPointTeleport.IsValid())
					{
						this.InventorySpawnPointTeleport = null;
					}
				}
				else
				{
					this.InventorySpawnPointTeleport = null;
				}
			}
			this.DiscoverRecipies();
		}

		public void DiscoverRecipies()
		{
			this.DiscoveredRecipies.Clear();
			LinkedList<Recipe> linkedList = new LinkedList<Recipe>();
			Dictionary<Recipe, bool> dictionary = new Dictionary<Recipe, bool>();
			foreach (Recipe recipe in Recipe.CookBook)
			{
				if (this.Discovered(recipe) && this.CanCraft(recipe))
				{
					this.DiscoveredRecipies.Add(recipe);
					linkedList.AddLast(recipe);
					dictionary[recipe] = true;
				}
			}
			foreach (Recipe recipe2 in Recipe.CookBook)
			{
				if (this.Discovered(recipe2) && !this.CanCraft(recipe2))
				{
					this.DiscoveredRecipies.Add(recipe2);
					linkedList.AddLast(recipe2);
					dictionary[recipe2] = true;
				}
			}
			for (LinkedListNode<Recipe> linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				foreach (InventoryItem inventoryItem in linkedListNode.Value.Ingredients)
				{
					foreach (Recipe recipe3 in Recipe.CookBook)
					{
						if (recipe3.Result.ItemClass == inventoryItem.ItemClass && !dictionary.ContainsKey(recipe3))
						{
							dictionary[recipe3] = true;
							linkedList.AddLast(recipe3);
							this.DiscoveredRecipies.Add(recipe3);
						}
					}
				}
			}
			if (this.DiscoveredRecipies.Count == 0)
			{
				this.DiscoveredRecipies.Add(Recipe.CookBook[0]);
			}
		}

		public bool Discovered(Recipe recipe)
		{
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				InventoryItem inventoryItem = this.Inventory[i];
				bool flag = this.DoesItemUnlockRecipe(inventoryItem, recipe);
				if (flag)
				{
					return true;
				}
			}
			int upperBound = this.TrayManager.Trays.GetUpperBound(0);
			int upperBound2 = this.TrayManager.Trays.GetUpperBound(1);
			for (int j = 0; j <= upperBound; j++)
			{
				for (int k = 0; k <= upperBound2; k++)
				{
					InventoryItem inventoryItem2 = this.TrayManager.Trays[j, k];
					bool flag = this.DoesItemUnlockRecipe(inventoryItem2, recipe);
					if (flag)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool DoesItemUnlockRecipe(InventoryItem item, Recipe recipe)
		{
			if (item != null)
			{
				if (recipe.Result.ItemClass == item.ItemClass)
				{
					return true;
				}
				if (item.ItemClass is GunInventoryItemClass)
				{
					GunInventoryItemClass gunInventoryItemClass = (GunInventoryItemClass)item.ItemClass;
					if (recipe.Result.ItemClass == gunInventoryItemClass.AmmoType)
					{
						return true;
					}
				}
				for (int i = 0; i < recipe.Ingredients.Count; i++)
				{
					if (recipe.Ingredients[i].ItemClass == item.ItemClass)
					{
						return true;
					}
				}
			}
			return false;
		}

		public int CountItems(InventoryItem.InventoryItemClass itemClass)
		{
			int num = 0;
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null && itemClass == this.Inventory[i].ItemClass)
				{
					num += this.Inventory[i].StackCount;
				}
			}
			return num + this.TrayManager.GetItemClassCount(itemClass);
		}

		public bool CanCraft(Recipe recipe)
		{
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Creative)
			{
				return true;
			}
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				int num = this.CountItems(recipe.Ingredients[i].ItemClass);
				if (num < recipe.Ingredients[i].StackCount)
				{
					return false;
				}
			}
			return true;
		}

		public void Craft(Recipe recipe)
		{
			if (CastleMinerZGame.Instance.InfiniteResourceMode)
			{
				InventoryItem inventoryItem = recipe.Result.ItemClass.CreateItem(recipe.Result.StackCount);
				this.AddInventoryItem(inventoryItem, false);
				return;
			}
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				InventoryItem inventoryItem2 = recipe.Ingredients[i];
				int stackCount = inventoryItem2.StackCount;
				for (int j = 0; j < this.Inventory.Length; j++)
				{
					InventoryItem inventoryItem3 = this.Inventory[j];
					this.DeductFromItemStack(ref inventoryItem3, ref stackCount, inventoryItem2.ItemClass);
					if (inventoryItem3 == null)
					{
						this.Inventory[j] = null;
					}
				}
				int upperBound = this.TrayManager.Trays.GetUpperBound(0);
				int upperBound2 = this.TrayManager.Trays.GetUpperBound(1);
				for (int k = 0; k <= upperBound; k++)
				{
					for (int l = 0; l <= upperBound2; l++)
					{
						InventoryItem inventoryItem3 = this.TrayManager.Trays[k, l];
						this.DeductFromItemStack(ref inventoryItem3, ref stackCount, inventoryItem2.ItemClass);
						if (inventoryItem3 == null)
						{
							this.TrayManager.Trays[k, l] = null;
						}
					}
				}
			}
			InventoryItem inventoryItem4 = recipe.Result.ItemClass.CreateItem(recipe.Result.StackCount);
			this.AddInventoryItem(inventoryItem4, false);
		}

		private void DeductFromItemStack(ref InventoryItem item, ref int required, InventoryItem.InventoryItemClass itemClass)
		{
			if (item != null && item.ItemClass == itemClass)
			{
				if (required < item.StackCount)
				{
					item.StackCount -= required;
					required = 0;
					return;
				}
				required -= item.StackCount;
				item = null;
			}
		}

		public void Remove(InventoryItem item)
		{
			if (this.TrayManager.RemoveItem(item))
			{
				return;
			}
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] == item)
				{
					this.Inventory[i] = null;
				}
			}
		}

		public bool CanConsume(InventoryItem.InventoryItemClass itemType, int amount)
		{
			if (itemType == null)
			{
				return true;
			}
			if (this.TrayManager.CanConsume(itemType, amount))
			{
				return true;
			}
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null && this.Inventory[i].CanConsume(itemType, amount))
				{
					return true;
				}
			}
			return false;
		}

		public bool Consume(InventoryItem item, int amount, bool ignoreInfiniteResources = false)
		{
			if (CastleMinerZGame.Instance.InfiniteResourceMode && !ignoreInfiniteResources)
			{
				return true;
			}
			if (item.StackCount >= amount)
			{
				item.StackCount -= amount;
				this.RemoveEmptyItems();
				return true;
			}
			return false;
		}

		public bool Consume(InventoryItem.InventoryItemClass itemClass, int amount)
		{
			if (CastleMinerZGame.Instance.InfiniteResourceMode)
			{
				return true;
			}
			if (itemClass == null)
			{
				return true;
			}
			InventoryItem inventoryItem = null;
			int num = 0;
			while (num < 2147483647 && amount > 0)
			{
				num = int.MaxValue;
				int upperBound = this.TrayManager.Trays.GetUpperBound(0);
				int upperBound2 = this.TrayManager.Trays.GetUpperBound(1);
				bool flag = false;
				for (int i = 0; i <= upperBound; i++)
				{
					for (int j = 0; j <= upperBound2; j++)
					{
						inventoryItem = this.TrayManager.Trays[i, j];
						if (inventoryItem != null && inventoryItem.ItemClass == itemClass && inventoryItem.StackCount < num)
						{
							num = inventoryItem.StackCount;
							flag = true;
							break;
						}
						inventoryItem = null;
					}
					if (flag)
					{
						break;
					}
				}
				this.DeductFromItemStack(ref inventoryItem, ref amount, itemClass);
			}
			num = 0;
			while (num < 2147483647 && amount > 0)
			{
				num = int.MaxValue;
				for (int k = 0; k < this.Inventory.Length; k++)
				{
					inventoryItem = this.Inventory[k];
					if (inventoryItem != null && inventoryItem.ItemClass == itemClass && inventoryItem.StackCount < num)
					{
						num = inventoryItem.StackCount;
						break;
					}
					inventoryItem = null;
				}
				this.DeductFromItemStack(ref inventoryItem, ref amount, itemClass);
			}
			return amount <= 0;
		}

		public void RemoveEmptyItems()
		{
			this.TrayManager.RemoveAllItems(true);
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null && this.Inventory[i].StackCount <= 0)
				{
					this.Inventory[i] = null;
				}
			}
		}

		public bool CanAdd(InventoryItem item)
		{
			bool flag = this.TrayManager.CanAdd(item);
			if (flag)
			{
				return true;
			}
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] == null || this.Inventory[i].CanStack(item))
				{
					return true;
				}
			}
			return false;
		}

		public int AddItemToTray(InventoryItem item)
		{
			return this.TrayManager.AddItemToTray(item);
		}

		public int AddItemToInventory(InventoryItem item)
		{
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null)
				{
					this.Inventory[i].Stack(item);
				}
			}
			if (item.StackCount <= 0)
			{
				return 0;
			}
			for (int j = 0; j < this.Inventory.Length; j++)
			{
				if (this.Inventory[j] == null)
				{
					this.Inventory[j] = item;
					return 0;
				}
			}
			return item.StackCount;
		}

		public void AddInventoryItem(InventoryItem item, bool displayOnPickup = false)
		{
			if (displayOnPickup)
			{
				if (item.StackCount > 1)
				{
					Console.WriteLine(string.Concat(new object[] { "You looted: ", item.Name, " (", item.StackCount, ")" }));
				}
				else
				{
					Console.WriteLine("You looted: " + item.Name);
				}
			}
			this.TrayManager.AddItemToExisting(item);
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null)
				{
					this.Inventory[i].Stack(item);
				}
			}
			if (item.StackCount <= 0)
			{
				this.DiscoverRecipies();
				return;
			}
			if (this.TrayManager.AddItemToEmpty(item))
			{
				this.DiscoverRecipies();
				return;
			}
			for (int j = 0; j < this.Inventory.Length; j++)
			{
				if (this.Inventory[j] == null)
				{
					this.Inventory[j] = item;
					this.DiscoverRecipies();
					return;
				}
			}
			if (item.StackCount > 0)
			{
				this.CreateAndPlacePickup(item, this._player.LocalPosition);
			}
		}

		protected void InitSelectionScreen(BlockInventoryItem teleportStation)
		{
			this._teleportSelectionScreen = new PCListSelectScreen(CastleMinerZGame.Instance, "Teleport Station: " + teleportStation.CustomBlockName, "Select a destination teleport:", CastleMinerZGame.Instance.DialogScreenImage, CastleMinerZGame.Instance._myriadMed, true, CastleMinerZGame.Instance.ButtonFrame);
			this._teleportSelectionScreen.ClickSound = "Click";
			this._teleportSelectionScreen.OpenSound = "Popup";
		}

		public void ShowTeleportStationMenu(Vector3 worldIndex)
		{
			BlockInventoryItem teleportAtWorldIndex = this.GetTeleportAtWorldIndex(worldIndex);
			List<string> list = new List<string>();
			if (teleportAtWorldIndex == null)
			{
				return;
			}
			int num = -1;
			for (int i = 0; i < this.TeleportStationObjects.Count; i++)
			{
				BlockInventoryItem blockInventoryItem = this.TeleportStationObjects[i];
				if (blockInventoryItem == teleportAtWorldIndex)
				{
					num = i;
				}
				list.Add(blockInventoryItem.CustomBlockName);
			}
			this.InitSelectionScreen(teleportAtWorldIndex);
			this._teleportSelectionScreen.Init(num, list);
			CastleMinerZGame.Instance.GameScreen._uiGroup.ShowPCDialogScreen(this._teleportSelectionScreen, delegate
			{
				if (this._teleportSelectionScreen.OptionSelected != -1)
				{
					SoundManager.Instance.PlayInstance("Teleport");
					CastleMinerZGame.Instance.GameScreen.TeleportToLocation(this.TeleportStationObjects[this._teleportSelectionScreen.OptionSelected].PointToLocation, false);
				}
			});
		}

		public BlockInventoryItem GetTeleportAtWorldIndex(Vector3 worldIndex)
		{
			for (int i = 0; i < this.TeleportStationObjects.Count; i++)
			{
				BlockInventoryItem blockInventoryItem = this.TeleportStationObjects[i];
				if (blockInventoryItem.PointToLocation == worldIndex)
				{
					return blockInventoryItem;
				}
			}
			return null;
		}

		public void DropOneSelectedTrayItem()
		{
			InventoryItem itemFromCurrentTray = this.TrayManager.GetItemFromCurrentTray(this.SelectedInventoryIndex);
			if (itemFromCurrentTray != null)
			{
				SoundManager.Instance.PlayInstance("dropitem");
				if (itemFromCurrentTray.StackCount == 1)
				{
					this.CreateAndPlacePickup(itemFromCurrentTray, this._player.LocalPosition);
					this.TrayManager.RemoveItem(itemFromCurrentTray);
					return;
				}
				InventoryItem inventoryItem = itemFromCurrentTray.PopOneItem();
				this.CreateAndPlacePickup(inventoryItem, this._player.LocalPosition);
			}
		}

		private void CreateAndPlacePickup(InventoryItem item, Vector3 location)
		{
			location.Y += 1f;
			PickupManager.Instance.CreatePickup(item, location, true, false);
		}

		public void DropAll(bool dropTray)
		{
			Vector3 localPosition = this._player.LocalPosition;
			localPosition.Y += 1f;
			if (dropTray)
			{
				int upperBound = this.TrayManager.Trays.GetUpperBound(0);
				int upperBound2 = this.TrayManager.Trays.GetUpperBound(1);
				for (int i = 0; i <= upperBound; i++)
				{
					for (int j = 0; j <= upperBound2; j++)
					{
						InventoryItem inventoryItem = this.TrayManager.Trays[i, j];
						if (inventoryItem != null)
						{
							PickupManager.Instance.CreatePickup(inventoryItem, localPosition, true, false);
							this.TrayManager.Trays[i, j] = null;
						}
					}
				}
			}
			for (int k = 0; k < this.Inventory.Length; k++)
			{
				if (this.Inventory[k] != null)
				{
					PickupManager.Instance.CreatePickup(this.Inventory[k], localPosition, true, false);
					this.Inventory[k] = null;
				}
			}
			this.DiscoverRecipies();
		}

		public void DropItem(InventoryItem item)
		{
			if (this.TrayManager.RemoveItem(item))
			{
				this.CreateAndPlacePickup(item, this._player.LocalPosition);
				SoundManager.Instance.PlayInstance("dropitem");
				this.DiscoverRecipies();
				return;
			}
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] == item)
				{
					this.Inventory[i] = null;
					this.CreateAndPlacePickup(item, this._player.LocalPosition);
					SoundManager.Instance.PlayInstance("dropitem");
					this.DiscoverRecipies();
					return;
				}
			}
		}

		public GameScreen GameScreen
		{
			get
			{
				return CastleMinerZGame.Instance.GameScreen;
			}
		}

		public Player Player
		{
			set
			{
				this._player = value;
			}
		}

		public PlayerInventory(Player player, bool setDefault)
		{
			this._player = player;
			if (setDefault)
			{
				this.SetDefaultInventory();
			}
		}

		public void SetDefaultInventory()
		{
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				this.Inventory[i] = null;
			}
			this.TrayManager.RemoveAllItems(false);
			if (CastleMinerZGame.Instance.Difficulty != GameDifficultyTypes.HARDCORE)
			{
				this.AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.StonePickAxe, 1), false);
				this.AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Compass, 1), false);
				this.AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Pistol, 1), false);
				this.AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Knife, 1), false);
				this.AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Bullets, 200), false);
				this.AddInventoryItem(InventoryItem.CreateItem(InventoryItemIDs.Torch, 16), false);
			}
			this.DiscoverRecipies();
		}

		public void Update(GameTime gameTime)
		{
			this._bareHands.Update(gameTime);
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null)
				{
					this.Inventory[i].Update(gameTime);
				}
			}
			this.TrayManager.Update(gameTime);
		}

		public InventoryItem ActiveInventoryItem
		{
			get
			{
				InventoryItem itemFromCurrentTray = this.TrayManager.GetItemFromCurrentTray(this.SelectedInventoryIndex);
				if (itemFromCurrentTray == null)
				{
					return this._bareHands;
				}
				return itemFromCurrentTray;
			}
		}

		internal void SwitchCurrentTray()
		{
			this.TrayManager.SwitchCurrentTray();
		}

		public const int MaxTrayItems = 8;

		public const string Ident = "PINV";

		public InventoryItem[] Inventory = new InventoryItem[32];

		public InventoryTrayManager TrayManager = new InventoryTrayManager();

		public List<Recipe> DiscoveredRecipies = new List<Recipe>();

		public BlockInventoryItem InventorySpawnPointTeleport;

		public List<BlockInventoryItem> TeleportStationObjects = new List<BlockInventoryItem>();

		public int SelectedInventoryIndex;

		private PCListSelectScreen _teleportSelectionScreen;

		private Player _player;

		private InventoryItem _bareHands = InventoryItem.CreateItem(InventoryItemIDs.BareHands, 1);

		private enum PlayerInventoryVersion
		{
			RespawnBeacon = 1,
			TeleportStations,
			MultiTray,
			CurrentVersion
		}
	}
}
