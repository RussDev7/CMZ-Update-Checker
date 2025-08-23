using System;

namespace DNA.CastleMinerZ.AI
{
	public class AlienEnemyType : EnemyType
	{
		public AlienEnemyType()
			: base(EnemyTypeEnum.ALIEN, EnemyType.ModelNameEnum.ALIEN, EnemyType.TextureNameEnum.ALIEN, EnemyType.FoundInEnum.CRASHSITE, SessionStats.StatType.AlienDefeated)
		{
			this.ChanceOfBulletStrike = 0.6f;
			this.SpawnRadius = 10;
			this.StartingDistanceLimit = 40;
		}

		public override float GetMaxSpeed()
		{
			return MathTools.RandomFloat(2f, 5.5f);
		}

		public override IFSMState<BaseZombie> GetEmergeState(BaseZombie entity)
		{
			return EnemyStates.AlienEmerge;
		}

		public override IFSMState<BaseZombie> GetAttackState(BaseZombie entity)
		{
			return EnemyStates.AlienAttack;
		}

		public override IFSMState<BaseZombie> GetChaseState(BaseZombie entity)
		{
			return EnemyStates.AlienChase;
		}

		public override IFSMState<BaseZombie> GetGiveUpState(BaseZombie entity)
		{
			return EnemyStates.AlienGiveUp;
		}

		public override IFSMState<BaseZombie> GetHitState(BaseZombie entity)
		{
			return EnemyStates.AlienHit;
		}

		public override IFSMState<BaseZombie> GetDieState(BaseZombie entity)
		{
			return EnemyStates.AlienDie;
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
				result *= 0.25f;
			}
			else if ((damageType & DamageType.SHOTGUN) != (DamageType)0)
			{
				result *= 1.25f;
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
	}
}
