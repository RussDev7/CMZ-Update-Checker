using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public abstract class EnemyType
	{
		public static EnemyType GetEnemyType(EnemyTypeEnum et)
		{
			return EnemyType.Types[(int)et];
		}

		public static void Init()
		{
			if (EnemyType.Types == null)
			{
				EnemyType.Types = new EnemyType[]
				{
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_0, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_0, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.1f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_1, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_1, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.1f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_2, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_2, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.2f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_3, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_3, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.2f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_4, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_4, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.3f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_5, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_5, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.3f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_6, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_6, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.4f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_1_0, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_0, EnemyType.FoundInEnum.ABOVEGROUND, 2, 0.4f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_1_1, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_1, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.5f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_1_2, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_2, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.5f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_1_3, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_3, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.6f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_1_4, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_4, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.6f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_1_5, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_5, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.7f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_0_6, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_6, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.7f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_2_0, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_0, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.8f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_2_1, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_1, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.8f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_2_3, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_2, EnemyType.FoundInEnum.ABOVEGROUND, 3, 0.9f),
					new ZombieEnemyType(EnemyTypeEnum.ZOMBIE_2_4, EnemyType.ModelNameEnum.ZOMBIE, EnemyType.TextureNameEnum.ZOMBIE_3, EnemyType.FoundInEnum.ABOVEGROUND, 3, 1f),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_0_0, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_0, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_0_1, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_1, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_0_2, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_2, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_0_3, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_3, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_0_4, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_4, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_1_0, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_0, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_1_1, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_1, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new ArcherSkeletonEnemyType(EnemyTypeEnum.ARCHER_1_2, EnemyType.ModelNameEnum.SKELETONARCHER, EnemyType.TextureNameEnum.SKELETON_2, EnemyType.FoundInEnum.ABOVEGROUND, SkeletonClassEnum.ARCHER),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_0_0, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_0, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_0_1, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_1, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_0_2, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_2, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_0_3, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_3, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_0_4, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_4, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_0_0, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_0, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_1_0, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_1, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_0_1, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_2, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_1_1, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_3, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_0_2, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_4, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_1_2, EnemyType.ModelNameEnum.SKELETONZOMBIE, EnemyType.TextureNameEnum.SKELETON_0, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.ZOMBIE),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_0_3, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_1, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_0_0, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_2, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_0_4, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_3, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_0_1, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_4, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_1_0, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_0, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_0_2, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_1, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_1_1, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_2, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_0_3, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_3, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_SWORD_1_2, EnemyType.ModelNameEnum.SKELETONSWORD, EnemyType.TextureNameEnum.SKELETON_4, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.SWORD),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_0_4, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_0, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_1_0, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_1, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_1_1, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_2, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new SkeletonEnemyType(EnemyTypeEnum.SKEL_AXES_1_2, EnemyType.ModelNameEnum.SKELETONAXES, EnemyType.TextureNameEnum.SKELETON_3, EnemyType.FoundInEnum.CAVES, SkeletonClassEnum.AXES),
					new FelguardEnemyType(false),
					new FelguardEnemyType(true),
					new AlienEnemyType()
				};
				for (int i = 0; i < EnemyType._modelNames.Length; i++)
				{
					CastleMinerZGame.Instance.Content.Load<Model>(EnemyType._modelNames[i]);
				}
				float num = 1f;
				for (EnemyTypeEnum enemyTypeEnum = EnemyTypeEnum.ZOMBIE_0_0; enemyTypeEnum <= EnemyTypeEnum.ZOMBIE_2_4; enemyTypeEnum++)
				{
					float num2 = (float)enemyTypeEnum / 16f;
					if (enemyTypeEnum == EnemyTypeEnum.ZOMBIE_2_4)
					{
						EnemyType.Types[(int)enemyTypeEnum].StartingHealth = num * 1.5f;
					}
					else
					{
						EnemyType.Types[(int)enemyTypeEnum].StartingHealth = num;
					}
					EnemyType.Types[(int)enemyTypeEnum].BaseSlowSpeed = 0.5f + num2 * 1.5f;
					EnemyType.Types[(int)enemyTypeEnum].BaseFastSpeed = 6.5f + num2 * 1.5f;
					EnemyType.Types[(int)enemyTypeEnum].BaseRunActivationTime = 4f - num2;
					EnemyType.Types[(int)enemyTypeEnum].RandomRunActivationTime = 1f;
					EnemyType.Types[(int)enemyTypeEnum].FastJumpSpeed = 13f + 3f * num2;
					num += 1f;
				}
				num = 1f;
				for (EnemyTypeEnum enemyTypeEnum2 = EnemyTypeEnum.ARCHER_0_0; enemyTypeEnum2 <= EnemyTypeEnum.ARCHER_1_2; enemyTypeEnum2++)
				{
					EnemyType.Types[(int)enemyTypeEnum2].StartingHealth = num;
					num += 2.5f;
				}
				num = 1f;
				for (EnemyTypeEnum enemyTypeEnum3 = EnemyTypeEnum.SKEL_0_0; enemyTypeEnum3 <= EnemyTypeEnum.SKEL_AXES_1_2; enemyTypeEnum3++)
				{
					EnemyType.Types[(int)enemyTypeEnum3].StartingHealth = num;
					num += 0.9f;
				}
				EnemyType.Types[50].StartingHealth = 150f;
				EnemyType.Types[51].StartingHealth = 300f;
				EnemyType.Types[52].StartingHealth = 70f;
			}
		}

		private static EnemyTypeEnum FindEnemy(float dstep, float distance, EnemyTypeEnum firstEnemy, EnemyTypeEnum lastEnemy)
		{
			int num = lastEnemy - firstEnemy;
			float num2 = distance / dstep;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = (int)Math.Floor((double)num2);
			int num6 = num5 - 1;
			int num7 = num6 - 1;
			float num8 = num2 - (float)num5;
			float num9;
			if (num5 > num)
			{
				num5 = num;
				num6 = num - 1;
				num7 = num - 2;
				num9 = 1f;
				num3 = 1f;
				num4 = 0.5f;
			}
			else
			{
				num9 = (float)Math.Sin((double)(num8 * 1.5707964f / 3f));
				if (num6 >= 0)
				{
					num3 = (float)Math.Sin((double)((1f + num8) * 1.5707964f / 3f));
				}
				if (num7 >= 0)
				{
					num4 = (float)Math.Sin((double)((2f + num8) * 1.5707964f / 3f));
				}
			}
			float num10 = MathTools.RandomFloat(num9 + num3 + num4);
			int num11;
			if (num10 <= num9)
			{
				num11 = num5;
			}
			else if (num10 <= num9 + num3)
			{
				num11 = num6;
			}
			else
			{
				num11 = num7;
			}
			return num11 + firstEnemy;
		}

		public static EnemyTypeEnum GetZombie(float distance)
		{
			if (EnemyManager.FelguardSpawned && EnemyManager.ReadyToSpawnFelgard && EnemyType.rand.NextDouble() < (double)EnemyType.felguardProbability)
			{
				EnemyManager.Instance.ResetFelgardTimer();
				return EnemyTypeEnum.FELGUARD;
			}
			return EnemyType.FindEnemy(188.88889f, distance, EnemyTypeEnum.ZOMBIE_0_0, EnemyTypeEnum.ZOMBIE_2_4);
		}

		public static EnemyTypeEnum GetAbovegroundEnemy(float percentMidnight, float distance)
		{
			float num = (float)Math.Pow((double)(1f - percentMidnight), 4.0);
			EnemyTypeEnum enemyTypeEnum;
			if (MathTools.RandomFloat() < num)
			{
				enemyTypeEnum = EnemyType.FindEnemy(425f, distance, EnemyTypeEnum.ARCHER_0_0, EnemyTypeEnum.ARCHER_1_2);
			}
			else
			{
				if (EnemyManager.FelguardSpawned && EnemyManager.ReadyToSpawnFelgard && EnemyType.rand.NextDouble() < (double)EnemyType.felguardProbability)
				{
					EnemyManager.Instance.ResetFelgardTimer();
					return EnemyTypeEnum.FELGUARD;
				}
				enemyTypeEnum = EnemyType.FindEnemy(188.88889f, distance, EnemyTypeEnum.ZOMBIE_0_0, EnemyTypeEnum.ZOMBIE_2_4);
			}
			return enemyTypeEnum;
		}

		public static EnemyTypeEnum GetBelowgroundEnemy(float depth, float distance)
		{
			float num = 141.66667f;
			distance += depth * 2f * num / 50f;
			float num2 = (float)EnemyType.rand.NextDouble();
			if (EnemyManager.ReadyToSpawnFelgard && (num2 < EnemyType.felguardProbability || !EnemyManager.FelguardSpawned))
			{
				EnemyTypeEnum enemyTypeEnum = EnemyType.FindEnemy(num, distance, EnemyTypeEnum.SKEL_0_0, EnemyTypeEnum.FELGUARD);
				if (enemyTypeEnum == EnemyTypeEnum.FELGUARD)
				{
					EnemyManager.Instance.ResetFelgardTimer();
					EnemyManager.FelguardSpawned = true;
				}
				return enemyTypeEnum;
			}
			return EnemyType.FindEnemy(num, distance, EnemyTypeEnum.SKEL_0_0, EnemyTypeEnum.SKEL_AXES_1_2);
		}

		public abstract float GetDamageTypeMultiplier(DamageType damageType, bool headShot);

		public abstract IFSMState<BaseZombie> GetEmergeState(BaseZombie entity);

		public abstract IFSMState<BaseZombie> GetAttackState(BaseZombie entity);

		public abstract IFSMState<BaseZombie> GetGiveUpState(BaseZombie entity);

		public abstract IFSMState<BaseZombie> GetHitState(BaseZombie entity);

		public abstract IFSMState<BaseZombie> GetDieState(BaseZombie entity);

		public abstract IFSMState<BaseZombie> GetDigState(BaseZombie entity);

		public virtual IFSMState<BaseZombie> GetRestartState(BaseZombie entity)
		{
			return this.GetChaseState(entity);
		}

		public virtual float GetMaxSpeed()
		{
			return 2f;
		}

		public virtual float GetSlowSpeed()
		{
			return this.BaseSlowSpeed;
		}

		public virtual float GetFastSpeed()
		{
			return this.BaseFastSpeed;
		}

		public virtual IFSMState<BaseZombie> GetChaseState(BaseZombie entity)
		{
			return EnemyStates.Chase;
		}

		public EnemyType(EnemyTypeEnum t, EnemyType.ModelNameEnum model, EnemyType.TextureNameEnum tname, EnemyType.FoundInEnum foundin, SessionStats.StatType category)
		{
			this.EType = t;
			this.ModelName = EnemyType._modelNames[(int)model];
			this.Scale = EnemyType._modelScales[(int)model];
			this.Facing = EnemyType._modelFacing[(int)model];
			this.FoundIn = foundin;
			this.EnemyTexture = CastleMinerZGame.Instance.Content.Load<Texture2D>(EnemyType._textureNames[(int)tname]);
			this.SpawnRadius = 15;
			this.StartingDistanceLimit = 25;
			this.TextureIndex = tname;
			this.Category = category;
		}

		public EnemyType.InitPackage CreateInitPackage(float midnight)
		{
			EnemyType.InitPackage initPackage = default(EnemyType.InitPackage);
			initPackage.SlowSpeed = MathTools.RandomFloat(this.BaseSlowSpeed, this.BaseSlowSpeed + this.RandomSlowSpeed);
			if (this.HasRunFast)
			{
				initPackage.FastSpeed = MathTools.RandomFloat(this.BaseFastSpeed - 0.25f, this.BaseFastSpeed + 0.25f);
				initPackage.RunActivationTime = MathHelper.Lerp(this.BaseRunActivationTime, this.BaseRunActivationTime * 0.5f, MathTools.RandomFloat(midnight));
				initPackage.NormalActivationTime = 45f;
				if (midnight > 0.8f)
				{
					initPackage.EmergeSpeed = this.SpawnAnimationSpeed;
				}
				else if (this.SpawnAnimationSpeed == 1f)
				{
					initPackage.EmergeSpeed = 1f;
				}
				else
				{
					initPackage.EmergeSpeed = MathHelper.Lerp(this.SpawnAnimationSpeed / 2f, this.SpawnAnimationSpeed, Math.Min(1f, midnight * 2f));
				}
			}
			else
			{
				initPackage.FastSpeed = 0f;
				initPackage.RunActivationTime = 0f;
				initPackage.NormalActivationTime = 0f;
				initPackage.EmergeSpeed = this.SpawnAnimationSpeed;
			}
			return initPackage;
		}

		private const float DISTANCE_TO_HELL = 3400f;

		public static readonly string[] _modelNames = new string[] { "Enemies\\Zombies\\Zombie", "Enemies\\Skeletons\\SkeletonZombie", "Enemies\\Skeletons\\SkeletonArcher", "Enemies\\Skeletons\\SkeletonAxes", "Enemies\\Skeletons\\SkeletonSword", "Enemies\\Demon\\Demon", "Enemies\\Demon\\Demon", "Enemies\\Alien\\alien" };

		public static readonly float[] _modelScales = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1.3f, 1f, 1f };

		public static readonly float[] _modelFacing = new float[] { 3.141592f, 3.141592f, 3.141592f, 3.141592f, 3.141592f, 3.141592f, 3.141592f, 3.141592f, 3.141592f };

		public static readonly string[] _textureNames = new string[]
		{
			"Enemies\\Zombies\\Diffuse01_0", "Enemies\\Zombies\\Diffuse06", "Enemies\\Zombies\\Diffuse10", "Enemies\\Zombies\\Diffuse11", "Enemies\\Zombies\\Diffuse13", "Enemies\\Zombies\\Diffuse14", "Enemies\\Zombies\\Diffuse17", "Enemies\\Skeletons\\Diffuse01_0", "Enemies\\Skeletons\\Diffuse04", "Enemies\\Skeletons\\Diffuse05",
			"Enemies\\Skeletons\\Diffuse06", "Enemies\\Skeletons\\Diffuse07", "Enemies\\Demon\\Diffuse_0", "Enemies\\Demon\\Diffuse_0", "Enemies\\Alien\\Diffuse_0", "Enemies\\Zombies\\Treasure\\DiffuseTreasure", "Enemies\\AntlerBeast\\PA3_5", "Enemies\\Reaper\\rb_ao"
		};

		public static EnemyType[] Types = null;

		private static Random rand = new Random();

		private static float felguardProbability = 0.1f;

		public EnemyTypeEnum EType;

		public EnemyType.FoundInEnum FoundIn;

		public Texture2D EnemyTexture;

		public EnemyType.TextureNameEnum TextureIndex;

		public SessionStats.StatType Category;

		public float Scale;

		public float Facing;

		public float ChanceOfBulletStrike;

		public string ModelName;

		public float StartingHealth;

		public int SpawnRadius;

		public int StartingDistanceLimit;

		public float AttackAnimationSpeed = 1f;

		public float DieAnimationSpeed = 1f;

		public float HitAnimationSpeed = 1f;

		public float SpawnAnimationSpeed = 1f;

		public bool HasRunFast;

		public float FastJumpSpeed = 10f;

		public float BaseSlowSpeed = 2f;

		public float RandomSlowSpeed = 3.5f;

		public float BaseFastSpeed = 6f;

		public float BaseRunActivationTime = 1000f;

		public float RandomRunActivationTime;

		public float DiggingMultiplier = 1f;

		public int HardestBlockThatCanBeDug = 2;

		public float BaseNormalActivationTime = 1000f;

		public enum ModelNameEnum
		{
			ZOMBIE,
			SKELETONZOMBIE,
			SKELETONARCHER,
			SKELETONAXES,
			SKELETONSWORD,
			FELGUARD,
			HELL_LORD,
			ALIEN,
			REAPER
		}

		public enum TextureNameEnum
		{
			ZOMBIE_0,
			ZOMBIE_1,
			ZOMBIE_2,
			ZOMBIE_3,
			ZOMBIE_4,
			ZOMBIE_5,
			ZOMBIE_6,
			SKELETON_0,
			SKELETON_1,
			SKELETON_2,
			SKELETON_3,
			SKELETON_4,
			FELGUARD,
			HELL_LORD,
			ALIEN,
			TREASURE_ZOMBIE,
			ANTLER_BEAST,
			REAPER,
			COUNT
		}

		public enum FoundInEnum
		{
			ABOVEGROUND,
			CAVES,
			HELL,
			CRASHSITE
		}

		public struct InitPackage
		{
			public static EnemyType.InitPackage Read(BinaryReader reader)
			{
				return new EnemyType.InitPackage
				{
					SlowSpeed = (float)reader.ReadInt32() / 1000f,
					FastSpeed = (float)reader.ReadInt32() / 1000f,
					EmergeSpeed = (float)reader.ReadInt32() / 1000f,
					RunActivationTime = (float)reader.ReadInt32() / 1000f,
					NormalActivationTime = (float)reader.ReadInt32() / 1000f
				};
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write((int)(1000f * this.SlowSpeed));
				writer.Write((int)(1000f * this.FastSpeed));
				writer.Write((int)(1000f * this.EmergeSpeed));
				writer.Write((int)(1000f * this.RunActivationTime));
				writer.Write((int)(1000f * this.NormalActivationTime));
			}

			public float SlowSpeed;

			public float FastSpeed;

			public float EmergeSpeed;

			public float RunActivationTime;

			public float NormalActivationTime;
		}
	}
}
