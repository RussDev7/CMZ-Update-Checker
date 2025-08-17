using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonLoiterLeftState : DragonBaseState
	{
		public override void Enter(DragonEntity entity)
		{
			entity.LoiterTimer = MathHelper.Lerp(entity.EType.MinLoiterTime, entity.EType.MaxLoiterTime, MathTools.RandomFloat());
			if (entity.ChancesToNotAttack != 0)
			{
				entity.LoiterTimer *= 1.5f;
			}
		}

		public virtual float GetNewYaw(DragonEntity entity, Vector3 dest)
		{
			float num = dest.Length();
			float num2 = DragonBaseState.GetHeading(dest, 0f) + 1.5707964f;
			if (num > entity.EType.LoiterDistance)
			{
				num2 += Math.Min(1.5f, (num - entity.EType.LoiterDistance) / 30f);
			}
			else
			{
				num2 -= Math.Min(1.5f, (entity.EType.LoiterDistance - num) / 20f);
			}
			return MathHelper.WrapAngle(num2);
		}

		public override void Update(DragonEntity entity, float dt)
		{
			if (entity.Target == null && DragonBaseState.SearchForNewTarget(entity, dt))
			{
				entity.StateMachine.ChangeState(DragonBaseState.GetNextAttackType(entity));
				entity.StateMachine.Update(dt);
				return;
			}
			Vector3 vector = entity.WorldPosition - entity.TravelTarget;
			vector.Y = 0f;
			if (entity.LoiterTimer != 3.4028235E+38f)
			{
				entity.LoiterTimer -= dt;
				if (entity.LoiterTimer < 0f)
				{
					entity.StateMachine.ChangeState(DragonBaseState.GetNextAttackType(entity));
					entity.StateMachine.Update(dt);
					return;
				}
			}
			entity.TargetYaw = this.GetNewYaw(entity, vector);
		}
	}
}
