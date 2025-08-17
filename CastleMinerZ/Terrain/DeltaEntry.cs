using System;

namespace DNA.CastleMinerZ.Terrain
{
	public class DeltaEntry
	{
		public static IntVector3 GetVector(int delta)
		{
			return new IntVector3((delta >> 16) & 15, (delta >> 8) & 127, delta & 15);
		}

		public static BlockTypeEnum GetBlockType(int delta)
		{
			return (BlockTypeEnum)((delta >> 24) & 255);
		}

		public static int Create(IntVector3 vec, BlockTypeEnum type)
		{
			return (int)(((int)type << 24) | (BlockTypeEnum)((vec.X & 15) << 16) | (BlockTypeEnum)((vec.Y & 127) << 8) | (BlockTypeEnum)(vec.Z & 15));
		}

		public static bool SameLocation(int delta1, int delta2)
		{
			return ((delta1 ^ delta2) & 1015567) == 0;
		}
	}
}
