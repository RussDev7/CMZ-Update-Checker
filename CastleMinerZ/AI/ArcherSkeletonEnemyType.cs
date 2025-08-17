using System;

namespace DNA.CastleMinerZ.AI
{
	public class ArcherSkeletonEnemyType : SkeletonEnemyType
	{
		public ArcherSkeletonEnemyType(EnemyTypeEnum t, EnemyType.ModelNameEnum model, EnemyType.TextureNameEnum tname, EnemyType.FoundInEnum foundin, SkeletonClassEnum skelclass)
			: base(t, model, tname, foundin, skelclass)
		{
			this.SpawnRadius = 40;
		}

		public override IFSMState<BaseZombie> GetEmergeState(BaseZombie entity)
		{
			return EnemyStates.ArcherEmerge;
		}

		public override IFSMState<BaseZombie> GetChaseState(BaseZombie entity)
		{
			return EnemyStates.ArcherChase;
		}

		public override IFSMState<BaseZombie> GetAttackState(BaseZombie entity)
		{
			return EnemyStates.ArcherAttack;
		}
	}
}
