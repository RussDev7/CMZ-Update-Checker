using System;
using DNA.CastleMinerZ.Globalization;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Achievements
{
	public class TotalCraftedAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public TotalCraftedAchievement(string apiName, CastleMinerZAchievementManager manager, int items, string name)
			: base(apiName, manager, name, string.Concat(new string[]
			{
				Strings.Craft,
				" ",
				items.ToString(),
				" ",
				Strings.items
			}))
		{
			this._items = items;
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.TotalItemsCrafted >= this._items;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				return MathHelper.Clamp((float)base.PlayerStats.TotalItemsCrafted / (float)this._items, 0f, 1f);
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int total = base.PlayerStats.TotalItemsCrafted;
				if (this._lastAmount != total)
				{
					this._lastAmount = total;
					this.lastString = string.Concat(new string[]
					{
						"(",
						total.ToString(),
						"/",
						this._items.ToString(),
						") ",
						Strings.Items_Crafted
					});
				}
				return this.lastString;
			}
		}

		private int _items;

		private string lastString;

		private int _lastAmount = -1;
	}
}
