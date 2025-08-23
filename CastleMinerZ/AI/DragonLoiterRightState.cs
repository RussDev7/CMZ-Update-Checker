using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonLoiterRightState : DragonLoiterLeftState
	{
		public override float GetNewYaw(DragonEntity entity, Vector3 dest)
		{
			float dist = dest.Length();
			float ty = DragonBaseState.GetHeading(dest, 0f) - 1.5707964f;
			if (dist > entity.EType.LoiterDistance)
			{
				ty -= Math.Min(1.5f, (dist - entity.EType.LoiterDistance) / 30f);
			}
			else
			{
				ty += Math.Min(1.5f, (entity.EType.LoiterDistance - dist) / 20f);
			}
			return MathHelper.WrapAngle(ty);
		}
	}
}
