using System;
using DNA.Audio;
using DNA.CastleMinerZ.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieChase : EnemyBaseState
	{
		public override bool IsRestartable()
		{
			return true;
		}

		protected virtual void StartMoveAnimation(BaseZombie entity)
		{
			if (entity.CurrentSpeed < 2.7f)
			{
				entity.CurrentPlayer = entity.PlayClip("walk", true, TimeSpan.FromSeconds(0.25));
				entity.CurrentPlayer.Speed = Math.Min(entity.CurrentSpeed / this._walkSpeed, 1f);
				return;
			}
			if (entity.CurrentSpeed < 3.7f)
			{
				entity.CurrentPlayer = entity.PlayClip("walk2", true, TimeSpan.FromSeconds(0.25));
				entity.CurrentPlayer.Speed = Math.Min(entity.CurrentSpeed / this._walkSpeed, 1f);
				return;
			}
			if (entity.CurrentSpeed < 5f || !entity.EType.HasRunFast)
			{
				entity.CurrentPlayer = entity.PlayClip("run", true, TimeSpan.FromSeconds(0.25));
				entity.CurrentPlayer.Speed = Math.Min(entity.CurrentSpeed / this._runSpeed, 1f);
				return;
			}
			entity.CurrentPlayer = entity.PlayClip("run_fast", true, TimeSpan.FromSeconds(0.25));
			entity.CurrentPlayer.Speed = Math.Min(entity.CurrentSpeed / this._runFastSpeed, 1f);
		}

		public override void Enter(BaseZombie entity)
		{
			entity.IsBlocking = true;
			entity.IsHittable = true;
			this.StartMoveAnimation(entity);
			entity.ResetFrustration();
		}

		public override void HandleSpeedUp(BaseZombie entity)
		{
			this.StartMoveAnimation(entity);
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.ZombieGrowlCue == null || !entity.ZombieGrowlCue.IsPlaying)
			{
				if (entity.EType.FoundIn == EnemyType.FoundInEnum.ABOVEGROUND)
				{
					entity.ZombieGrowlCue = SoundManager.Instance.PlayInstance("ZombieGrowl", entity.SoundEmitter);
				}
				else if (entity.EType.FoundIn == EnemyType.FoundInEnum.CAVES)
				{
					entity.ZombieGrowlCue = SoundManager.Instance.PlayInstance("Skeleton", entity.SoundEmitter);
				}
				else if (entity.EType.FoundIn == EnemyType.FoundInEnum.CRASHSITE)
				{
					entity.ZombieGrowlCue = SoundManager.Instance.PlayInstance("Alien", entity.SoundEmitter);
				}
				else
				{
					entity.ZombieGrowlCue = SoundManager.Instance.PlayInstance("Felguard", entity.SoundEmitter);
				}
			}
			Vector3 newVel = entity.PlayerPhysics.WorldVelocity;
			newVel.X = 0f;
			newVel.Z = 0f;
			Vector3 targetPos = entity.Target.WorldPosition - entity.WorldPosition;
			float elevationDifference = targetPos.Y;
			targetPos.Y = 0f;
			float d = targetPos.Length();
			if (d < 5f)
			{
				float hitTime = entity.TimeToIntercept();
				if (hitTime < 1f / entity.CurrentSpeed)
				{
					if (Math.Abs(elevationDifference) > 4f && entity.OnGround && entity.Target.InContact)
					{
						entity.StateMachine.ChangeState(entity.EType.GetDigState(entity));
						return;
					}
					entity.StateMachine.ChangeState(entity.EType.GetAttackState(entity));
					return;
				}
			}
			if (entity.FrustrationCount <= 0f)
			{
				entity.StateMachine.ChangeState(entity.EType.GetDigState(entity));
				entity.PlayerPhysics.WorldVelocity = newVel;
				return;
			}
			if (entity.EType.HasRunFast && !entity.IsMovingFast && entity.Target.IsLocal)
			{
				entity.TimeLeftTilFast -= dt;
				if (entity.TimeLeftTilFast <= 0f || EnemyManager.Instance.ZombieFestIsOn)
				{
					SpeedUpEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, entity.EnemyID, (int)entity.Target.Gamer.Id);
				}
				Vector3 spd = entity.Target.PlayerPhysics.WorldVelocity;
				spd.Y = 0f;
				if (spd.LengthSquared() > 3.5f)
				{
					entity.TimeLeftTilRunFast -= dt;
					if (entity.TimeLeftTilRunFast < 0f)
					{
						SpeedUpEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, entity.EnemyID, (int)entity.Target.Gamer.Id);
					}
				}
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
				speed *= (entity.IsMovingFast ? 1f : 0.5f);
			}
			else if (entity.TouchingWall)
			{
				newVel.Y += (entity.IsMovingFast ? entity.EType.FastJumpSpeed : 10f);
			}
			newVel.X = targetPos.X * speed;
			newVel.Z = targetPos.Z * speed;
			entity.PlayerPhysics.WorldVelocity = newVel;
			newVel.Y = 0f;
			if (newVel.LengthSquared() > 0.2f)
			{
				float heading = (float)Math.Atan2((double)(-(double)newVel.Z), (double)newVel.X) + 1.5707964f;
				entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, heading), 0f, 0f);
			}
			if (d >= 5f)
			{
				if (entity.IsMovingFast)
				{
					float hitTime2 = entity.TimeToIntercept();
					if (hitTime2 > 8f)
					{
						entity.StateMachine.ChangeState(entity.EType.GetDigState(entity));
						entity.PlayerPhysics.WorldVelocity = newVel;
						return;
					}
				}
				else if (d > (float)entity.PlayerDistanceLimit)
				{
					entity.StateMachine.ChangeState(entity.EType.GetDigState(entity));
					entity.PlayerPhysics.WorldVelocity = newVel;
				}
			}
		}

		protected float _runSpeed = 3f;

		private float _runFastSpeed = 4f;

		protected float _walkSpeed = 1f;
	}
}
