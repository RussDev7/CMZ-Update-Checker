using System;
using System.IO;
using DNA.Audio;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Input;
using DNA.IO;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class BlockInventoryItem : InventoryItem
	{
		public Vector3 PointToLocation
		{
			get
			{
				return this._pointToLocation;
			}
		}

		public string CustomBlockName
		{
			get
			{
				return this._customBlockName;
			}
		}

		public BlockInventoryItem(BlockInventoryItemClass classtype, int stackCount)
			: base(classtype, stackCount)
		{
		}

		public BlockTypeEnum BlockTypeID
		{
			get
			{
				return ((BlockInventoryItemClass)base.ItemClass).BlockType._type;
			}
		}

		public virtual BlockTypeEnum GetConstructedBlockType(BlockFace face, IntVector3 position)
		{
			return this.BlockTypeID;
		}

		public virtual void AlterBlock(Player player, IntVector3 addSpot, BlockFace inFace)
		{
			AlterBlockMessage.Send((LocalNetworkGamer)player.Gamer, addSpot, this.GetConstructedBlockType(inFace, addSpot));
		}

		public virtual bool CanPlaceHere(IntVector3 addSpot, BlockFace inFace)
		{
			return true;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			Trigger use = controller.Use;
			if (base.CoolDownTimer.Expired && controller.Use.Pressed && base.StackCount > 0)
			{
				base.CoolDownTimer.Reset();
				IntVector3 location = hud.Build(this, true);
				if (location != IntVector3.Zero)
				{
					bool buildNow = true;
					if (BlockType.IsDoor(this.BlockTypeID))
					{
						CastleMinerZGame.Instance.CurrentWorld.SetDoor(location, DoorEntity.GetModelNameFromInventoryId(base.ItemClass.ID));
					}
					if (this.BlockTypeID == BlockTypeEnum.SpawnPointBasic)
					{
						this.PlaceLocator(location);
						CastleMinerZGame.Instance.LocalPlayer.SetSpawnPoint(this);
						hud.PlayerInventory.Consume(this, 1, false);
					}
					else if (this.BlockTypeID == BlockTypeEnum.TeleportStation)
					{
						this._hudReference = hud;
						this.ShowKeyboard(null);
						buildNow = false;
					}
					else
					{
						hud.PlayerInventory.Consume(this, 1, false);
					}
					if (buildNow)
					{
						hud.Build(this, false);
					}
				}
				hud.LocalPlayer.UsingTool = true;
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(base.ItemClass.ID);
				itemStats.Used++;
				return;
			}
			hud.LocalPlayer.UsingTool = false;
		}

		public void PlaceLocator(InGameHUD hud)
		{
			this.PlaceLocator(hud.ConstructionProbe._worldIndex + Vector3.Zero);
		}

		public void PlaceLocator(Vector3 location)
		{
			SoundManager.Instance.PlayInstance("locator");
			this._pointToLocation = location;
		}

		protected override void Read(BinaryReader reader)
		{
			base.Read(reader);
			if (this.BlockTypeID == BlockTypeEnum.SpawnPointBasic || this.BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				this._pointToLocation = reader.ReadVector3();
			}
			if (this.BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				this._customBlockName = reader.ReadString();
			}
			BlockTypeEnum baseBlock = SpawnBlockView.GetInActiveSpawnBlockType(this.BlockTypeID);
			BlockTypeEnum blockTypeID = this.BlockTypeID;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			if (this.BlockTypeID == BlockTypeEnum.SpawnPointBasic || this.BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				writer.Write(this._pointToLocation);
			}
			if (this.BlockTypeID == BlockTypeEnum.TeleportStation)
			{
				writer.Write(string.IsNullOrEmpty(this._customBlockName) ? "Delta" : this._customBlockName);
			}
		}

		protected override void OnKeyboardSubmit()
		{
			string nameText = this._keyboardInputScreen.TextInput;
			if (nameText != null)
			{
				if (nameText.Length > 10)
				{
					this._customBlockName = nameText.Substring(0, 10);
				}
				else
				{
					this._customBlockName = nameText;
				}
			}
			IntVector3 location = this._hudReference.Build(this, false);
			this.PlaceLocator(location);
			CastleMinerZGame.Instance.LocalPlayer.AddTeleportStationObject(this);
			CastleMinerZGame.Instance.LocalPlayer.PlayerInventory.Consume(this, 1, true);
			this._hudReference = null;
		}

		protected override void OnKeyboardCancel()
		{
			this._hudReference = null;
		}

		private Vector3 _pointToLocation = Vector3.Zero;

		private string _customBlockName;

		private InGameHUD _hudReference;
	}
}
