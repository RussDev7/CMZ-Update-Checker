using System;
using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class DepthTraveledAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public DepthTraveledAchievement(string apiName, CastleMinerZAchievementManager manager, float depth, string name)
			: base(apiName, manager, name, Strings.Travel_Down_At_Least + " " + ((int)(-(int)depth)).ToString())
		{
			this._depth = depth;
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.MaxDepth <= this._depth;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp(base.PlayerStats.MaxDepth / this._depth, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int num = (int)(-(int)base.PlayerStats.MaxDepth);
				if (this._lastAmount != num)
				{
					this._lastAmount = num;
					this.lastString = string.Concat(new string[]
					{
						"(",
						num.ToString(),
						"/",
						((int)(-(int)this._depth)).ToString(),
						") ",
						Strings.Distance_Traveled
					});
				}
				return this.lastString;
			}
		}

		private float _depth;

		private string lastString;

		private int _lastAmount = -1;
	}
}
