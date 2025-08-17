using System;
using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class TotalKillsAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public TotalKillsAchievement(string apiName, CastleMinerZAchievementManager manager, int kills, string name)
			: base(apiName, manager, name, Strings.Kill_ + kills.ToString() + " " + Strings.Enemies)
		{
			this._kills = kills;
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.TotalKills >= this._kills;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp((float)base.PlayerStats.TotalKills / (float)this._kills, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int totalKills = base.PlayerStats.TotalKills;
				if (this._lastAmount != totalKills)
				{
					this._lastAmount = totalKills;
					this.lastString = string.Concat(new string[]
					{
						"(",
						totalKills.ToString(),
						"/",
						this._kills.ToString(),
						") ",
						Strings.Enemies_Killed
					});
				}
				return this.lastString;
			}
		}

		private int _kills;

		private string lastString;

		private int _lastAmount = -1;
	}
}
