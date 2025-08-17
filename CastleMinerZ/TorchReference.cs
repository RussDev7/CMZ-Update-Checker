using System;
using System.IO;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public struct TorchReference
	{
		public TorchReference(Vector3 blockCenter, BlockFace facing)
		{
			this.Position = blockCenter;
			this.Facing = facing;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(this.Position.X);
			writer.Write(this.Position.Y);
			writer.Write(this.Position.Z);
			writer.Write((byte)this.Facing);
		}

		public void Read(BinaryReader reader)
		{
			this.Position.X = reader.ReadSingle();
			this.Position.Y = reader.ReadSingle();
			this.Position.Z = reader.ReadSingle();
			this.Facing = (BlockFace)reader.ReadByte();
		}

		public Vector3 Position;

		public BlockFace Facing;
	}
}
