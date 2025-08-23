using System;
using System.IO;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Inventory
{
	public class Door
	{
		public IntVector3 Location
		{
			get
			{
				return this._location;
			}
		}

		public DoorEntity.ModelNameEnum ModelName
		{
			get
			{
				return this._modelName;
			}
		}

		public Door(IntVector3 location, DoorEntity.ModelNameEnum modelName)
		{
			this._location = location;
			this._modelName = modelName;
		}

		public bool IsSlotLocked(int index)
		{
			foreach (NetworkGamer gamer in CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers)
			{
				if (gamer.Tag != null)
				{
					Player player = (Player)gamer.Tag;
					int playerLockedIndex = player.FocusCrateItem.X + player.FocusCrateItem.Y * 8;
					if (player.FocusCrate == this.Location && playerLockedIndex == index)
					{
						return true;
					}
				}
			}
			return false;
		}

		public Door(BinaryReader reader)
		{
			this.Read(reader);
		}

		public void Write(BinaryWriter writer)
		{
			this._location.Write(writer);
			writer.Write((byte)this._modelName);
		}

		public void Read(BinaryReader reader)
		{
			this._location = IntVector3.Read(reader);
			this._modelName = (DoorEntity.ModelNameEnum)reader.ReadByte();
		}

		private const int Columns = 8;

		public bool Destroyed;

		public bool Open;

		private DoorEntity.ModelNameEnum _modelName;

		private IntVector3 _location;
	}
}
