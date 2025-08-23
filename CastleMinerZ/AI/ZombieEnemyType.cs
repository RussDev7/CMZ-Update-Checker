using System;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieEnemyType : EnemyType
	{
		public ZombieEnemyType(EnemyTypeEnum t, EnemyType.ModelNameEnum model, EnemyType.TextureNameEnum tname, EnemyType.FoundInEnum foundin, int maxDigHardness, float digMultiplier)
			: base(t, model, tname, foundin, SessionStats.StatType.ZombieDefeated)
		{
			this.ChanceOfBulletStrike = 1f;
			this.SpawnRadius = 20;
			this.AttackAnimationSpeed = 2f;
			this.DieAnimationSpeed = 2f;
			this.HitAnimationSpeed = 2f;
			this.SpawnAnimationSpeed = 3f;
			this.HasRunFast = true;
			this.BaseSlowSpeed = 2.5f;
			this.RandomSlowSpeed = 0.5f;
			this.DiggingMultiplier = digMultiplier;
			this.HardestBlockThatCanBeDug = maxDigHardness;
		}

		public override IFSMState<BaseZombie> GetEmergeState(BaseZombie entity)
		{
			return EnemyStates.ZombieEmerge;
		}

		public override IFSMState<BaseZombie> GetAttackState(BaseZombie entity)
		{
			return EnemyStates.ZombieTryAttack;
		}

		public override IFSMState<BaseZombie> GetGiveUpState(BaseZombie entity)
		{
			return EnemyStates.ZombieGiveUp;
		}

		public override IFSMState<BaseZombie> GetHitState(BaseZombie entity)
		{
			return EnemyStates.ZombieHit;
		}

		public override IFSMState<BaseZombie> GetDieState(BaseZombie entity)
		{
			return EnemyStates.ZombieDie;
		}

		public override IFSMState<BaseZombie> GetDigState(BaseZombie entity)
		{
			return EnemyStates.ZombieDig;
		}

		public override float GetDamageTypeMultiplier(DamageType damageType, bool headShot)
		{
			float result = 1f;
			if ((damageType & DamageType.BLUNT) != (DamageType)0)
			{
				result *= 0.5f;
			}
			else if ((damageType & DamageType.BLADE) != (DamageType)0)
			{
				result *= 0.75f;
			}
			if (headShot)
			{
				result *= 2.5f;
			}
			return result;
		}
	}
}
