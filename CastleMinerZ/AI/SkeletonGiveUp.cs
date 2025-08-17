using System;
using DNA.CastleMinerZ.Net;

namespace DNA.CastleMinerZ.AI
{
	public class SkeletonGiveUp : ZombieGiveUp
	{
		public override void Enter(BaseZombie entity)
		{
			base.ZeroVelocity(entity);
			entity.CurrentPlayer = entity.PlayClip("enraged", false, TimeSpan.FromSeconds(0.25));
			if (entity.Target != null && entity.Target.IsLocal && entity.Target.Gamer != null)
			{
				EnemyGiveUpMessage.Send(entity.EnemyID, (int)entity.Target.Gamer.Id);
			}
		}
	}
}
