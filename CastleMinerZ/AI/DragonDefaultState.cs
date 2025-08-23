using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonDefaultState : DragonBaseState
	{
		public override void Enter(DragonEntity entity)
		{
			entity.TimeLeftBeforeNextViewCheck = entity.EType.SlowViewCheckInterval;
			entity.TargetVelocity = entity.EType.Speed;
			Vector3 pos = entity.WorldPosition;
			Vector3 yawVector = DragonBaseState.MakeYawVector(entity.DefaultHeading);
			float spawnDistance = entity.EType.SpawnDistance;
			if (entity.FirstTimeForDefaultState)
			{
				spawnDistance *= 2f;
			}
			entity.FirstTimeForDefaultState = false;
			entity.Target = null;
			entity.TravelTarget = pos + yawVector * spawnDistance;
			entity.TargetYaw = entity.DefaultHeading;
			entity.TargetAltitude = entity.EType.CruisingAltitude;
			entity.HadTargetThisPass = false;
			entity.LoitersLeft = 3;
			entity.ChancesToNotAttack = entity.EType.ChancesToNotAttack;
		}

		public override void Update(DragonEntity entity, float dt)
		{
			if (entity.Removed)
			{
				return;
			}
			if (DragonBaseState.SearchForNewTarget(entity, dt))
			{
				entity.StateMachine.ChangeState(DragonBaseState.GetNextAttackType(entity));
				entity.StateMachine.Update(dt);
				return;
			}
			Vector3 vtotarget;
			float dist = DragonBaseState.SteerTowardTarget(entity, out vtotarget);
			if (dist < 10f)
			{
				entity.Removed = true;
				EnemyManager.Instance.RemoveDragon();
			}
		}
	}
}
