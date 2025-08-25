using System;
using DNA.CastleMinerZ.Globalization;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class DragonType
	{
		public static DragonType GetDragonType(DragonTypeEnum et)
		{
			int idx = (int)et;
			if (idx < 0 || idx >= DragonType.Types.Length)
			{
				idx = DragonType.Types.Length - 1;
			}
			return DragonType.Types[idx];
		}

		public string GetDragonName()
		{
			switch (this.EType)
			{
			case DragonTypeEnum.FIRE:
				return Strings.Fire_Dragon;
			case DragonTypeEnum.FOREST:
				return Strings.Forest_Dragon;
			case DragonTypeEnum.LIZARD:
				return Strings.Sand_Dragon;
			case DragonTypeEnum.ICE:
				return Strings.Ice_Dragon;
			case DragonTypeEnum.SKELETON:
				return Strings.Undead_Dragon;
			default:
				return "";
			}
		}

		public static void Init()
		{
			if (DragonType.Types == null)
			{
				DragonType.Types = new DragonType[]
				{
					new DragonType(DragonTypeEnum.FIRE, DragonType.TextureNameEnum.FIRE, 20f, 30f, 0.4f, DragonDamageType.DESTRUCTION),
					new DragonType(DragonTypeEnum.FOREST, DragonType.TextureNameEnum.FOREST, 100f, 40f, 0.4f, DragonDamageType.DESTRUCTION),
					new DragonType(DragonTypeEnum.LIZARD, DragonType.TextureNameEnum.LIZARD, 300f, 40f, 0.4f, DragonDamageType.DESTRUCTION),
					new DragonType(DragonTypeEnum.ICE, DragonType.TextureNameEnum.ICE, 600f, 60f, 0.4f, DragonDamageType.ICE),
					new DragonType(DragonTypeEnum.SKELETON, DragonType.TextureNameEnum.SKELETON, 1000f, 60f, 0.4f, DragonDamageType.DESTRUCTION)
				};
				DragonType.BreakLookup = new bool[DragonType.Types.Length, 95];
				BlockTypeEnum[][] breaklists = new BlockTypeEnum[][]
				{
					new BlockTypeEnum[]
					{
						BlockTypeEnum.Rock,
						BlockTypeEnum.LanternFancy,
						BlockTypeEnum.Lantern,
						BlockTypeEnum.SpawnPointBasic,
						BlockTypeEnum.GlassBasic,
						BlockTypeEnum.GlassIron,
						BlockTypeEnum.GlassStrong,
						BlockTypeEnum.GlassMystery,
						BlockTypeEnum.GoldOre,
						BlockTypeEnum.IronOre,
						BlockTypeEnum.CopperOre,
						BlockTypeEnum.CoalOre,
						BlockTypeEnum.DiamondOre,
						BlockTypeEnum.IronWall,
						BlockTypeEnum.CopperWall,
						BlockTypeEnum.GoldenWall,
						BlockTypeEnum.DiamondWall
					},
					new BlockTypeEnum[]
					{
						BlockTypeEnum.LanternFancy,
						BlockTypeEnum.Lantern,
						BlockTypeEnum.SpawnPointBasic,
						BlockTypeEnum.GlassBasic,
						BlockTypeEnum.GlassIron,
						BlockTypeEnum.GlassStrong,
						BlockTypeEnum.GlassMystery,
						BlockTypeEnum.IronWall,
						BlockTypeEnum.CopperWall,
						BlockTypeEnum.GoldenWall,
						BlockTypeEnum.DiamondWall
					},
					new BlockTypeEnum[]
					{
						BlockTypeEnum.LanternFancy,
						BlockTypeEnum.Lantern,
						BlockTypeEnum.SpawnPointBasic,
						BlockTypeEnum.GlassIron,
						BlockTypeEnum.GlassStrong,
						BlockTypeEnum.GlassMystery,
						BlockTypeEnum.IronWall,
						BlockTypeEnum.GoldenWall,
						BlockTypeEnum.DiamondWall
					},
					new BlockTypeEnum[]
					{
						BlockTypeEnum.SpawnPointBasic,
						BlockTypeEnum.GlassStrong,
						BlockTypeEnum.GlassMystery,
						BlockTypeEnum.GoldenWall,
						BlockTypeEnum.DiamondWall
					},
					new BlockTypeEnum[]
					{
						BlockTypeEnum.SpawnPointBasic,
						BlockTypeEnum.GlassStrong,
						BlockTypeEnum.GlassMystery,
						BlockTypeEnum.DiamondWall
					}
				};
				for (int dnum = 0; dnum < DragonType.Types.Length; dnum++)
				{
					DragonType.BreakLookup[dnum, 0] = true;
					DragonType.BreakLookup[dnum, 5] = true;
					DragonType.BreakLookup[dnum, 14] = true;
					DragonType.BreakLookup[dnum, 20] = true;
					DragonType.BreakLookup[dnum, 21] = true;
					DragonType.BreakLookup[dnum, 44] = true;
					DragonType.BreakLookup[dnum, 43] = true;
					DragonType.BreakLookup[dnum, 94] = true;
					for (int mnum = 0; mnum < breaklists[dnum].Length; mnum++)
					{
						DragonType.BreakLookup[dnum, (int)breaklists[dnum][mnum]] = true;
					}
				}
			}
		}

		public DragonType(DragonTypeEnum type, DragonType.TextureNameEnum tname, float health, float fireballVelocity, float fireballDamage, DragonDamageType damageType)
		{
			this.Texture = CastleMinerZGame.Instance.Content.Load<Texture2D>(DragonType._textureNames[(int)tname]);
			this.EType = type;
			this.StartingHealth = health;
			this.Speed = 20f;
			this.MaxAccel = 10f;
			this.YawRate = 0.35f;
			this.RollRate = 1f;
			this.MaxRoll = 1f;
			this.PitchRate = 0.2f;
			this.MaxPitch = 0.4f;
			this.SlowViewCheckInterval = 2f;
			this.FastViewCheckInterval = 1f;
			this.HoverViewCheckInterval = 1f;
			this.MaxViewDistance = 300f;
			this.MaxAttackDistance = 200f;
			this.MinAttackDistance = 50f;
			this.BreakOffStrafeDistance = 40f;
			this.FireballDamage = fireballDamage;
			this.FireballVelocity = fireballVelocity;
			this.HoverDistance = 50f;
			this.LoiterDistance = 200f;
			this.SpawnDistance = 750f;
			this.CruisingAltitude = 120f;
			this.LoiterAltitude = 90f;
			this.HuntingAltitude = 70f;
			this.StrafeFireRate = 2f;
			this.HoverFireRate = 1f;
			this.MinLoiterTime = 2f;
			this.MaxLoiterTime = 6f;
			this.MinHoverShots = 4;
			this.MaxHoverShots = 8;
			this.ChanceOfHoverAttack = 0.5f;
			this.ChancesToNotAttack = 5;
			this.ShotHearingInterval = 5f;
			if (this.EType == DragonTypeEnum.SKELETON)
			{
				this.MaxLoiters = 100;
			}
			else
			{
				this.MaxLoiters = 3;
			}
			this.DamageType = damageType;
		}

		public static readonly string[] _textureNames = new string[] { "Enemies\\Dragon\\dragon-01_0", "Enemies\\Dragon\\dragon-09", "Enemies\\Dragon\\dragon-25", "Enemies\\Dragon\\dragon-27", "Enemies\\Dragon\\dragon-35" };

		public static DragonType[] Types = null;

		public static bool[,] BreakLookup;

		public Texture2D Texture;

		public DragonTypeEnum EType;

		public float StartingHealth;

		public float Speed;

		public float MaxAccel;

		public float Scale;

		public float YawRate;

		public float RollRate;

		public float MaxRoll;

		public float PitchRate;

		public float MaxPitch;

		public float SlowViewCheckInterval;

		public float FastViewCheckInterval;

		public float HoverViewCheckInterval;

		public float MaxViewDistance;

		public float HoverDistance;

		public float MaxAttackDistance;

		public float MinAttackDistance;

		public float BreakOffStrafeDistance;

		public float SpawnDistance;

		public float CruisingAltitude;

		public float LoiterAltitude;

		public float HuntingAltitude;

		public float StrafeFireRate;

		public float HoverFireRate;

		public float MinLoiterTime;

		public float MaxLoiterTime;

		public float LoiterDistance;

		public int MinHoverShots;

		public int MaxHoverShots;

		public float ChanceOfHoverAttack;

		public int ChancesToNotAttack;

		public float ShotHearingInterval;

		public float FireballVelocity;

		public float FireballDamage;

		public int MaxLoiters;

		public DragonDamageType DamageType;

		public enum TextureNameEnum
		{
			FIRE,
			FOREST,
			ICE,
			LIZARD,
			SKELETON,
			COUNT
		}
	}
}
