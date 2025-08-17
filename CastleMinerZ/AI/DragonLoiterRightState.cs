using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonLoiterRightState : DragonLoiterLeftState
	{
		public override float GetNewYaw(DragonEntity entity, Vector3 dest)
		{
			float num = dest.Length();
			float num2 = DragonBaseState.GetHeading(dest, 0f) - 1.5707964f;
			if (num > entity.EType.LoiterDistance)
			{
				num2 -= Math.Min(1.5f, (num - entity.EType.LoiterDistance) / 30f);
			}
			else
			{
				num2 += Math.Min(1.5f, (entity.EType.LoiterDistance - num) / 20f);
			}
			return MathHelper.WrapAngle(num2);
		}
	}
}
