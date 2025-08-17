using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Net;

namespace DNA.CastleMinerZ
{
	public class SessionStats
	{
		public SessionStats()
		{
			this.statMap.Add(SessionStats.StatType.ZombieDefeated, new SessionStats.SessionStatsData(0, 10, SessionStats.StatAction.HasDefeated, Strings.Zombies, Strings.Zombies));
			this.statMap.Add(SessionStats.StatType.HellMinionDefeated, new SessionStats.SessionStatsData(0, 10, SessionStats.StatAction.HasDefeated, Strings.Skeletons, Strings.Skeletons));
			this.statMap.Add(SessionStats.StatType.SkeletonDefeated, new SessionStats.SessionStatsData(0, 10, SessionStats.StatAction.HasDefeated, Strings.Skeletons, Strings.Skeletons));
			this.statMap.Add(SessionStats.StatType.DragonDefeated, new SessionStats.SessionStatsData(0, 1, SessionStats.StatAction.HasDefeated, Strings.Dragon, Strings.Dragons));
			this.statMap.Add(SessionStats.StatType.AlienDefeated, new SessionStats.SessionStatsData(0, 5, SessionStats.StatAction.HasDefeated, Strings.Alien, Strings.Aliens));
			this.statMap.Add(SessionStats.StatType.FelguardDefeated, new SessionStats.SessionStatsData(0, 1, SessionStats.StatAction.HasDefeated, Strings.Underlord, Strings.Underlords));
			this.statMap.Add(SessionStats.StatType.PlayerDefeated, new SessionStats.SessionStatsData(0, 10, SessionStats.StatAction.HasFallen, Strings.Times, Strings.Times));
			this.statMap.Add(SessionStats.StatType.LootBlockOpened, new SessionStats.SessionStatsData(0, 10, SessionStats.StatAction.HasOpened, Strings.Loot_Block, Strings.Loot_Blocks));
			this.statMap.Add(SessionStats.StatType.LuckyLootBlockOpened, new SessionStats.SessionStatsData(0, 5, SessionStats.StatAction.HasOpened, Strings.Lucky_Loot_Block, Strings.Lucky_Loot_Blocks));
		}

		internal void AddStat(SessionStats.StatType category)
		{
			if (!this.statMap.ContainsKey(category))
			{
				return;
			}
			SessionStats.SessionStatsData sessionStatsData = this.statMap[category];
			sessionStatsData.Count++;
			this.statMap[category] = sessionStatsData;
			this.CheckProgress(sessionStatsData, category);
		}

		private string GetActionAsString(SessionStats.StatAction action)
		{
			string text = "has";
			switch (action)
			{
			case SessionStats.StatAction.HasDefeated:
				text = Strings.Has_Defeated;
				break;
			case SessionStats.StatAction.HasOpened:
				text = Strings.Has_Opened;
				break;
			case SessionStats.StatAction.HasFallen:
				text = Strings.Has_Fallen;
				break;
			}
			return text;
		}

		private string GetBroadcastString(SessionStats.SessionStatsData statData)
		{
			string text = statData.Count.ToString() + " " + ((statData.Count == 1) ? statData.StatSingular : statData.StatPlural);
			return string.Concat(new string[]
			{
				CastleMinerZGame.Instance.LocalPlayer.Gamer.Gamertag,
				" ",
				this.GetActionAsString(statData.Action),
				" ",
				text
			});
		}

		private void CheckProgress(SessionStats.SessionStatsData statData, SessionStats.StatType category)
		{
			int displayIncrement = statData.DisplayIncrement;
			int count = statData.Count;
			if (count % displayIncrement == 0 && this._broadcastStatsAllowed)
			{
				BroadcastTextMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, this.GetBroadcastString(statData));
			}
		}

		private bool _broadcastStatsAllowed = true;

		private Dictionary<SessionStats.StatType, SessionStats.SessionStatsData> statMap = new Dictionary<SessionStats.StatType, SessionStats.SessionStatsData>();

		public enum StatType
		{
			ZombieDefeated,
			SkeletonDefeated,
			SKELETONARCHER,
			SKELETONAXES,
			SKELETONSWORD,
			FelguardDefeated,
			AlienDefeated,
			DragonDefeated,
			HellMinionDefeated,
			LootBlockOpened,
			LuckyLootBlockOpened,
			PlayerDefeated
		}

		public enum StatAction
		{
			HasDefeated,
			HasOpened,
			HasFallen
		}

		public struct SessionStatsData
		{
			public SessionStatsData(int count, int displayIncrement, SessionStats.StatAction action, string statSingular, string statPlural)
			{
				this.Count = count;
				this.DisplayIncrement = displayIncrement;
				this.Action = action;
				this.StatSingular = statSingular;
				this.StatPlural = statPlural;
			}

			public int Count;

			public int DisplayIncrement;

			public string StatSingular;

			public string StatPlural;

			public SessionStats.StatAction Action;
		}
	}
}
