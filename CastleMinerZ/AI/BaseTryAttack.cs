using System;
using DNA.CastleMinerZ.UI;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public abstract class BaseTryAttack : EnemyBaseState
	{
		protected abstract string[] AttackArray { get; }

		protected abstract string RageAnimation { get; }

		protected abstract float[][] HitTimes { get; }

		protected abstract float[] HitDamages { get; }

		protected abstract float[] HitRanges { get; }

		protected abstract float HitDotMultiplier { get; }

		public string GetRandomAttack(BaseZombie entity)
		{
			string[] a = this.AttackArray;
			int i = entity.Rnd.Next(0, a.Length);
			entity.AnimationIndex = i;
			return a[i];
		}

		public override void Enter(BaseZombie entity)
		{
			if (entity.OnGround)
			{
				base.ZeroVelocity(entity);
			}
			entity.CurrentPlayer = entity.PlayClip(this.GetRandomAttack(entity), false, TimeSpan.FromSeconds(0.25));
			entity.SwingCount = 2 + entity.Rnd.Next(0, 3);
			entity.HitCount = 0;
			entity.MissCount = entity.Rnd.Next(3, 5);
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
				if (entity.Target.IsLocal && entity.AnimationIndex != -1)
				{
					float[] hittimes = this.HitTimes[entity.AnimationIndex];
					if (entity.CurrentPlayer.CurrentTime.TotalSeconds >= (double)(hittimes[entity.HitCount] / entity.CurrentPlayer.Speed))
					{
						Vector3 targetPos = entity.Target.WorldPosition - entity.WorldPosition;
						if (Math.Abs(targetPos.Y) < 1.2f)
						{
							bool takeDamage = false;
							targetPos.Y = 0f;
							float lsq = targetPos.LengthSquared();
							if ((double)lsq < 0.05)
							{
								takeDamage = true;
							}
							else
							{
								float range = this.HitRanges[entity.AnimationIndex];
								if (lsq < range * range)
								{
									targetPos.Normalize();
									if (Vector3.Dot(targetPos, Vector3.Normalize(entity.LocalToWorld.Forward)) * this.HitDotMultiplier > 0.7f)
									{
										takeDamage = true;
									}
								}
							}
							if (takeDamage)
							{
								InGameHUD.Instance.ApplyDamage(this.HitDamages[entity.AnimationIndex], entity.WorldPosition);
							}
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
			if (entity.HitCount == 0)
			{
				entity.MissCount--;
			}
			Vector3 targetPos2 = entity.Target.WorldPosition - entity.WorldPosition;
			float elevationDifference = targetPos2.Y;
			targetPos2.Y = 0f;
			if (entity.MissCount <= 0)
			{
				if (Math.Abs(elevationDifference) > 1.5f && entity.Target.InContact && entity.OnGround)
				{
					entity.StateMachine.ChangeState(entity.EType.GetDigState(entity));
					return;
				}
				entity.MissCount = entity.Rnd.Next(1, 3);
				entity.HitCount = 1;
				entity.AnimationIndex = -1;
				entity.CurrentPlayer = entity.PlayClip(this.RageAnimation, false, TimeSpan.FromSeconds(0.25));
				return;
			}
			else
			{
				float d = targetPos2.Length();
				if (d >= 1f)
				{
					entity.StateMachine.ChangeState(entity.EType.GetChaseState(entity));
					return;
				}
				if (d > 0.1f)
				{
					float heading = (float)Math.Atan2((double)(-(double)targetPos2.Z), (double)targetPos2.X) + 1.5707964f;
					entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, heading), 0f, 0f);
				}
				entity.CurrentPlayer = entity.PlayClip(this.GetRandomAttack(entity), false, TimeSpan.FromSeconds(0.25));
				entity.HitCount = 0;
				entity.SwingCount = 2 + entity.Rnd.Next(0, 3);
				return;
			}
		}
	}
}
