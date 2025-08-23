using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class ArcherSkeletonTryAttack : EnemyBaseState
	{
		public override void Enter(BaseZombie entity)
		{
			base.ZeroVelocity(entity);
			entity.HitCount = 0;
			entity.CurrentPlayer = entity.PlayClip("atack_archer1", false, TimeSpan.FromSeconds(0.25));
			entity.SwingCount--;
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (!entity.IsNearAnimationEnd)
			{
				Vector3 targetPos = entity.Target.WorldPosition - entity.WorldPosition;
				targetPos.Y = 0f;
				if (targetPos.LengthSquared() > 0.2f)
				{
					float heading = (float)Math.Atan2((double)(-(double)targetPos.Z), (double)targetPos.X) + 1.5707964f;
					entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, heading), 0f, 0f);
				}
				if (entity.CurrentPlayer.CurrentTime.TotalSeconds > 1.100000023841858 && entity.HitCount == 0)
				{
					entity.HitCount = 1;
					Vector3 pos = entity.WorldPosition;
					Vector3 pos2 = entity.Target.WorldPosition;
					pos.Y += 1.5f;
					pos2.Y += 0.5f;
					Vector3 vel;
					if (!MathTools.CalculateInitialBallisticVector(pos, pos2, 25f, out vel, -10f))
					{
						vel = entity.Target.WorldPosition - entity.WorldPosition;
						if (vel.LengthSquared() < 0.001f)
						{
							vel = Vector3.Up;
						}
						else
						{
							vel.Normalize();
						}
						vel *= 25f;
					}
					Vector3 pos3 = entity.WorldPosition;
					pos3.Y += 1.5f;
					TracerManager.Instance.AddArrow(pos3, vel, entity.Target);
				}
				return;
			}
			if (entity.SwingCount <= 0)
			{
				entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
				return;
			}
			entity.StateMachine.ChangeState(EnemyStates.ArcherIdle);
		}
	}
}
