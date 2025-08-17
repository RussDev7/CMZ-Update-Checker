using System;
using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class KillEnemyGrenadeAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public KillEnemyGrenadeAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, manager, name, Strings.Kill_An_Enemy_With_A_Grenade)
		{
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.EnemiesKilledWithGrenade > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.EnemiesKilledWithGrenade > 0)
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
				int enemiesKilledWithGrenade = base.PlayerStats.EnemiesKilledWithGrenade;
				if (this._lastAmount != enemiesKilledWithGrenade)
				{
					this._lastAmount = enemiesKilledWithGrenade;
					this.lastString = enemiesKilledWithGrenade.ToString() + " " + ((enemiesKilledWithGrenade == 1) ? Strings.Enemy_Killed : Strings.Enemies_Killed);
				}
				return this.lastString;
			}
		}

		private string lastString;

		private int _lastAmount = -1;
	}
}
