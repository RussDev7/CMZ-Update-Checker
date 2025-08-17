using System;
using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class UndeadKilledAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public UndeadKilledAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, manager, name, Strings.Kill_The_Undead_Dragon)
		{
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.UndeadDragonKills > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.UndeadDragonKills > 0)
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
				if (base.PlayerStats.UndeadDragonKills > 0)
				{
					return Strings.Undead_Dragon_Killed;
				}
				return Strings.Undead_Dragon_Not_Killed;
			}
		}
	}
}
