using System;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public struct BaseDragonWaypoint
	{
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.Position);
			writer.Write(this.Velocity);
			writer.Write(this.HostTime);
			writer.Write(this.TargetRoll);
			writer.Write((byte)this.Animation);
			writer.Write((byte)this.Sound);
		}

		public static BaseDragonWaypoint ReadBaseWaypoint(BinaryReader reader)
		{
			BaseDragonWaypoint baseDragonWaypoint;
			baseDragonWaypoint.Position = reader.ReadVector3();
			baseDragonWaypoint.Velocity = reader.ReadVector3();
			baseDragonWaypoint.HostTime = reader.ReadSingle();
			baseDragonWaypoint.TargetRoll = reader.ReadSingle();
			baseDragonWaypoint.Animation = (DragonAnimEnum)reader.ReadByte();
			baseDragonWaypoint.Sound = (DragonSoundEnum)reader.ReadByte();
			return baseDragonWaypoint;
		}

		public static void InterpolatePositionVelocity(float time, BaseDragonWaypoint wpt1, BaseDragonWaypoint wpt2, out Vector3 outpos, out Vector3 outvel)
		{
			float num = wpt2.HostTime - wpt1.HostTime;
			if (num == 0f || time >= wpt2.HostTime)
			{
				BaseDragonWaypoint.Extrapolate(time, wpt2, out outpos, out outvel);
				return;
			}
			if (time <= wpt1.HostTime)
			{
				BaseDragonWaypoint.Extrapolate(time, wpt1, out outpos, out outvel);
				return;
			}
			float num2 = (time - wpt1.HostTime) / num;
			float num3 = 1f / num;
			Vector3 vector = wpt1.Velocity * num;
			Vector3 vector2 = wpt2.Velocity * num;
			outpos = Vector3.Hermite(wpt1.Position, vector, wpt2.Position, vector2, num2);
			outvel = MathTools.Hermite1stDerivative(wpt1.Position, vector, wpt2.Position, vector2, num2) * num3;
		}

		public static void Extrapolate(float time, BaseDragonWaypoint wpt, out Vector3 outpos, out Vector3 outvel)
		{
			outpos = wpt.Position + wpt.Velocity * (time - wpt.HostTime);
			outvel = wpt.Velocity;
		}

		public float HostTime;

		public DragonAnimEnum Animation;

		public float TargetRoll;

		public DragonSoundEnum Sound;

		public Vector3 Position;

		public Vector3 Velocity;
	}
}
