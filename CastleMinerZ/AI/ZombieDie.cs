using System;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieDie : EnemyBaseState
	{
		public virtual string GetAnimName(BaseZombie entity)
		{
			return "death" + (entity.Rnd.Next(3) + 1).ToString();
		}

		public override void Enter(BaseZombie entity)
		{
			base.ZeroVelocity(entity);
			entity.IsBlocking = false;
			entity.IsHittable = false;
			entity.CurrentPlayer = entity.PlayClip(this.GetAnimName(entity), false, TimeSpan.FromSeconds(0.25));
			entity.FrustrationCount = 5f;
			entity.CurrentPlayer.Speed = entity.EType.DieAnimationSpeed;
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.CurrentPlayer.Finished)
			{
				entity.FrustrationCount -= dt;
				if (entity.FrustrationCount < 0f)
				{
					entity.Remove();
				}
			}
		}
	}
}
