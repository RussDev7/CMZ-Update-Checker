using System;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonHostMigrationInfo
	{
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.NextDragonTime);
			writer.Write(this.Roll);
			writer.Write(this.TargetRoll);
			writer.Write(this.Pitch);
			writer.Write(this.TargetPitch);
			writer.Write(this.Yaw);
			writer.Write(this.TargetYaw);
			writer.Write(this.Velocity);
			writer.Write(this.TargetVelocity);
			writer.Write(this.DefaultHeading);
			writer.Write(this.FlapDebt);
			writer.Write(this.NextFireballIndex);
			writer.Write(this.ForBiome);
			writer.Write(this.NextUpdateTime);
			writer.Write((byte)this.Animation);
			writer.Write((byte)this.EType);
			writer.Write(this.Position);
			writer.Write(this.Target);
		}

		public static DragonHostMigrationInfo Read(BinaryReader reader)
		{
			return new DragonHostMigrationInfo
			{
				NextDragonTime = reader.ReadSingle(),
				Roll = reader.ReadSingle(),
				TargetRoll = reader.ReadSingle(),
				Pitch = reader.ReadSingle(),
				TargetPitch = reader.ReadSingle(),
				Yaw = reader.ReadSingle(),
				TargetYaw = reader.ReadSingle(),
				Velocity = reader.ReadSingle(),
				TargetVelocity = reader.ReadSingle(),
				DefaultHeading = reader.ReadSingle(),
				FlapDebt = reader.ReadSingle(),
				NextFireballIndex = reader.ReadInt32(),
				ForBiome = reader.ReadBoolean(),
				NextUpdateTime = reader.ReadSingle(),
				Animation = (DragonAnimEnum)reader.ReadByte(),
				EType = (DragonTypeEnum)reader.ReadByte(),
				Position = reader.ReadVector3(),
				Target = reader.ReadVector3()
			};
		}

		public float NextDragonTime;

		public float Roll;

		public float TargetRoll;

		public float Pitch;

		public float TargetPitch;

		public float Yaw;

		public float TargetYaw;

		public float Velocity;

		public float TargetVelocity;

		public float DefaultHeading;

		public float FlapDebt;

		public float NextUpdateTime;

		public int NextFireballIndex;

		public bool ForBiome;

		public DragonTypeEnum EType;

		public DragonAnimEnum Animation;

		public Vector3 Position;

		public Vector3 Target;
	}
}
