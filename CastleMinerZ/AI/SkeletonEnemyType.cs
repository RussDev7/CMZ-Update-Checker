using System;

namespace DNA.CastleMinerZ.AI
{
	public class SkeletonEnemyType : EnemyType
	{
		public SkeletonEnemyType(EnemyTypeEnum t, EnemyType.ModelNameEnum model, EnemyType.TextureNameEnum tname, EnemyType.FoundInEnum foundin, SkeletonClassEnum skelclass)
			: base(t, model, tname, foundin, SessionStats.StatType.SkeletonDefeated)
		{
			this.SkeletonClass = skelclass;
			this.ChanceOfBulletStrike = 0.6f;
			this.SpawnRadius = 10;
		}

		public override float GetMaxSpeed()
		{
			return MathTools.RandomFloat(2f, 5.5f);
		}

		public override IFSMState<BaseZombie> GetEmergeState(BaseZombie entity)
		{
			return EnemyStates.Chase;
		}

		public override IFSMState<BaseZombie> GetAttackState(BaseZombie entity)
		{
			if (this.SkeletonClass == SkeletonClassEnum.AXES)
			{
				return EnemyStates.AxeSkeletonTryAttack;
			}
			return EnemyStates.SkeletonTryAttack;
		}

		public override IFSMState<BaseZombie> GetGiveUpState(BaseZombie entity)
		{
			return EnemyStates.SkeletonGiveUp;
		}

		public override IFSMState<BaseZombie> GetHitState(BaseZombie entity)
		{
			return EnemyStates.SkeletonHit;
		}

		public override IFSMState<BaseZombie> GetDieState(BaseZombie entity)
		{
			return EnemyStates.SkeletonDie;
		}

		public override IFSMState<BaseZombie> GetDigState(BaseZombie entity)
		{
			return this.GetGiveUpState(entity);
		}

		public override float GetDamageTypeMultiplier(DamageType damageType, bool headShot)
		{
			float result = 1f;
			if ((damageType & DamageType.PIERCING) != (DamageType)0)
			{
				result *= 0.5f;
			}
			else if ((damageType & DamageType.SHOTGUN) != (DamageType)0)
			{
				result *= 1.5f;
			}
			else if ((damageType & DamageType.BLADE) != (DamageType)0)
			{
				result *= 0.75f;
			}
			if (headShot)
			{
				result *= 2f;
			}
			return result;
		}

		public SkeletonClassEnum SkeletonClass;
	}
}
