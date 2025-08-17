using System;
using DNA.Audio;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieEmerge : EnemyBaseState
	{
		public virtual string GetClipName(BaseZombie entity)
		{
			return "arise_" + (entity.Rnd.Next(4) + 1).ToString();
		}

		public override void Enter(BaseZombie entity)
		{
			entity.IsBlocking = true;
			entity.IsHittable = false;
			entity.CurrentPlayer = entity.PlayClip(this.GetClipName(entity), false, TimeSpan.Zero);
			entity.CurrentPlayer.Speed = entity.InitPkg.EmergeSpeed;
			entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(base.MakeHeading(entity, (float)entity.Rnd.NextDouble() * 6.2831855f), 0f, 0f);
			if (entity.EType.FoundIn == EnemyType.FoundInEnum.ABOVEGROUND)
			{
				SoundManager.Instance.PlayInstance("CreatureUnearth", entity.SoundEmitter);
				entity.ZombieGrowlCue = SoundManager.Instance.PlayInstance("ZombieCry", entity.SoundEmitter);
			}
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.IsNearAnimationEnd)
			{
				entity.StateMachine.ChangeState(entity.EType.GetChaseState(entity));
			}
		}
	}
}
