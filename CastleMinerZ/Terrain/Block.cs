using System;

namespace DNA.CastleMinerZ.Terrain
{
	public struct Block
	{
		public static int MinLightLevel { get; set; }

		public static int ClearLighting(int data)
		{
			return data &= -512;
		}

		public static bool IsLit(int data)
		{
			return (data & 255) != 0;
		}

		public static bool NeedToLightNewNeighbors(int data)
		{
			return Block.IsLit(data) && (data & -2147482624) == 0;
		}

		public static bool IsUninitialized(int data)
		{
			return (data & int.MinValue) != 0;
		}

		public static int IsUninitialized(int data, bool value)
		{
			if (value)
			{
				return data | int.MinValue;
			}
			return data & int.MaxValue;
		}

		public static int InitAsSky(int data, int light)
		{
			data &= -16;
			return data | 256 | light;
		}

		public static int GetLighting(int data)
		{
			return data & 511;
		}

		public static bool IsSky(int data)
		{
			return (data & 256) != 0;
		}

		public static int IsSky(int data, bool value)
		{
			if (!value)
			{
				return data & -257;
			}
			return data | 256;
		}

		public static bool IsOpaque(int data)
		{
			return (data & 512) != 0;
		}

		public static int IsOpaque(int data, bool value)
		{
			if (!value)
			{
				return data & -513;
			}
			return data | 512;
		}

		public static bool HasAlpha(int data)
		{
			return (data & 2048) != 0;
		}

		public static int HasAlpha(int data, bool value)
		{
			if (!value)
			{
				return data & -2049;
			}
			return data | 2048;
		}

		public static bool IsInList(int data)
		{
			return (data & 1024) != 0;
		}

		public static int IsInList(int data, bool value)
		{
			if (!value)
			{
				return data & -1025;
			}
			return data | 1024;
		}

		public static int GetSunLightLevel(int data)
		{
			return data & 15;
		}

		public static int SetSunLightLevel(int data, int value)
		{
			return (data & -16) | value;
		}

		public static int GetTorchLightLevel(int data)
		{
			return (data & 240) >> 4;
		}

		public static int SetTorchLightLevel(int data, int value)
		{
			return (data & -241) | (value << 4);
		}

		public static BlockTypeEnum GetTypeIndex(int data)
		{
			uint num = (uint)(data & 2147479552) >> 12;
			if (num > 94U)
			{
				return BlockTypeEnum.Dirt;
			}
			return (BlockTypeEnum)num;
		}

		public static BlockType GetType(int data)
		{
			return BlockType.GetType(Block.GetTypeIndex(data));
		}

		public static int SetType(int data, BlockTypeEnum value)
		{
			uint num = (uint)value;
			if (num > 94U)
			{
				num = 1U;
			}
			int num2 = (data & -2147479553) | (int)((int)num << 12);
			BlockType type = BlockType.GetType((BlockTypeEnum)num);
			if (type.Opaque)
			{
				num2 |= 512;
			}
			else
			{
				num2 &= -513;
			}
			if (type.HasAlpha)
			{
				num2 |= 2048;
			}
			else
			{
				num2 &= -2049;
			}
			return num2;
		}

		public const int NUM_BITS_IN_LIGHT_LEVEL = 4;

		public const int SUNLIGHT_NUM_BITS = 4;

		public const int TORCHLIGHT_NUM_BITS = 4;

		public const int SKY_NUM_BITS = 1;

		public const int OPAQUE_NUM_BITS = 1;

		public const int IN_LIST_NUM_BITS = 1;

		public const int HAS_ALPHA_NUM_BITS = 1;

		public const int UNINITIALIZED_NUM_BITS = 1;

		public const int BLOCKTYPE_NUM_BITS = 19;

		public const int SUNLIGHT_SHIFT = 0;

		public const int TORCHLIGHT_SHIFT = 4;

		public const int SKY_SHIFT = 8;

		public const int OPAQUE_SHIFT = 9;

		public const int IN_LIST_SHIFT = 10;

		public const int HAS_ALPHA_SHIFT = 11;

		public const int BLOCKTYPE_SHIFT = 12;

		public const int UNINITIALIZED_SHIFT = 31;

		public const int SUNLIGHT_MASK = 15;

		public const int TORCHLIGHT_MASK = 240;

		public const int SKY_MASK = 256;

		public const int OPAQUE_MASK = 512;

		public const int IN_LIST_MASK = 1024;

		public const int HAS_ALPHA_MASK = 2048;

		public const int BLOCKTYPE_MASK = 2147479552;

		public const int UNINITIALIZED_MASK = -2147483648;

		public const int MAXLIGHTLEVEL = 15;

		public static readonly BlockFace[] OpposingFace = new BlockFace[]
		{
			BlockFace.NEGX,
			BlockFace.POSZ,
			BlockFace.POSX,
			BlockFace.NEGZ,
			BlockFace.NEGY,
			BlockFace.POSY
		};
	}
}
