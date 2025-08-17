using System;
using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class DaysPastAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public DaysPastAchievement(string apiName, CastleMinerZAchievementManager manager, int days, string name)
			: base(apiName, manager, name, string.Concat(new string[]
			{
				Strings.Survive_for,
				" ",
				days.ToString(),
				" ",
				Strings.Days
			}))
		{
			this._days = days;
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.MaxDaysSurvived >= this._days;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp((float)base.PlayerStats.MaxDaysSurvived / (float)this._days, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int maxDaysSurvived = base.PlayerStats.MaxDaysSurvived;
				if (this._lastAmount != maxDaysSurvived)
				{
					this._lastAmount = maxDaysSurvived;
					this.lastString = string.Concat(new string[]
					{
						"(",
						maxDaysSurvived.ToString(),
						"/",
						this._days.ToString(),
						") ",
						Strings.Days_Survived
					});
				}
				return this.lastString;
			}
		}

		private int _days;

		private string lastString;

		private int _lastAmount = -1;
	}
}
