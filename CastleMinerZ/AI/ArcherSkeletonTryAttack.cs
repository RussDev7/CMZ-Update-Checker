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
				Vector3 vector = entity.Target.WorldPosition - entity.WorldPosition;
				vector.Y = 0f;
				if (vector.LengthSquared() > 0.2f)
				{
					float num = (float)Math.Atan2((double)(-(double)vector.Z), (double)vector.X) + 1.5707964f;
					entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, num), 0f, 0f);
				}
				if (entity.CurrentPlayer.CurrentTime.TotalSeconds > 1.100000023841858 && entity.HitCount == 0)
				{
					entity.HitCount = 1;
					Vector3 worldPosition = entity.WorldPosition;
					Vector3 worldPosition2 = entity.Target.WorldPosition;
					worldPosition.Y += 1.5f;
					worldPosition2.Y += 0.5f;
					Vector3 vector2;
					if (!MathTools.CalculateInitialBallisticVector(worldPosition, worldPosition2, 25f, out vector2, -10f))
					{
						vector2 = entity.Target.WorldPosition - entity.WorldPosition;
						if (vector2.LengthSquared() < 0.001f)
						{
							vector2 = Vector3.Up;
						}
						else
						{
							vector2.Normalize();
						}
						vector2 *= 25f;
					}
					Vector3 worldPosition3 = entity.WorldPosition;
					worldPosition3.Y += 1.5f;
					TracerManager.Instance.AddArrow(worldPosition3, vector2, entity.Target);
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
