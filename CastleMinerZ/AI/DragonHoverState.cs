using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonHoverState : DragonBaseState
	{
		public override void Enter(DragonEntity entity)
		{
			entity.ShotsLeft = MathTools.RandomInt(entity.EType.MinHoverShots, entity.EType.MaxHoverShots);
			entity.TimeLeftBeforeNextShot = entity.EType.HoverFireRate + entity.EType.HoverFireRate * MathTools.RandomFloat();
			entity.TargetVelocity = 0.25f;
			entity.NextAnimation = DragonAnimEnum.HOVER;
		}

		public override void Exit(DragonEntity entity)
		{
			entity.TargetVelocity = entity.EType.Speed;
			entity.NextAnimation = DragonAnimEnum.FLAP;
		}

		public override void Update(DragonEntity entity, float dt)
		{
			if (entity.Target != null && !entity.Target.ValidLivingGamer)
			{
				entity.Target = null;
			}
			if (DragonBaseState.DoViewCheck(entity, dt, entity.EType.FastViewCheckInterval) && entity.Target != null)
			{
				Vector3 target = entity.Target.WorldPosition;
				target.Y += 1.5f;
				if (DragonBaseState.CanSeePosition(entity, target))
				{
					entity.TravelTarget = entity.Target.WorldPosition;
				}
			}
			Vector3 pos = entity.WorldPosition;
			entity.TargetAltitude = entity.EType.HuntingAltitude;
			Vector3 dest = entity.TravelTarget - pos;
			entity.TargetYaw = MathHelper.WrapAngle(DragonBaseState.GetHeading(dest, entity.TargetYaw));
			entity.ShootTarget = entity.TravelTarget;
			if (!entity.ShotPending)
			{
				entity.TimeLeftBeforeNextShot -= dt;
				if (entity.TimeLeftBeforeNextShot < 0f)
				{
					if (entity.ShotsLeft == 0)
					{
						entity.StateMachine.ChangeState(DragonStates.Loiter);
						entity.StateMachine.Update(dt);
						return;
					}
					entity.TimeLeftBeforeNextShot += entity.EType.HoverFireRate;
					entity.ShotPending = true;
					entity.ShotsLeft--;
				}
			}
		}
	}
}
