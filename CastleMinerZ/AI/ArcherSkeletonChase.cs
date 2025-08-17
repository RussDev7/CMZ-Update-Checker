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
			Vector3 worldVelocity = entity.PlayerPhysics.WorldVelocity;
			worldVelocity.X = 0f;
			worldVelocity.Z = 0f;
			Vector3 vector = entity.Target.WorldPosition - entity.WorldPosition;
			vector.Y = 0f;
			float num = vector.Length();
			if (num < 35f && entity.StateTimer <= 0f)
			{
				entity.StateTimer = 0.5f;
				Vector3 worldPosition = entity.WorldPosition;
				Vector3 worldPosition2 = entity.Target.WorldPosition;
				worldPosition.Y += 1.5f;
				worldPosition2.Y += 1.5f;
				ArcherSkeletonChase.tp.Init(worldPosition, worldPosition2);
				BlockTerrain.Instance.Trace(ArcherSkeletonChase.tp);
				if (!ArcherSkeletonChase.tp._collides)
				{
					if (vector.LengthSquared() > 0.2f)
					{
						float num2 = (float)Math.Atan2((double)(-(double)vector.Z), (double)vector.X) + 1.5707964f;
						entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, num2), 0f, 0f);
					}
					entity.StateMachine.ChangeState(entity.EType.GetAttackState(entity));
					return;
				}
			}
			if (entity.FrustrationCount < 0f)
			{
				entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
				entity.PlayerPhysics.WorldVelocity = worldVelocity;
				return;
			}
			if (vector.LengthSquared() < 0.001f)
			{
				vector = Vector3.Zero;
			}
			else
			{
				vector.Normalize();
			}
			float num3 = entity.CurrentSpeed;
			if (!entity.OnGround)
			{
				num3 *= 0.5f;
			}
			else if (entity.TouchingWall)
			{
				worldVelocity.Y += 10f;
			}
			worldVelocity.X = vector.X * num3;
			worldVelocity.Z = vector.Z * num3;
			entity.PlayerPhysics.WorldVelocity = worldVelocity;
			worldVelocity.Y = 0f;
			if (worldVelocity.LengthSquared() > 0.2f)
			{
				float num4 = (float)Math.Atan2((double)(-(double)worldVelocity.Z), (double)worldVelocity.X) + 1.5707964f;
				entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, num4), 0f, 0f);
			}
		}

		private static TraceProbe tp = new TraceProbe();
	}
}
