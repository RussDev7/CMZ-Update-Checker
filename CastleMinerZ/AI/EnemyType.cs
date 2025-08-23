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
				float health = 1f;
				for (EnemyTypeEnum t = EnemyTypeEnum.ZOMBIE_0_0; t <= EnemyTypeEnum.ZOMBIE_2_4; t++)
				{
					float progress = (float)t / 16f;
					if (t == EnemyTypeEnum.ZOMBIE_2_4)
					{
						EnemyType.Types[(int)t].StartingHealth = health * 1.5f;
					}
					else
					{
						EnemyType.Types[(int)t].StartingHealth = health;
					}
					EnemyType.Types[(int)t].BaseSlowSpeed = 0.5f + progress * 1.5f;
					EnemyType.Types[(int)t].BaseFastSpeed = 6.5f + progress * 1.5f;
					EnemyType.Types[(int)t].BaseRunActivationTime = 4f - progress;
					EnemyType.Types[(int)t].RandomRunActivationTime = 1f;
					EnemyType.Types[(int)t].FastJumpSpeed = 13f + 3f * progress;
					health += 1f;
				}
				health = 1f;
				for (EnemyTypeEnum t2 = EnemyTypeEnum.ARCHER_0_0; t2 <= EnemyTypeEnum.ARCHER_1_2; t2++)
				{
					EnemyType.Types[(int)t2].StartingHealth = health;
					health += 2.5f;
				}
				health = 1f;
				for (EnemyTypeEnum t3 = EnemyTypeEnum.SKEL_0_0; t3 <= EnemyTypeEnum.SKEL_AXES_1_2; t3++)
				{
					EnemyType.Types[(int)t3].StartingHealth = health;
					health += 0.9f;
				}
				EnemyType.Types[50].StartingHealth = 150f;
				EnemyType.Types[51].StartingHealth = 300f;
				EnemyType.Types[52].StartingHealth = 70f;
			}
		}

		private static EnemyTypeEnum FindEnemy(float dstep, float distance, EnemyTypeEnum firstEnemy, EnemyTypeEnum lastEnemy)
		{
			int maxIndex = lastEnemy - firstEnemy;
			float dplace = distance / dstep;
			float secondProb = 0f;
			float thirdProb = 0f;
			int firstIndex = (int)Math.Floor((double)dplace);
			int secondIndex = firstIndex - 1;
			int thirdIndex = secondIndex - 1;
			float frac = dplace - (float)firstIndex;
			float firstProb;
			if (firstIndex > maxIndex)
			{
				firstIndex = maxIndex;
				secondIndex = maxIndex - 1;
				thirdIndex = maxIndex - 2;
				firstProb = 1f;
				secondProb = 1f;
				thirdProb = 0.5f;
			}
			else
			{
				firstProb = (float)Math.Sin((double)(frac * 1.5707964f / 3f));
				if (secondIndex >= 0)
				{
					secondProb = (float)Math.Sin((double)((1f + frac) * 1.5707964f / 3f));
				}
				if (thirdIndex >= 0)
				{
					thirdProb = (float)Math.Sin((double)((2f + frac) * 1.5707964f / 3f));
				}
			}
			float r = MathTools.RandomFloat(firstProb + secondProb + thirdProb);
			int zombieIndex;
			if (r <= firstProb)
			{
				zombieIndex = firstIndex;
			}
			else if (r <= firstProb + secondProb)
			{
				zombieIndex = secondIndex;
			}
			else
			{
				zombieIndex = thirdIndex;
			}
			return zombieIndex + firstEnemy;
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
			float chanceOfArcher = (float)Math.Pow((double)(1f - percentMidnight), 4.0);
			EnemyTypeEnum result;
			if (MathTools.RandomFloat() < chanceOfArcher)
			{
				result = EnemyType.FindEnemy(425f, distance, EnemyTypeEnum.ARCHER_0_0, EnemyTypeEnum.ARCHER_1_2);
			}
			else
			{
				if (EnemyManager.FelguardSpawned && EnemyManager.ReadyToSpawnFelgard && EnemyType.rand.NextDouble() < (double)EnemyType.felguardProbability)
				{
					EnemyManager.Instance.ResetFelgardTimer();
					return EnemyTypeEnum.FELGUARD;
				}
				result = EnemyType.FindEnemy(188.88889f, distance, EnemyTypeEnum.ZOMBIE_0_0, EnemyTypeEnum.ZOMBIE_2_4);
			}
			return result;
		}

		public static EnemyTypeEnum GetBelowgroundEnemy(float depth, float distance)
		{
			float bandSize = 141.66667f;
			distance += depth * 2f * bandSize / 50f;
			float random = (float)EnemyType.rand.NextDouble();
			if (EnemyManager.ReadyToSpawnFelgard && (random < EnemyType.felguardProbability || !EnemyManager.FelguardSpawned))
			{
				EnemyTypeEnum enemy = EnemyType.FindEnemy(bandSize, distance, EnemyTypeEnum.SKEL_0_0, EnemyTypeEnum.FELGUARD);
				if (enemy == EnemyTypeEnum.FELGUARD)
				{
					EnemyManager.Instance.ResetFelgardTimer();
					EnemyManager.FelguardSpawned = true;
				}
				return enemy;
			}
			return EnemyType.FindEnemy(bandSize, distance, EnemyTypeEnum.SKEL_0_0, EnemyTypeEnum.SKEL_AXES_1_2);
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
			EnemyType.InitPackage result = default(EnemyType.InitPackage);
			result.SlowSpeed = MathTools.RandomFloat(this.BaseSlowSpeed, this.BaseSlowSpeed + this.RandomSlowSpeed);
			if (this.HasRunFast)
			{
				result.FastSpeed = MathTools.RandomFloat(this.BaseFastSpeed - 0.25f, this.BaseFastSpeed + 0.25f);
				result.RunActivationTime = MathHelper.Lerp(this.BaseRunActivationTime, this.BaseRunActivationTime * 0.5f, MathTools.RandomFloat(midnight));
				result.NormalActivationTime = 45f;
				if (midnight > 0.8f)
				{
					result.EmergeSpeed = this.SpawnAnimationSpeed;
				}
				else if (this.SpawnAnimationSpeed == 1f)
				{
					result.EmergeSpeed = 1f;
				}
				else
				{
					result.EmergeSpeed = MathHelper.Lerp(this.SpawnAnimationSpeed / 2f, this.SpawnAnimationSpeed, Math.Min(1f, midnight * 2f));
				}
			}
			else
			{
				result.FastSpeed = 0f;
				result.RunActivationTime = 0f;
				result.NormalActivationTime = 0f;
				result.EmergeSpeed = this.SpawnAnimationSpeed;
			}
			return result;
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
