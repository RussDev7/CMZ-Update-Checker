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
				string directory = Path.GetDirectoryName(path);
				if (!saveDevice.DirectoryExists(directory))
				{
					saveDevice.CreateDirectory(directory);
				}
				saveDevice.Save(path, true, true, delegate(Stream stream)
				{
					BinaryWriter writer = new BinaryWriter(stream);
					this.Save(writer);
					writer.Flush();
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
				BinaryReader reader = new BinaryReader(stream);
				this.Load(reader);
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
			int fileVersionInt = reader.ReadInt32();
			PlayerInventory.PlayerInventoryVersion fileVersion = (PlayerInventory.PlayerInventoryVersion)fileVersionInt;
			if (fileVersionInt < 0 || fileVersion > PlayerInventory.PlayerInventoryVersion.CurrentVersion)
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
			if (fileVersion > PlayerInventory.PlayerInventoryVersion.MultiTray)
			{
				this.TrayManager.Read(reader);
			}
			else
			{
				this.TrayManager.ReadLegacy(reader);
			}
			if (fileVersion > PlayerInventory.PlayerInventoryVersion.TeleportStations)
			{
				int listCount = reader.ReadInt32();
				for (int j = 0; j < listCount; j++)
				{
					if (reader.ReadBoolean())
					{
						BlockInventoryItem blockInvItem = InventoryItem.Create(reader) as BlockInventoryItem;
						this.TeleportStationObjects.Add(blockInvItem);
						if (this.TeleportStationObjects[j] != null && !this.TeleportStationObjects[j].IsValid())
						{
							this.TeleportStationObjects[j] = null;
						}
						else
						{
							AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, IntVector3.FromVector3(blockInvItem.PointToLocation), blockInvItem.BlockTypeID);
						}
					}
				}
			}
			if (fileVersion > PlayerInventory.PlayerInventoryVersion.RespawnBeacon)
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
			LinkedList<Recipe> toParse = new LinkedList<Recipe>();
			Dictionary<Recipe, bool> discovered = new Dictionary<Recipe, bool>();
			foreach (Recipe r in Recipe.CookBook)
			{
				if (this.Discovered(r) && this.CanCraft(r))
				{
					this.DiscoveredRecipies.Add(r);
					toParse.AddLast(r);
					discovered[r] = true;
				}
			}
			foreach (Recipe r2 in Recipe.CookBook)
			{
				if (this.Discovered(r2) && !this.CanCraft(r2))
				{
					this.DiscoveredRecipies.Add(r2);
					toParse.AddLast(r2);
					discovered[r2] = true;
				}
			}
			for (LinkedListNode<Recipe> current = toParse.First; current != null; current = current.Next)
			{
				foreach (InventoryItem item in current.Value.Ingredients)
				{
					foreach (Recipe r3 in Recipe.CookBook)
					{
						if (r3.Result.ItemClass == item.ItemClass && !discovered.ContainsKey(r3))
						{
							discovered[r3] = true;
							toParse.AddLast(r3);
							this.DiscoveredRecipies.Add(r3);
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
				InventoryItem item = this.Inventory[i];
				bool discovered = this.DoesItemUnlockRecipe(item, recipe);
				if (discovered)
				{
					return true;
				}
			}
			int bound0 = this.TrayManager.Trays.GetUpperBound(0);
			int bound = this.TrayManager.Trays.GetUpperBound(1);
			for (int row = 0; row <= bound0; row++)
			{
				for (int col = 0; col <= bound; col++)
				{
					InventoryItem item2 = this.TrayManager.Trays[row, col];
					bool discovered = this.DoesItemUnlockRecipe(item2, recipe);
					if (discovered)
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
					GunInventoryItemClass ginv = (GunInventoryItemClass)item.ItemClass;
					if (recipe.Result.ItemClass == ginv.AmmoType)
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
			int count = 0;
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null && itemClass == this.Inventory[i].ItemClass)
				{
					count += this.Inventory[i].StackCount;
				}
			}
			return count + this.TrayManager.GetItemClassCount(itemClass);
		}

		public bool CanCraft(Recipe recipe)
		{
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Creative)
			{
				return true;
			}
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				int count = this.CountItems(recipe.Ingredients[i].ItemClass);
				if (count < recipe.Ingredients[i].StackCount)
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
				InventoryItem invitem = recipe.Result.ItemClass.CreateItem(recipe.Result.StackCount);
				this.AddInventoryItem(invitem, false);
				return;
			}
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				InventoryItem ingredient = recipe.Ingredients[i];
				int required = ingredient.StackCount;
				for (int j = 0; j < this.Inventory.Length; j++)
				{
					InventoryItem invItem = this.Inventory[j];
					this.DeductFromItemStack(ref invItem, ref required, ingredient.ItemClass);
					if (invItem == null)
					{
						this.Inventory[j] = null;
					}
				}
				int bound0 = this.TrayManager.Trays.GetUpperBound(0);
				int bound = this.TrayManager.Trays.GetUpperBound(1);
				for (int row = 0; row <= bound0; row++)
				{
					for (int col = 0; col <= bound; col++)
					{
						InventoryItem invItem = this.TrayManager.Trays[row, col];
						this.DeductFromItemStack(ref invItem, ref required, ingredient.ItemClass);
						if (invItem == null)
						{
							this.TrayManager.Trays[row, col] = null;
						}
					}
				}
			}
			InventoryItem item = recipe.Result.ItemClass.CreateItem(recipe.Result.StackCount);
			this.AddInventoryItem(item, false);
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
			InventoryItem item = null;
			int upperBound = this.TrayManager.Trays.GetUpperBound(0);
			int upperBound2 = this.TrayManager.Trays.GetUpperBound(1);
			for (int i = 0; i <= upperBound; i++)
			{
				for (int j = 0; j <= upperBound2; j++)
				{
					item = this.TrayManager.Trays[i, j];
					this.DeductFromItemStack(ref item, ref amount, itemClass);
					if (item == null)
					{
						this.TrayManager.Trays[i, j] = null;
					}
				}
			}
			for (int k = 0; k < this.Inventory.Length; k++)
			{
				item = this.Inventory[k];
				this.DeductFromItemStack(ref item, ref amount, itemClass);
				if (item == null)
				{
					this.Inventory[k] = null;
				}
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
			bool result = this.TrayManager.CanAdd(item);
			if (result)
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
			BlockInventoryItem currentlyUsedStation = this.GetTeleportAtWorldIndex(worldIndex);
			List<string> teleportNames = new List<string>();
			if (currentlyUsedStation == null)
			{
				return;
			}
			int sourceIndex = -1;
			for (int i = 0; i < this.TeleportStationObjects.Count; i++)
			{
				BlockInventoryItem tpStation = this.TeleportStationObjects[i];
				if (tpStation == currentlyUsedStation)
				{
					sourceIndex = i;
				}
				teleportNames.Add(tpStation.CustomBlockName);
			}
			this.InitSelectionScreen(currentlyUsedStation);
			this._teleportSelectionScreen.Init(sourceIndex, teleportNames);
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
				BlockInventoryItem tpStation = this.TeleportStationObjects[i];
				if (tpStation.PointToLocation == worldIndex)
				{
					return tpStation;
				}
			}
			return null;
		}

		public void DropOneSelectedTrayItem()
		{
			InventoryItem selectedTrayItem = this.TrayManager.GetItemFromCurrentTray(this.SelectedInventoryIndex);
			if (selectedTrayItem != null)
			{
				SoundManager.Instance.PlayInstance("dropitem");
				if (selectedTrayItem.StackCount == 1)
				{
					this.CreateAndPlacePickup(selectedTrayItem, this._player.LocalPosition);
					this.TrayManager.RemoveItem(selectedTrayItem);
					return;
				}
				InventoryItem item = selectedTrayItem.PopOneItem();
				this.CreateAndPlacePickup(item, this._player.LocalPosition);
			}
		}

		private void CreateAndPlacePickup(InventoryItem item, Vector3 location)
		{
			location.Y += 1f;
			PickupManager.Instance.CreatePickup(item, location, true, false);
		}

		public void DropAll(bool dropTray)
		{
			Vector3 v = this._player.LocalPosition;
			v.Y += 1f;
			if (dropTray)
			{
				int bound0 = this.TrayManager.Trays.GetUpperBound(0);
				int bound = this.TrayManager.Trays.GetUpperBound(1);
				for (int row = 0; row <= bound0; row++)
				{
					for (int col = 0; col <= bound; col++)
					{
						InventoryItem item = this.TrayManager.Trays[row, col];
						if (item != null)
						{
							PickupManager.Instance.CreatePickup(item, v, true, false);
							this.TrayManager.Trays[row, col] = null;
						}
					}
				}
			}
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null)
				{
					PickupManager.Instance.CreatePickup(this.Inventory[i], v, true, false);
					this.Inventory[i] = null;
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
				InventoryItem item = this.TrayManager.GetItemFromCurrentTray(this.SelectedInventoryIndex);
				if (item == null)
				{
					this._player.currentAnimState = Player.AnimationState.Unshouldered;
					return this._bareHands;
				}
				return item;
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
