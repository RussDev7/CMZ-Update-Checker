using System;
using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class AlienEncounterAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public AlienEncounterAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, manager, name, Strings.Find_An_Alien)
		{
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.AlienEncounters > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.AlienEncounters > 0)
				{
					return 1f;
				}
				return 0f;
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int alienEncounters = base.PlayerStats.AlienEncounters;
				if (this._lastAmount != alienEncounters)
				{
					this._lastAmount = alienEncounters;
					this.lastString = alienEncounters.ToString() + " " + ((alienEncounters == 1) ? Strings.Alien_Encounter : Strings.Alien_Encounters);
				}
				return this.lastString;
			}
		}

		private string lastString;

		private int _lastAmount = -1;
	}
}
