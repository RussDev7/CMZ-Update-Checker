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
			string[] attackArray = this.AttackArray;
			int num = entity.Rnd.Next(0, attackArray.Length);
			entity.AnimationIndex = num;
			return attackArray[num];
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
					float[] array = this.HitTimes[entity.AnimationIndex];
					if (entity.CurrentPlayer.CurrentTime.TotalSeconds >= (double)(array[entity.HitCount] / entity.CurrentPlayer.Speed))
					{
						Vector3 vector = entity.Target.WorldPosition - entity.WorldPosition;
						if (Math.Abs(vector.Y) < 1.2f)
						{
							bool flag = false;
							vector.Y = 0f;
							float num = vector.LengthSquared();
							if ((double)num < 0.05)
							{
								flag = true;
							}
							else
							{
								float num2 = this.HitRanges[entity.AnimationIndex];
								if (num < num2 * num2)
								{
									vector.Normalize();
									if (Vector3.Dot(vector, Vector3.Normalize(entity.LocalToWorld.Forward)) * this.HitDotMultiplier > 0.7f)
									{
										flag = true;
									}
								}
							}
							if (flag)
							{
								InGameHUD.Instance.ApplyDamage(this.HitDamages[entity.AnimationIndex], entity.WorldPosition);
							}
						}
						entity.HitCount++;
						if (entity.HitCount == array.Length)
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
			Vector3 vector2 = entity.Target.WorldPosition - entity.WorldPosition;
			float y = vector2.Y;
			vector2.Y = 0f;
			if (entity.MissCount <= 0)
			{
				if (Math.Abs(y) > 1.5f && entity.Target.InContact && entity.OnGround)
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
				float num3 = vector2.Length();
				if (num3 >= 1f)
				{
					entity.StateMachine.ChangeState(entity.EType.GetChaseState(entity));
					return;
				}
				if (num3 > 0.1f)
				{
					float num4 = (float)Math.Atan2((double)(-(double)vector2.Z), (double)vector2.X) + 1.5707964f;
					entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, num4), 0f, 0f);
				}
				entity.CurrentPlayer = entity.PlayClip(this.GetRandomAttack(entity), false, TimeSpan.FromSeconds(0.25));
				entity.HitCount = 0;
				entity.SwingCount = 2 + entity.Rnd.Next(0, 3);
				return;
			}
		}
	}
}
