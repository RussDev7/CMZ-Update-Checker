using System;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieDig : ZombieTryAttack
	{
		public override void Enter(BaseZombie entity)
		{
			Vector3 delta = entity.Target.WorldPosition - entity.WorldPosition;
			if (entity.Target.IsLocal && delta.LengthSquared() > 256f)
			{
				entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
				return;
			}
			if (entity.OnGround)
			{
				base.ZeroVelocity(entity);
			}
			entity.CurrentPlayer = entity.PlayClip(base.GetRandomAttack(entity), false, TimeSpan.FromSeconds(0.25));
			entity.HitCount = 0;
			entity.SwingCount = 0;
			entity.MissCount = 0;
			entity.CurrentPlayer.Speed = entity.EType.AttackAnimationSpeed;
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.OnGround)
			{
				base.ZeroVelocity(entity);
			}
			else
			{
				base.ReduceVelocity(entity);
			}
			if (!entity.IsNearAnimationEnd)
			{
				if (entity.AnimationIndex != -1)
				{
					float[] hittimes = this.HitTimes[entity.AnimationIndex];
					if (entity.CurrentPlayer.CurrentTime.TotalSeconds >= (double)(hittimes[entity.HitCount] / entity.CurrentPlayer.Speed))
					{
						Vector3 ourPos = entity.WorldPosition;
						Vector3 theirPos = entity.Target.WorldPosition;
						IntVector3 v = IntVector3.FromVector3(entity.WorldPosition);
						if (theirPos.Y >= ourPos.Y + 1f)
						{
							v.Y++;
						}
						else if (theirPos.Y <= ourPos.Y - 1f)
						{
							v.Y--;
						}
						IntVector3 minRect = v;
						IntVector3 maxRect = v;
						maxRect.Y += 2;
						if (theirPos.X > ourPos.X)
						{
							maxRect.X++;
						}
						else
						{
							minRect.X--;
						}
						if (theirPos.Z >= ourPos.Z)
						{
							maxRect.Z++;
						}
						else
						{
							minRect.Z--;
						}
						entity.SwingCount++;
						bool playDigSound = true;
						int swingCount = (int)((float)entity.SwingCount * entity.EType.DiggingMultiplier);
						switch (Explosive.EnemyBreakBlocks(minRect, maxRect, swingCount, entity.EType.HardestBlockThatCanBeDug, entity.Target.IsLocal))
						{
						case Explosive.EnemyBreakBlocksResult.BlocksWillBreak:
							entity.MissCount |= 1;
							break;
						case Explosive.EnemyBreakBlocksResult.BlocksWillNotBreak:
							entity.MissCount |= 4;
							break;
						case Explosive.EnemyBreakBlocksResult.BlocksBroken:
							entity.MissCount |= 2;
							break;
						case Explosive.EnemyBreakBlocksResult.RegionIsEmpty:
							playDigSound = false;
							break;
						}
						if (playDigSound)
						{
							SoundManager.Instance.PlayInstance("ZombieDig", entity.SoundEmitter);
						}
						entity.HitCount++;
						if (entity.HitCount == hittimes.Length)
						{
							entity.AnimationIndex = -1;
							entity.HitCount = 0;
						}
					}
				}
				return;
			}
			if (entity.MissCount == 4)
			{
				entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
				return;
			}
			if (entity.MissCount == 0 || (entity.MissCount & 2) != 0)
			{
				entity.StateMachine.ChangeState(entity.EType.GetChaseState(entity));
				return;
			}
			Vector3 targetPos = entity.Target.WorldPosition - entity.WorldPosition;
			float elevationDifference = targetPos.Y;
			targetPos.Y = 0f;
			float d = targetPos.Length();
			int distanceThreshold = ((entity.SpawnSource != null) ? 48 : 16);
			int elevationThreshold = ((entity.SpawnSource != null) ? 24 : 8);
			if (d < 1f && Math.Abs(elevationDifference) < 1.5f)
			{
				entity.StateMachine.ChangeState(entity.EType.GetAttackState(entity));
				return;
			}
			if (entity.Target.IsLocal && (d > (float)distanceThreshold || Math.Abs(elevationDifference) > (float)elevationThreshold))
			{
				entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
				return;
			}
			float heading = (float)Math.Atan2((double)(-(double)targetPos.Z), (double)targetPos.X) + 1.5707964f;
			entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, heading), 0f, 0f);
			entity.CurrentPlayer = entity.PlayClip(base.GetRandomAttack(entity), false, TimeSpan.FromSeconds(0.25));
			entity.HitCount = 0;
			entity.MissCount = 0;
		}
	}
}
