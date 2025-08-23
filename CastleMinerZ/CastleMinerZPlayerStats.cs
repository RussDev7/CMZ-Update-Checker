using System;
using System.Collections.Generic;
using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.Distribution;
using DNA.Distribution.Steam;
using DNA.Input;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class CastleMinerZPlayerStats : PlayerStats
	{
		public SteamWorks SteamAPI
		{
			get
			{
				if (this._steamAPI != null)
				{
					return this._steamAPI.Target;
				}
				return null;
			}
		}

		public CastleMinerZPlayerStats()
		{
			if (CastleMinerZGame.Instance != null)
			{
				this._steamAPI = new WeakReference<SteamWorks>(((SteamOnlineServices)CastleMinerZGame.Instance.LicenseServices).SteamAPI);
				this.SteamAPI.SetupStats(this._stats);
				this.SetDefaultStats();
			}
		}

		public override int Version
		{
			get
			{
				return 7;
			}
		}

		public int TotalKills
		{
			get
			{
				return this._stats[0].GetIntValue();
			}
			set
			{
				this._stats[0].SetValue(value);
			}
		}

		public int GamesPlayed
		{
			get
			{
				return this._stats[4].GetIntValue();
			}
			set
			{
				this._stats[4].SetValue(value);
			}
		}

		public int MaxDaysSurvived
		{
			get
			{
				return this._stats[1].GetIntValue();
			}
			set
			{
				this._stats[1].SetValue(value);
			}
		}

		public float MaxDistanceTraveled
		{
			get
			{
				return this._stats[5].GetFloatValue();
			}
			set
			{
				this._stats[5].SetValue(value);
			}
		}

		public float MaxDepth
		{
			get
			{
				return this._stats[6].GetFloatValue();
			}
			set
			{
				this._stats[6].SetValue(value);
			}
		}

		public int TotalItemsCrafted
		{
			get
			{
				return this._stats[7].GetIntValue();
			}
			set
			{
				this._stats[7].SetValue(value);
			}
		}

		public int UndeadDragonKills
		{
			get
			{
				return this._stats[8].GetIntValue();
			}
			set
			{
				this._stats[8].SetValue(value);
			}
		}

		public int ForestDragonKills
		{
			get
			{
				return this._stats[9].GetIntValue();
			}
			set
			{
				this._stats[9].SetValue(value);
			}
		}

		public int IceDragonKills
		{
			get
			{
				return this._stats[10].GetIntValue();
			}
			set
			{
				this._stats[10].SetValue(value);
			}
		}

		public int FireDragonKills
		{
			get
			{
				return this._stats[11].GetIntValue();
			}
			set
			{
				this._stats[11].SetValue(value);
			}
		}

		public int SandDragonKills
		{
			get
			{
				return this._stats[12].GetIntValue();
			}
			set
			{
				this._stats[12].SetValue(value);
			}
		}

		public int AlienEncounters
		{
			get
			{
				return this._stats[13].GetIntValue();
			}
			set
			{
				this._stats[13].SetValue(value);
			}
		}

		public int DragonsKilledWithGuidedMissile
		{
			get
			{
				return this._stats[14].GetIntValue();
			}
			set
			{
				this._stats[14].SetValue(value);
			}
		}

		public int EnemiesKilledWithTNT
		{
			get
			{
				return this._stats[15].GetIntValue();
			}
			set
			{
				this._stats[15].SetValue(value);
			}
		}

		public int EnemiesKilledWithGrenade
		{
			get
			{
				return this._stats[16].GetIntValue();
			}
			set
			{
				this._stats[16].SetValue(value);
			}
		}

		public int EnemiesKilledWithLaserWeapon
		{
			get
			{
				return this._stats[17].GetIntValue();
			}
			set
			{
				this._stats[17].SetValue(value);
			}
		}

		public void AddStat(SessionStats.StatType statType)
		{
			this.sessionStats.AddStat(statType);
		}

		public int BlocksDugCount(BlockTypeEnum type)
		{
			int ret;
			if (!this.BlocksDug.TryGetValue(type, out ret))
			{
				return 0;
			}
			return ret;
		}

		public void DugBlock(BlockTypeEnum type)
		{
			int ret = 0;
			this.BlocksDug.TryGetValue(type, out ret);
			ret++;
			this.BlocksDug[type] = ret;
		}

		public CastleMinerZPlayerStats.ItemStats GetItemStats(InventoryItemIDs ItemID)
		{
			CastleMinerZPlayerStats.ItemStats ret;
			if (!this.AllItemStats.TryGetValue(ItemID, out ret))
			{
				ret = new CastleMinerZPlayerStats.ItemStats(ItemID);
				this.AllItemStats[ItemID] = ret;
			}
			return ret;
		}

		private void SetupStatsForWriting()
		{
			this._stats[2].SetValue(this.Version);
			this._stats[3].SetValue((float)this.TimeOnline.TotalHours);
			this.SteamAPI.MinimalUpdate();
		}

		protected override void SaveData(BinaryWriter writer)
		{
			if (this.SteamAPI != null)
			{
				this.SetupStatsForWriting();
				this.SteamAPI.StoreStats();
			}
			writer.Write(this.TimeOfPurchase.Ticks);
			writer.Write(this.FirstPlayTime.Ticks);
			writer.Write((float)this.TimeInTrial.TotalMinutes);
			writer.Write((float)this.TimeInFull.TotalMinutes);
			writer.Write((float)this.TimeInMenu.TotalMinutes);
			writer.Write(this.BlocksDug.Count);
			foreach (KeyValuePair<BlockTypeEnum, int> pair in this.BlocksDug)
			{
				writer.Write((int)pair.Key);
				writer.Write(pair.Value);
			}
			writer.Write(this.AllItemStats.Count);
			foreach (KeyValuePair<InventoryItemIDs, CastleMinerZPlayerStats.ItemStats> pair2 in this.AllItemStats)
			{
				writer.Write((int)pair2.Key);
				pair2.Value.Write(writer);
			}
			writer.Write(this.BanList.Count);
			foreach (KeyValuePair<ulong, DateTime> pair3 in this.BanList)
			{
				writer.Write(pair3.Key);
				writer.Write(pair3.Value.Ticks);
			}
			writer.Write(this.SecondTrayFaded);
			writer.Write(this.InvertYAxis);
			writer.Write(this.brightness);
			writer.Write(this.musicVolume);
			writer.Write(this.controllerSensitivity);
			writer.Write(this.AutoClimb);
			writer.Write((byte)this.DrawDistance);
			writer.Write(this.PostOnAchievement);
			writer.Write(this.PostOnHost);
			writer.Write(this.AutoClimb);
			InputBinding binding = CastleMinerZGame.Instance._controllerMapping.Binding;
			binding.SaveData(writer);
			writer.Write(this.musicMute);
		}

		protected void SetDefaultStats()
		{
			this.TimeInFull = TimeSpan.Zero;
			this.TimeInMenu = TimeSpan.Zero;
			this.TimeInTrial = TimeSpan.Zero;
			this.TimeOnline = TimeSpan.Zero;
		}

		protected override void LoadData(BinaryReader reader, int version)
		{
			if (this.SteamAPI != null)
			{
				this.SteamAPI.RetrieveStats();
			}
			this.TimeOfPurchase = new DateTime(reader.ReadInt64());
			if (version < 0 || version > 7)
			{
				return;
			}
			this.FirstPlayTime = new DateTime(reader.ReadInt64());
			this.TimeInTrial = TimeSpan.FromMinutes((double)reader.ReadSingle());
			this.TimeInFull = TimeSpan.FromMinutes((double)reader.ReadSingle());
			this.TimeInMenu = TimeSpan.FromMinutes((double)reader.ReadSingle());
			int count = reader.ReadInt32();
			this.BlocksDug.Clear();
			for (int i = 0; i < count; i++)
			{
				this.BlocksDug[(BlockTypeEnum)reader.ReadInt32()] = reader.ReadInt32();
			}
			count = reader.ReadInt32();
			this.AllItemStats.Clear();
			for (int j = 0; j < count; j++)
			{
				InventoryItemIDs id = (InventoryItemIDs)reader.ReadInt32();
				CastleMinerZPlayerStats.ItemStats stat = new CastleMinerZPlayerStats.ItemStats(id);
				stat.Read(reader);
				this.AllItemStats[id] = stat;
			}
			count = reader.ReadInt32();
			this.BanList.Clear();
			for (int k = 0; k < count; k++)
			{
				ulong playerid = (ulong)reader.ReadInt64();
				this.BanList[playerid] = new DateTime(reader.ReadInt64());
			}
			if (version > 6)
			{
				this.SecondTrayFaded = reader.ReadBoolean();
			}
			this.InvertYAxis = reader.ReadBoolean();
			this.brightness = reader.ReadSingle();
			this.musicVolume = reader.ReadSingle();
			this.controllerSensitivity = reader.ReadSingle();
			this.controllerSensitivity = MathHelper.Clamp(this.controllerSensitivity, 0f, 1f);
			this.AutoClimb = reader.ReadBoolean();
			this.DrawDistance = (int)reader.ReadByte();
			this.PostOnAchievement = reader.ReadBoolean();
			this.PostOnHost = reader.ReadBoolean();
			this.AutoClimb = reader.ReadBoolean();
			if (version > 2)
			{
				InputBinding binding = CastleMinerZGame.Instance._controllerMapping.Binding;
				binding.LoadData(reader);
			}
			else
			{
				CastleMinerZGame.Instance._controllerMapping.SetToDefault();
			}
			if (version > 3)
			{
				this.musicMute = reader.ReadBoolean();
			}
			if (version == 4)
			{
				CastleMinerZGame.Instance._controllerMapping.SetToDefault();
			}
			if (version == 5)
			{
				CastleMinerZGame.Instance._controllerMapping.SetTrayDefaultKeys();
			}
		}

		private IStatInterface[] _stats = new IStatInterface[]
		{
			new CastleMinerZPlayerStats.CMZIntStat("TotalKills"),
			new CastleMinerZPlayerStats.CMZIntStat("MaxDaysSurvived"),
			new CastleMinerZPlayerStats.CMZIntStat("Version"),
			new CastleMinerZPlayerStats.CMZFloatStat("TimeInGame"),
			new CastleMinerZPlayerStats.CMZIntStat("GamesPlayed"),
			new CastleMinerZPlayerStats.CMZFloatStat("MaxDistanceTraveled"),
			new CastleMinerZPlayerStats.CMZFloatStat("MaxDepth"),
			new CastleMinerZPlayerStats.CMZIntStat("TotalItemsCrafted"),
			new CastleMinerZPlayerStats.CMZIntStat("UndeadDragonKills"),
			new CastleMinerZPlayerStats.CMZIntStat("ForestDragonKills"),
			new CastleMinerZPlayerStats.CMZIntStat("IceDragonKills"),
			new CastleMinerZPlayerStats.CMZIntStat("FireDragonKills"),
			new CastleMinerZPlayerStats.CMZIntStat("SandDragonKills"),
			new CastleMinerZPlayerStats.CMZIntStat("AlienEncounters"),
			new CastleMinerZPlayerStats.CMZIntStat("DragonsKilledWithGuidedMissile"),
			new CastleMinerZPlayerStats.CMZIntStat("EnemiesKilledWithTNT"),
			new CastleMinerZPlayerStats.CMZIntStat("EnemiesKilledWithGrenade"),
			new CastleMinerZPlayerStats.CMZIntStat("EnemiesKilledWithLaserWeapon")
		};

		private WeakReference<SteamWorks> _steamAPI;

		public TimeSpan TimeOnline;

		public DateTime TimeOfPurchase;

		public DateTime FirstPlayTime = DateTime.UtcNow;

		public TimeSpan TimeInTrial;

		public TimeSpan TimeInFull;

		public TimeSpan TimeInMenu;

		public bool SecondTrayFaded;

		public bool InvertYAxis;

		public float brightness;

		public float musicVolume = 1f;

		public bool musicMute;

		public float controllerSensitivity = 1f;

		public bool AutoClimb = true;

		public int DrawDistance = 1;

		public bool PostOnAchievement = true;

		public bool PostOnHost = true;

		private Dictionary<BlockTypeEnum, int> BlocksDug = new Dictionary<BlockTypeEnum, int>();

		private Dictionary<InventoryItemIDs, CastleMinerZPlayerStats.ItemStats> AllItemStats = new Dictionary<InventoryItemIDs, CastleMinerZPlayerStats.ItemStats>();

		public Dictionary<ulong, DateTime> BanList = new Dictionary<ulong, DateTime>();

		private SessionStats sessionStats = new SessionStats();

		public List<ServerInfo> ServerList = new List<ServerInfo>();

		private class CMZStatBase
		{
			public CMZStatBase(string name, StatType type)
			{
				this._apiName = name;
				this._statType = type;
				this._index = CastleMinerZPlayerStats.CMZStatBase._sIndexCount++;
			}

			public int GetIndex()
			{
				return this._index;
			}

			public string GetName()
			{
				return this._apiName;
			}

			public bool DidValueChange()
			{
				return this._valueChanged;
			}

			public void ClearValueChanged()
			{
				this._valueChanged = false;
			}

			public StatType ValueType()
			{
				return this._statType;
			}

			private static int _sIndexCount;

			private int _index;

			private string _apiName;

			protected bool _valueChanged;

			protected bool _initialized;

			private StatType _statType;
		}

		private class CMZIntStat : CastleMinerZPlayerStats.CMZStatBase, IStatInterface
		{
			public CMZIntStat(string name)
				: base(name, StatType.INT)
			{
			}

			public CMZIntStat(string name, int def)
				: base(name, StatType.INT)
			{
				this._value = def;
			}

			public float GetFloatValue()
			{
				return (float)this._value;
			}

			public int GetIntValue()
			{
				return this._value;
			}

			public void InitValue(float value)
			{
				this.InitValue((int)value);
			}

			public void SetValue(float value)
			{
				this.SetValue((int)value);
			}

			public void SetValue(int value)
			{
				if (this._value != value)
				{
					this._value = value;
					this._valueChanged = true;
				}
			}

			public void InitValue(int value)
			{
				this._value = value;
				this._initialized = true;
				this._valueChanged = false;
			}

			private int _value;
		}

		private class CMZFloatStat : CastleMinerZPlayerStats.CMZStatBase, IStatInterface
		{
			public CMZFloatStat(string name)
				: base(name, StatType.FLOAT)
			{
			}

			public CMZFloatStat(string name, float def)
				: base(name, StatType.FLOAT)
			{
				this._value = def;
			}

			public float GetFloatValue()
			{
				return this._value;
			}

			public int GetIntValue()
			{
				return (int)this._value;
			}

			public void SetValue(float value)
			{
				if (this._value != value)
				{
					this._value = value;
					this._valueChanged = true;
				}
			}

			public void InitValue(float value)
			{
				this._value = value;
				this._valueChanged = false;
				this._initialized = true;
			}

			public void SetValue(int value)
			{
				this.SetValue((float)value);
			}

			public void InitValue(int value)
			{
				this.SetValue((float)value);
			}

			private float _value;
		}

		public enum CMZStat
		{
			TotalKills,
			MaxDaysSurvived,
			Version,
			TimeInGame,
			GamesPlayed,
			MaxDistanceTraveled,
			MaxDepth,
			TotalItemsCrafted,
			UndeadDragonKills,
			ForestDragonKills,
			IceDragonKills,
			FireDragonKills,
			SandDragonKills,
			AlienEncounters,
			DragonsKilledWithGuidedMissile,
			EnemiesKilledWithTNT,
			EnemiesKilledWithGrenade,
			EnemiesKilledWithLaserWeapon
		}

		public class ItemStats
		{
			public InventoryItemIDs ItemID
			{
				get
				{
					return this._itemID;
				}
			}

			public ItemStats(InventoryItemIDs itemID)
			{
				this._itemID = itemID;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(this.TimeHeld.Ticks);
				writer.Write(this.Crafted);
				writer.Write(this.Used);
				writer.Write(this.Hits);
				writer.Write(this.KillsZombies);
				writer.Write(this.KillsSkeleton);
				writer.Write(this.KillsHell);
			}

			public void Read(BinaryReader reader)
			{
				this.TimeHeld = TimeSpan.FromTicks(reader.ReadInt64());
				this.Crafted = reader.ReadInt32();
				this.Used = reader.ReadInt32();
				this.Hits = reader.ReadInt32();
				this.KillsZombies = reader.ReadInt32();
				this.KillsSkeleton = reader.ReadInt32();
				this.KillsHell = reader.ReadInt32();
			}

			internal void AddStat(SessionStats.StatType category)
			{
				switch (category)
				{
				case SessionStats.StatType.ZombieDefeated:
					this.KillsZombies++;
					return;
				case SessionStats.StatType.SkeletonDefeated:
				case SessionStats.StatType.SKELETONARCHER:
				case SessionStats.StatType.SKELETONAXES:
				case SessionStats.StatType.SKELETONSWORD:
					this.KillsSkeleton++;
					break;
				case SessionStats.StatType.FelguardDefeated:
				case SessionStats.StatType.AlienDefeated:
				case SessionStats.StatType.DragonDefeated:
					break;
				case SessionStats.StatType.HellMinionDefeated:
					this.KillsHell++;
					return;
				default:
					return;
				}
			}

			private InventoryItemIDs _itemID;

			public TimeSpan TimeHeld;

			public int Crafted;

			public int Used;

			public int Hits;

			public int KillsZombies;

			public int KillsSkeleton;

			public int KillsHell;
		}

		private enum StatVersion
		{
			PreControlBinding = 2,
			PreGraphicsUpdate,
			PreFlySprintUpdate,
			PreExtraTrayUpdate,
			PreFadeTrayOption,
			CurrentVersion
		}
	}
}
