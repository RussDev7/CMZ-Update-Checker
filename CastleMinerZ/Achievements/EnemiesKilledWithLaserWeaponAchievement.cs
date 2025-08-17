using System;
using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Achievements
{
	public class EnemiesKilledWithLaserWeaponAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		public EnemiesKilledWithLaserWeaponAchievement(string apiName, CastleMinerZAchievementManager manager, string name)
			: base(apiName, manager, name, Strings.Kill_An_Enemy_With_A_Laser_Weapon)
		{
		}

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.EnemiesKilledWithLaserWeapon > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.EnemiesKilledWithLaserWeapon > 0)
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
				int enemiesKilledWithLaserWeapon = base.PlayerStats.EnemiesKilledWithLaserWeapon;
				if (this._lastAmount != enemiesKilledWithLaserWeapon)
				{
					this._lastAmount = enemiesKilledWithLaserWeapon;
					this.lastString = enemiesKilledWithLaserWeapon.ToString() + " " + ((enemiesKilledWithLaserWeapon == 1) ? Strings.Enemy_Killed : Strings.Enemies_Killed);
				}
				return this.lastString;
			}
		}

		private string lastString;

		private int _lastAmount = -1;
	}
}
