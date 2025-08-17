using System;
using DNA.CastleMinerZ.Globalization;
using Facebook;

namespace DNA.CastleMinerZ.Achievements
{
	public class CastleMinerZAchievementManager : AchievementManager<CastleMinerZPlayerStats>
	{
		public CastleMinerZAchievementManager(CastleMinerZGame game)
			: base(game.PlayerStats)
		{
			this._game = game;
		}

		public override void CreateAcheivements()
		{
			base.AddAcheivement(this.Achievements[0] = new PlayTimeAchievement("ACH_TIME_PLAYED_1", this, 1, Strings.Short_Timer));
			base.AddAcheivement(this.Achievements[1] = new PlayTimeAchievement("ACH_TIME_PLAYED_10", this, 10, Strings.Veteren_MinerZ));
			base.AddAcheivement(this.Achievements[2] = new PlayTimeAchievement("ACH_TIME_PLAYED_100", this, 100, Strings.MinerZ_Potato));
			base.AddAcheivement(this.Achievements[3] = new DistaceTraveledAchievement("ACH_DISTANCE_50", this, 50, Strings.First_Contact));
			base.AddAcheivement(this.Achievements[4] = new DistaceTraveledAchievement("ACH_DISTANCE_200", this, 200, Strings.Leaving_Home));
			base.AddAcheivement(this.Achievements[5] = new DistaceTraveledAchievement("ACH_DISTANCE_1000", this, 1000, Strings.Desert_Crawler));
			base.AddAcheivement(this.Achievements[6] = new DistaceTraveledAchievement("ACH_DISTANCE_2300", this, 2300, Strings.Mountain_Man));
			base.AddAcheivement(this.Achievements[7] = new DistaceTraveledAchievement("ACH_DISTANCE_3000", this, 3000, Strings.Deep_Freeze));
			base.AddAcheivement(this.Achievements[8] = new DistaceTraveledAchievement("ACH_DISTANCE_3600", this, 3600, Strings.Hell_On_Earth));
			base.AddAcheivement(this.Achievements[9] = new DistaceTraveledAchievement("ACH_DISTANCE_5000", this, 5000, Strings.Around_the_World));
			base.AddAcheivement(this.Achievements[10] = new DepthTraveledAchievement("ACH_DEPTH_20", this, -20f, Strings.Deep_Digger));
			base.AddAcheivement(this.Achievements[11] = new DepthTraveledAchievement("ACH_DEPTH_40", this, -40f, Strings.Welcome_To_Hell));
			base.AddAcheivement(this.Achievements[12] = new DaysPastAchievement("ACH_DAYS_1", this, 1, Strings.Survived_The_Night));
			base.AddAcheivement(this.Achievements[13] = new DaysPastAchievement("ACH_DAYS_7", this, 7, Strings.A_Week_Later));
			base.AddAcheivement(this.Achievements[14] = new DaysPastAchievement("ACH_DAYS_28", this, 28, Strings._28_Days_Later));
			base.AddAcheivement(this.Achievements[15] = new DaysPastAchievement("ACH_DAYS_100", this, 100, Strings.Survivor));
			base.AddAcheivement(this.Achievements[16] = new DaysPastAchievement("ACH_DAYS_196", this, 196, Strings._28_Weeks_Later));
			base.AddAcheivement(this.Achievements[17] = new DaysPastAchievement("ACH_DAYS_365", this, 365, Strings.Anniversary));
			base.AddAcheivement(this.Achievements[18] = new TotalCraftedAchievement("ACH_CRAFTED_1", this, 1, Strings.Tinkerer));
			base.AddAcheivement(this.Achievements[19] = new TotalCraftedAchievement("ACH_CRAFTED_100", this, 100, Strings.Crafter));
			base.AddAcheivement(this.Achievements[20] = new TotalCraftedAchievement("ACH_CRAFTED_1000", this, 1000, Strings.Master_Craftsman));
			base.AddAcheivement(this.Achievements[21] = new TotalKillsAchievement("ACH_TOTAL_KILLS_1", this, 1, Strings.Self_Defense));
			base.AddAcheivement(this.Achievements[22] = new TotalKillsAchievement("ACH_TOTAL_KILLS_100", this, 100, Strings.No_Fear));
			base.AddAcheivement(this.Achievements[23] = new TotalKillsAchievement("ACH_TOTAL_KILLS_1000", this, 1000, Strings.Zombie_Slayer));
			base.AddAcheivement(this.Achievements[24] = new UndeadKilledAchievement("ACH_UNDEAD_DRAGON_KILLED", this, Strings.Dragon_Slayer));
			base.AddAcheivement(this.Achievements[25] = new AlienEncounterAchievement("ACH_ALIEN_ENCOUNTER", this, Strings.Alien_Encounter));
			base.AddAcheivement(this.Achievements[26] = new EnemiesKilledWithLaserWeaponAchievement("ACH_LASER_KILLS", this, Strings.Alien_Technology));
			base.AddAcheivement(this.Achievements[27] = new CraftTNTAcheivement("ACH_CRAFT_TNT", this, Strings.Demolition_Expert));
			base.AddAcheivement(this.Achievements[28] = new KillDragonGuidedMissileAchievement("ACH_GUIDED_MISSILE_KILL", this, Strings.Air_Defense));
			base.AddAcheivement(this.Achievements[29] = new KillEnemyTNTAchievement("ACH_TNT_KILL", this, Strings.Fire_In_The_Hole));
			base.AddAcheivement(this.Achievements[30] = new KillEnemyGrenadeAchievement("ACH_GRENADE_KILL", this, Strings.Boom));
		}

		public override void OnAchieved(AchievementManager<CastleMinerZPlayerStats>.Achievement acheivement)
		{
			string apiname = acheivement.APIName;
			if (apiname != null)
			{
				base.PlayerStats.SteamAPI.SetAchievement(apiname, true);
			}
			if (this._game.PlayerStats.PostOnAchievement)
			{
				CastleMinerZGame.Instance.TaskScheduler.QueueUserWorkItem(delegate
				{
					try
					{
						new FacebookClient(CastleMinerZGame.FacebookAccessToken);
						new PostToWall
						{
							Message = string.Concat(new string[]
							{
								Strings.Has_earned,
								" ",
								acheivement.Name,
								" ",
								Strings.playing,
								" CastleMiner Z"
							}),
							Link = "http://castleminerz.com/",
							Description = Strings.Travel_with_your_friends_in_a_huge__ever_changing_world_and_craft_modern_weapons_to_defend_yourself_from_dragons_and_the_zombie_horde_,
							ActionName = Strings.Download_Now,
							ActionURL = "http://castleminerz.com/Download.html",
							ImageURL = "http://digitaldnagames.com/Images/CastleMinerZBox.jpg",
							AccessToken = CastleMinerZGame.FacebookAccessToken
						}.Post();
					}
					catch
					{
					}
				});
			}
			base.OnAchieved(acheivement);
		}

		private CastleMinerZGame _game;

		public AchievementManager<CastleMinerZPlayerStats>.Achievement[] Achievements = new AchievementManager<CastleMinerZPlayerStats>.Achievement[31];
	}
}
