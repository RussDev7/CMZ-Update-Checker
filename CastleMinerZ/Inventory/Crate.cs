using System;
using System.IO;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Inventory
{
	public class Crate
	{
		public IntVector3 Location
		{
			get
			{
				return this._location;
			}
		}

		public Crate(IntVector3 location)
		{
			this._location = location;
		}

		public void EjectContents()
		{
			for (int i = 0; i < this.Inventory.Length; i++)
			{
				if (this.Inventory[i] != null && !this.IsSlotLocked(i))
				{
					Vector3 vector = IntVector3.ToVector3(this.Location) + new Vector3(0.5f);
					PickupManager.Instance.CreateUpwardPickup(this.Inventory[i], vector, 3f, false);
					this.Inventory[i] = null;
				}
			}
		}

		public bool IsSlotLocked(int index)
		{
			foreach (NetworkGamer networkGamer in CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers)
			{
				if (networkGamer.Tag != null)
				{
					Player player = (Player)networkGamer.Tag;
					int num = player.FocusCrateItem.X + player.FocusCrateItem.Y * 8;
					if (player.FocusCrate == this.Location && num == index)
					{
						return true;
					}
				}
			}
			return false;
		}

		public Crate(BinaryReader reader)
		{
			this.Read(reader);
		}

		public void Write(BinaryWriter writer)
		{
			this._location.Write(writer);
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
		}

		public void Read(BinaryReader reader)
		{
			this._location = IntVector3.Read(reader);
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
		}

		private const int Columns = 8;

		public bool Destroyed;

		private IntVector3 _location;

		public InventoryItem[] Inventory = new InventoryItem[32];
	}
}
