using System;
using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class PlayTimeAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public PlayTimeAchievement(string apiName, CastleMinerZAchievementManager manager, int hours, string name)
			: base(apiName, manager, name, string.Concat(new string[]
			{
				Strings.Play_Online_For,
				" ",
				hours.ToString(),
				" ",
				Strings.Hours
			}))
		{
			this._hours = hours;
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.TimeOnline.TotalHours >= (double)this._hours;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp((float)base.PlayerStats.TimeOnline.TotalHours / (float)this._hours, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int num = (int)base.PlayerStats.TimeOnline.TotalHours;
				if (this._lastAmount != num)
				{
					this._lastAmount = num;
					this.lastString = string.Concat(new string[]
					{
						"(",
						num.ToString(),
						"/",
						this._hours.ToString(),
						") ",
						Strings.hours_played
					});
				}
				return this.lastString;
			}
		}

		private int _hours;

		private string lastString;

		private int _lastAmount = -1;
	}
}
