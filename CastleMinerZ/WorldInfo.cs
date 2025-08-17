using System;
using System.Collections.Generic;
using System.IO;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Terrain.WorldBuilders;
using DNA.Drawing.UI;
using DNA.IO;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class WorldInfo
	{
		public int Version
		{
			get
			{
				return 5;
			}
		}

		private WorldInfo()
		{
			this.ServerMessage = Screen.CurrentGamer + "'s " + Strings.Server;
			this.CreateSavePath();
		}

		public WorldInfo(BinaryReader reader)
		{
			this.CreateSavePath();
			this.Load(reader);
		}

		public WorldInfo(WorldInfo info)
		{
			this._savePath = info._savePath;
			this._terrainVersion = info._terrainVersion;
			this._name = info._name;
			this._ownerGamerTag = info._ownerGamerTag;
			this._creatorGamerTag = info._creatorGamerTag;
			this._createdDate = info._createdDate;
			this._lastPlayedDate = info._lastPlayedDate;
			this._seed = info._seed;
			this._worldID = info._worldID;
			this._lastPosition = info._lastPosition;
			this.InfiniteResourceMode = info.InfiniteResourceMode;
		}

		public void SetDoor(IntVector3 location, DoorEntity.ModelNameEnum modelName)
		{
			Door door;
			if (!this.Doors.TryGetValue(location, out door))
			{
				door = new Door(location, modelName);
				this.Doors[location] = door;
			}
		}

		public Spawner GetSpawner(IntVector3 location, bool createIfMissing, BlockTypeEnum blockType)
		{
			Spawner spawner;
			if (!this.Spawners.TryGetValue(location, out spawner))
			{
				if (!createIfMissing)
				{
					return null;
				}
				spawner = new Spawner(location);
				this.Spawners[location] = spawner;
			}
			return spawner;
		}

		public Door GetDoor(IntVector3 location)
		{
			Door door;
			if (this.Doors.TryGetValue(location, out door))
			{
				return door;
			}
			return null;
		}

		public Crate GetCrate(IntVector3 crateLocation, bool createIfMissing)
		{
			Crate crate;
			if (!this.Crates.TryGetValue(crateLocation, out crate))
			{
				if (!createIfMissing)
				{
					return null;
				}
				crate = new Crate(crateLocation);
				this.Crates[crateLocation] = crate;
			}
			return crate;
		}

		public static WorldInfo CreateNewWorld(SignedInGamer gamer)
		{
			Random random = new Random();
			WorldInfo worldInfo = new WorldInfo();
			int num = 839880689;
			if (!CastleMinerZGame.TrialMode)
			{
				num = random.Next();
			}
			worldInfo.MakeNew(gamer, num);
			return worldInfo;
		}

		public static WorldInfo CreateNewWorld(SignedInGamer gamer, int seed)
		{
			WorldInfo worldInfo = new WorldInfo();
			worldInfo.MakeNew(gamer, seed);
			return worldInfo;
		}

		public static WorldInfo CreateNewWorld(int seed)
		{
			return WorldInfo.CreateNewWorld(null, seed);
		}

		public static WorldInfo[] LoadWorldInfo(SaveDevice device)
		{
			WorldInfo[] array;
			try
			{
				WorldInfo.CorruptWorlds.Clear();
				if (!device.DirectoryExists(WorldInfo.BasePath))
				{
					array = new WorldInfo[0];
				}
				else
				{
					List<WorldInfo> list = new List<WorldInfo>();
					string[] directories = device.GetDirectories(WorldInfo.BasePath);
					foreach (string text in directories)
					{
						WorldInfo worldInfo = null;
						try
						{
							worldInfo = WorldInfo.LoadFromStroage(text, device);
						}
						catch
						{
							worldInfo = null;
							WorldInfo.CorruptWorlds.Add(text);
						}
						if (worldInfo != null)
						{
							list.Add(worldInfo);
						}
					}
					array = list.ToArray();
				}
			}
			catch
			{
				array = new WorldInfo[0];
			}
			return array;
		}

		public void CreateSavePath()
		{
			Guid guid = Guid.NewGuid();
			this._savePath = Path.Combine(WorldInfo.BasePath, guid.ToString());
		}

		private void MakeNew(SignedInGamer creator, int seed)
		{
			if (creator == null)
			{
				this._name = Strings.New_World + " " + DateTime.Now.ToString("g");
			}
			else
			{
				this._name = string.Concat(new object[]
				{
					creator,
					"'s ",
					Strings.World,
					" ",
					DateTime.Now.ToString("g")
				});
			}
			this.CreateSavePath();
			if (creator == null)
			{
				this._ownerGamerTag = (this._creatorGamerTag = null);
			}
			else
			{
				this._ownerGamerTag = (this._creatorGamerTag = creator.Gamertag);
			}
			this._createdDate = (this._lastPlayedDate = DateTime.Now);
			this._worldID = Guid.NewGuid();
			this._seed = seed;
		}

		public void TakeOwnership(SignedInGamer gamer, SaveDevice device)
		{
			if (this._creatorGamerTag == null)
			{
				this._creatorGamerTag = gamer.Gamertag;
			}
			this._ownerGamerTag = gamer.Gamertag;
			this._worldID = Guid.NewGuid();
			this.SaveToStorage(gamer, device);
		}

		public string SavePath
		{
			get
			{
				if (this.OwnerGamerTag == null)
				{
					return null;
				}
				return this._savePath;
			}
			set
			{
				this._savePath = value;
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		public string OwnerGamerTag
		{
			get
			{
				return this._ownerGamerTag;
			}
		}

		public string CreatorGamerTag
		{
			get
			{
				return this._creatorGamerTag;
			}
		}

		public DateTime CreatedDate
		{
			get
			{
				return this._createdDate;
			}
		}

		public DateTime LastPlayedDate
		{
			get
			{
				return this._lastPlayedDate;
			}
			set
			{
				this._lastPlayedDate = value;
			}
		}

		public int Seed
		{
			get
			{
				return this._seed;
			}
		}

		public Guid WorldID
		{
			get
			{
				return this._worldID;
			}
		}

		public Vector3 LastPosition
		{
			get
			{
				return this._lastPosition;
			}
			set
			{
				this._lastPosition = value;
			}
		}

		public void SaveToStorage(SignedInGamer gamer, SaveDevice saveDevice)
		{
			try
			{
				if (!saveDevice.DirectoryExists(this.SavePath))
				{
					saveDevice.CreateDirectory(this.SavePath);
				}
				string text = Path.Combine(this.SavePath, WorldInfo.FileName);
				saveDevice.Save(text, true, true, delegate(Stream stream)
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

		private static WorldInfo LoadFromStroage(string folder, SaveDevice saveDevice)
		{
			WorldInfo info = new WorldInfo();
			saveDevice.Load(Path.Combine(folder, WorldInfo.FileName), delegate(Stream stream)
			{
				BinaryReader binaryReader = new BinaryReader(stream);
				info.Load(binaryReader);
				info._savePath = folder;
			});
			return info;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(this.Version);
			writer.Write((int)this._terrainVersion);
			writer.Write(this._name);
			writer.Write(this._ownerGamerTag);
			writer.Write(this._creatorGamerTag);
			writer.Write(this._createdDate.Ticks);
			writer.Write(this._lastPlayedDate.Ticks);
			writer.Write(this._seed);
			writer.Write(this._worldID.ToByteArray());
			writer.Write(this._lastPosition);
			writer.Write(this.Crates.Count);
			foreach (KeyValuePair<IntVector3, Crate> keyValuePair in this.Crates)
			{
				keyValuePair.Value.Write(writer);
			}
			writer.Write(this.Doors.Count);
			foreach (KeyValuePair<IntVector3, Door> keyValuePair2 in this.Doors)
			{
				keyValuePair2.Value.Write(writer);
			}
			writer.Write(this.Spawners.Count);
			foreach (KeyValuePair<IntVector3, Spawner> keyValuePair3 in this.Spawners)
			{
				keyValuePair3.Value.Write(writer);
			}
			writer.Write(this.InfiniteResourceMode);
			writer.Write(this.ServerMessage);
			writer.Write(this.ServerPassword);
			writer.Write(this.HellBossesSpawned);
			writer.Write(this.MaxHellBossSpawns);
		}

		private void Load(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			WorldInfo.WorldInfoVersion worldInfoVersion = (WorldInfo.WorldInfoVersion)num;
			if (num < 1 || worldInfoVersion > WorldInfo.WorldInfoVersion.CurrentVersion)
			{
				throw new Exception("Bad Info Version");
			}
			this._terrainVersion = (WorldTypeIDs)reader.ReadInt32();
			this._name = reader.ReadString();
			this._ownerGamerTag = reader.ReadString();
			this._creatorGamerTag = reader.ReadString();
			this._createdDate = new DateTime(reader.ReadInt64());
			this._lastPlayedDate = new DateTime(reader.ReadInt64());
			this._seed = reader.ReadInt32();
			this._worldID = new Guid(reader.ReadBytes(16));
			this._lastPosition = reader.ReadVector3();
			int num2 = reader.ReadInt32();
			this.Crates.Clear();
			for (int i = 0; i < num2; i++)
			{
				Crate crate = new Crate(reader);
				this.Crates[crate.Location] = crate;
			}
			if (worldInfoVersion > WorldInfo.WorldInfoVersion.Doors)
			{
				int num3 = reader.ReadInt32();
				this.Doors.Clear();
				for (int j = 0; j < num3; j++)
				{
					Door door = new Door(reader);
					this.Doors[door.Location] = door;
				}
			}
			if (worldInfoVersion > WorldInfo.WorldInfoVersion.Spawners)
			{
				int num4 = reader.ReadInt32();
				this.Spawners.Clear();
				for (int k = 0; k < num4; k++)
				{
					Spawner spawner = new Spawner(reader);
					this.Spawners[spawner.Location] = spawner;
				}
			}
			this.InfiniteResourceMode = reader.ReadBoolean();
			this.ServerMessage = reader.ReadString();
			this.ServerPassword = reader.ReadString();
			if (worldInfoVersion > WorldInfo.WorldInfoVersion.HellBosses)
			{
				this.HellBossesSpawned = reader.ReadInt32();
				this.MaxHellBossSpawns = reader.ReadInt32();
			}
		}

		public WorldBuilder GetBuilder()
		{
			return new CastleMinerZBuilder(this);
		}

		public void Update(GameTime gameTime)
		{
			foreach (KeyValuePair<IntVector3, Spawner> keyValuePair in this.Spawners)
			{
				keyValuePair.Value.UpdateSpawner(gameTime);
			}
		}

		public static Vector3 DefaultStartLocation = new Vector3(8f, 128f, -8f);

		public Dictionary<IntVector3, Crate> Crates = new Dictionary<IntVector3, Crate>();

		public Dictionary<IntVector3, Door> Doors = new Dictionary<IntVector3, Door>();

		public Dictionary<IntVector3, Spawner> Spawners = new Dictionary<IntVector3, Spawner>();

		public static List<string> CorruptWorlds = new List<string>();

		private static readonly string BasePath = "Worlds";

		private static readonly string FileName = "world.info";

		private string _savePath;

		public WorldTypeIDs _terrainVersion = WorldTypeIDs.CastleMinerZ;

		private string _name = Strings.World;

		private string _ownerGamerTag;

		private string _creatorGamerTag;

		private DateTime _createdDate;

		private DateTime _lastPlayedDate;

		private int _seed;

		private Guid _worldID;

		private Vector3 _lastPosition = WorldInfo.DefaultStartLocation;

		public bool InfiniteResourceMode;

		public int HellBossesSpawned;

		public int MaxHellBossSpawns;

		public string ServerMessage = Strings.Server;

		public string ServerPassword = "";

		private enum WorldInfoVersion
		{
			Initial = 1,
			Doors,
			Spawners,
			HellBosses,
			CurrentVersion
		}
	}
}
