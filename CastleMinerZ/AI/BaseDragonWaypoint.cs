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
			BaseDragonWaypoint result;
			result.Position = reader.ReadVector3();
			result.Velocity = reader.ReadVector3();
			result.HostTime = reader.ReadSingle();
			result.TargetRoll = reader.ReadSingle();
			result.Animation = (DragonAnimEnum)reader.ReadByte();
			result.Sound = (DragonSoundEnum)reader.ReadByte();
			return result;
		}

		public static void InterpolatePositionVelocity(float time, BaseDragonWaypoint wpt1, BaseDragonWaypoint wpt2, out Vector3 outpos, out Vector3 outvel)
		{
			float dt = wpt2.HostTime - wpt1.HostTime;
			if (dt == 0f || time >= wpt2.HostTime)
			{
				BaseDragonWaypoint.Extrapolate(time, wpt2, out outpos, out outvel);
				return;
			}
			if (time <= wpt1.HostTime)
			{
				BaseDragonWaypoint.Extrapolate(time, wpt1, out outpos, out outvel);
				return;
			}
			float p = (time - wpt1.HostTime) / dt;
			float oodt = 1f / dt;
			Vector3 t = wpt1.Velocity * dt;
			Vector3 t2 = wpt2.Velocity * dt;
			outpos = Vector3.Hermite(wpt1.Position, t, wpt2.Position, t2, p);
			outvel = MathTools.Hermite1stDerivative(wpt1.Position, t, wpt2.Position, t2, p) * oodt;
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
