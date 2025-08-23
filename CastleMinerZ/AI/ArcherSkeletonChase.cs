using System;
using DNA.Audio;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class ArcherSkeletonChase : EnemyBaseState
	{
		public override bool IsRestartable()
		{
			return true;
		}

		public override void Enter(BaseZombie entity)
		{
			entity.IsBlocking = true;
			entity.IsHittable = true;
			entity.CurrentPlayer = entity.PlayClip("walk_archer1", true, TimeSpan.FromSeconds(0.25));
			entity.CurrentPlayer.Speed = entity.CurrentSpeed / 1f;
			entity.ResetFrustration();
			entity.StateTimer = 0f;
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.ZombieGrowlCue == null || !entity.ZombieGrowlCue.IsPlaying)
			{
				entity.ZombieGrowlCue = SoundManager.Instance.PlayInstance("Skeleton", entity.SoundEmitter);
			}
			entity.StateTimer -= dt;
			Vector3 newVel = entity.PlayerPhysics.WorldVelocity;
			newVel.X = 0f;
			newVel.Z = 0f;
			Vector3 targetPos = entity.Target.WorldPosition - entity.WorldPosition;
			targetPos.Y = 0f;
			float d = targetPos.Length();
			if (d < 35f && entity.StateTimer <= 0f)
			{
				entity.StateTimer = 0.5f;
				Vector3 pfrom = entity.WorldPosition;
				Vector3 pto = entity.Target.WorldPosition;
				pfrom.Y += 1.5f;
				pto.Y += 1.5f;
				ArcherSkeletonChase.tp.Init(pfrom, pto);
				BlockTerrain.Instance.Trace(ArcherSkeletonChase.tp);
				if (!ArcherSkeletonChase.tp._collides)
				{
					if (targetPos.LengthSquared() > 0.2f)
					{
						float heading = (float)Math.Atan2((double)(-(double)targetPos.Z), (double)targetPos.X) + 1.5707964f;
						entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, heading), 0f, 0f);
					}
					entity.StateMachine.ChangeState(entity.EType.GetAttackState(entity));
					return;
				}
			}
			if (entity.FrustrationCount < 0f)
			{
				entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
				entity.PlayerPhysics.WorldVelocity = newVel;
				return;
			}
			if (targetPos.LengthSquared() < 0.001f)
			{
				targetPos = Vector3.Zero;
			}
			else
			{
				targetPos.Normalize();
			}
			float speed = entity.CurrentSpeed;
			if (!entity.OnGround)
			{
				speed *= 0.5f;
			}
			else if (entity.TouchingWall)
			{
				newVel.Y += 10f;
			}
			newVel.X = targetPos.X * speed;
			newVel.Z = targetPos.Z * speed;
			entity.PlayerPhysics.WorldVelocity = newVel;
			newVel.Y = 0f;
			if (newVel.LengthSquared() > 0.2f)
			{
				float heading2 = (float)Math.Atan2((double)(-(double)newVel.Z), (double)newVel.X) + 1.5707964f;
				entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, heading2), 0f, 0f);
			}
		}

		private static TraceProbe tp = new TraceProbe();
	}
}
