using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonStrafeAttackState : DragonBaseState
	{
		public override void Enter(DragonEntity entity)
		{
			entity.TimeLeftBeforeNextViewCheck = entity.EType.FastViewCheckInterval;
			entity.TargetAltitude = entity.EType.HuntingAltitude;
			entity.TimeLeftBeforeNextShot = entity.EType.StrafeFireRate;
			entity.TargetVelocity = entity.EType.Speed;
		}

		public override void Update(DragonEntity entity, float dt)
		{
			if (entity.Target != null && !entity.Target.ValidLivingGamer)
			{
				entity.Target = null;
				entity.MigrateDragon = false;
				entity.MigrateDragonTo = null;
			}
			if (entity.Target == null)
			{
				DragonBaseState.SearchForNewTarget(entity, dt);
			}
			Vector3 vtotarget;
			float dist = DragonBaseState.SteerTowardTarget(entity, out vtotarget);
			if (dist < entity.EType.BreakOffStrafeDistance)
			{
				entity.StateMachine.ChangeState(DragonStates.Loiter);
				return;
			}
			if (dist > entity.EType.MinAttackDistance)
			{
				Vector3 fwd = entity.LocalToWorld.Forward;
				fwd.Y = 0f;
				fwd.Normalize();
				if (dist < entity.EType.MaxAttackDistance && Vector3.Dot(vtotarget, fwd) / dist > 0.95f)
				{
					entity.ShootTarget = entity.TravelTarget;
					if (!entity.ShotPending)
					{
						entity.TimeLeftBeforeNextShot -= dt;
						if (entity.TimeLeftBeforeNextShot < 0f)
						{
							entity.TimeLeftBeforeNextShot += entity.EType.StrafeFireRate;
							if (entity.ChancesToNotAttack == 0)
							{
								entity.ShotPending = true;
							}
						}
					}
				}
			}
		}
	}
}
