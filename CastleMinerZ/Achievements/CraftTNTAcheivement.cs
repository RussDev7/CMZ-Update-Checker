using System;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.Achievements
{
	public class CraftTNTAcheivement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public CraftTNTAcheivement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, manager, name, Strings.Craft + " " + Strings.TNT)
		{
		}

		protected override bool IsSastified
		{
			get
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				return itemStats.Crafted > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				if (itemStats.Crafted > 0)
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
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				int crafted = itemStats.Crafted;
				if (this._lastAmount != crafted)
				{
					this._lastAmount = crafted;
					this.lastString = crafted.ToString() + " " + Strings.TNT_Crafted;
				}
				return this.lastString;
			}
		}

		private string lastString;

		private int _lastAmount = -1;
	}
}
