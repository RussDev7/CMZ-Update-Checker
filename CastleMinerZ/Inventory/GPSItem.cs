using System;
using System.IO;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.IO;
using DNA.Text;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class GPSItem : InventoryItem
	{
		public Vector3 PointToLocation
		{
			get
			{
				return this._pointToLocation;
			}
		}

		public Color color
		{
			get
			{
				switch (this.GPSClass.ID)
				{
				case InventoryItemIDs.GPS:
					return CMZColors.GetMaterialcColor(ToolMaterialTypes.Gold);
				case InventoryItemIDs.TeleportGPS:
					return CMZColors.GetMaterialcColor(ToolMaterialTypes.BloodStone);
				default:
					return CMZColors.GetMaterialcColor(ToolMaterialTypes.Diamond);
				}
			}
		}

		public override void GetDisplayText(StringBuilder builder)
		{
			base.GetDisplayText(builder);
			Vector3 playerPosition = CastleMinerZGame.Instance.LocalPlayer.LocalPosition;
			builder.Append(": ");
			builder.Append(this._GPSname);
			builder.Append(" - ");
			builder.Append(Strings.Distance);
			builder.Append(": ");
			builder.Concat((int)Vector3.Distance(playerPosition, this.PointToLocation));
		}

		public GPSItemClass GPSClass
		{
			get
			{
				return (GPSItemClass)base.ItemClass;
			}
		}

		public GPSItem(GPSItemClass classType, int stackCount)
			: base(classType, stackCount)
		{
			this._keyboardInputScreen = new PCKeyboardInputScreen(CastleMinerZGame.Instance, Strings.Name, Strings.Enter_A_Name_For_This_Locator, CastleMinerZGame.Instance.DialogScreenImage, CastleMinerZGame.Instance._myriadMed, true, CastleMinerZGame.Instance.ButtonFrame);
			this._keyboardInputScreen.ClickSound = "Click";
			this._keyboardInputScreen.OpenSound = "Popup";
		}

		protected override void Read(BinaryReader reader)
		{
			base.Read(reader);
			this._pointToLocation = reader.ReadVector3();
			this._GPSname = reader.ReadString();
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(this._pointToLocation);
			writer.Write(this._GPSname);
		}

		private void ShowKeyboard()
		{
			this._keyboardInputScreen.DefaultText = this._GPSname;
			CastleMinerZGame.Instance.GameScreen._uiGroup.ShowPCDialogScreen(this._keyboardInputScreen, delegate
			{
				if (this._keyboardInputScreen.OptionSelected != -1)
				{
					string nameText = this._keyboardInputScreen.TextInput;
					if (nameText != null)
					{
						if (nameText.Length > 10)
						{
							this._GPSname = nameText.Substring(0, 10);
							return;
						}
						this._GPSname = nameText;
					}
				}
			});
		}

		public void PlaceLocator(InGameHUD hud)
		{
			SoundManager.Instance.PlayInstance("locator");
			this._pointToLocation = hud.ConstructionProbe._worldIndex + Vector3.Zero;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (controller.Reload.Pressed)
			{
				this.ShowKeyboard();
				return;
			}
			InventoryItemIDs id = this.GPSClass.ID;
			InventoryItemIDs inventoryItemIDs = id;
			switch (inventoryItemIDs)
			{
			case InventoryItemIDs.GPS:
				if (controller.Use.Pressed)
				{
					this.PlaceLocator(hud);
					if (this.InflictDamage())
					{
						hud.PlayerInventory.Remove(this);
						return;
					}
					this.ShowKeyboard();
					return;
				}
				break;
			case InventoryItemIDs.TeleportGPS:
				if (controller.Use.Pressed)
				{
					this.PlaceLocator(hud);
					this.ShowKeyboard();
					return;
				}
				if (controller.Shoulder.Pressed)
				{
					if (this._pointToLocation == Vector3.Zero)
					{
						SoundManager.Instance.PlayInstance("Error");
						return;
					}
					SoundManager.Instance.PlayInstance("Teleport");
					string msg = string.Concat(new string[]
					{
						CastleMinerZGame.Instance.MyNetworkGamer.Gamertag,
						" ",
						Strings.Teleported_To,
						" ",
						this._GPSname
					});
					BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, msg);
					CastleMinerZGame.Instance.GameScreen.TeleportToLocation(this._pointToLocation, false);
					if (this.InflictDamage())
					{
						hud.PlayerInventory.Remove(this);
						return;
					}
				}
				break;
			default:
				if (inventoryItemIDs != InventoryItemIDs.SpawnBasic)
				{
					return;
				}
				if (controller.Use.Pressed)
				{
					this.PlaceLocator(hud);
					hud.PlayerInventory.Remove(this);
					return;
				}
				if (controller.Shoulder.Pressed)
				{
					if (this._pointToLocation == Vector3.Zero)
					{
						SoundManager.Instance.PlayInstance("Error");
						return;
					}
					SoundManager.Instance.PlayInstance("Teleport");
					string msg2 = string.Concat(new string[]
					{
						CastleMinerZGame.Instance.MyNetworkGamer.Gamertag,
						" ",
						Strings.Teleported_To,
						" ",
						this._GPSname
					});
					BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, msg2);
					CastleMinerZGame.Instance.GameScreen.TeleportToLocation(this._pointToLocation, false);
				}
				break;
			}
		}

		private Vector3 _pointToLocation = Vector3.Zero;

		private string _GPSname = "Alpha";

		private new PCKeyboardInputScreen _keyboardInputScreen;
	}
}
