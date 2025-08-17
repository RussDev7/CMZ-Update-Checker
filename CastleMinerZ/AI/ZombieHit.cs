using System;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieHit : EnemyBaseState
	{
		public virtual string GetAnimName(BaseZombie entity)
		{
			if (entity.Rnd.Next(2) == 0)
			{
				return "hit_reaction1";
			}
			return "hit_reaction3";
		}

		public override void Enter(BaseZombie entity)
		{
			base.ZeroVelocity(entity);
			entity.CurrentPlayer = entity.PlayClip(this.GetAnimName(entity), false, TimeSpan.FromSeconds(0.25));
			entity.CurrentPlayer.Speed = entity.EType.HitAnimationSpeed;
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.IsNearAnimationEnd)
			{
				base.Restart(entity);
			}
		}
	}
}
