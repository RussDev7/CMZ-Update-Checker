using System;
using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class DistaceTraveledAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public DistaceTraveledAchievement(string apiName, CastleMinerZAchievementManager manager, int distance, string name)
			: base(apiName, manager, name, Strings.Travel_At_Least + " " + distance.ToString())
		{
			this._distance = distance;
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.MaxDistanceTraveled >= (float)this._distance;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp(base.PlayerStats.MaxDistanceTraveled / (float)this._distance, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int total = (int)base.PlayerStats.MaxDistanceTraveled;
				if (this._lastAmount != total)
				{
					this._lastAmount = total;
					this.lastString = string.Concat(new string[]
					{
						"(",
						total.ToString(),
						"/",
						this._distance.ToString(),
						") ",
						Strings.Distance_Traveled
					});
				}
				return this.lastString;
			}
		}

		private int _distance;

		private string lastString;

		private int _lastAmount = -1;
	}
}
