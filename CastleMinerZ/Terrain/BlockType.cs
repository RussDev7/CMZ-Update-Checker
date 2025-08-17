using System;
using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ.Terrain
{
	public class BlockType
	{
		static BlockType()
		{
			BlockType._blockTypes[35].AllowSlopes = false;
			BlockType._blockTypes[34].AllowSlopes = false;
			BlockType._blockTypes[36].AllowSlopes = false;
			BlockType._blockTypes[37].AllowSlopes = false;
			BlockType._blockTypes[61].AllowSlopes = false;
			BlockType._blockTypes[60].AllowSlopes = false;
			BlockType._blockTypes[62].AllowSlopes = false;
			BlockType._blockTypes[63].AllowSlopes = false;
		}

		public static bool IsEmpty(BlockTypeEnum btype)
		{
			return btype == BlockTypeEnum.NumberOfBlocks || btype == BlockTypeEnum.Empty;
		}

		public override string ToString()
		{
			return this.Name;
		}

		public static BlockType GetType(BlockTypeEnum t)
		{
			return BlockType._blockTypes[(int)t];
		}

		public static BlockTypeEnum GetBlockEnumType(int block)
		{
			return (BlockTypeEnum)block;
		}

		public static bool IsContainer(BlockTypeEnum blockType)
		{
			return blockType == BlockTypeEnum.Crate || blockType == BlockTypeEnum.CrateStone || blockType == BlockTypeEnum.CrateIron || blockType == BlockTypeEnum.CrateCopper || blockType == BlockTypeEnum.CrateGold || blockType == BlockTypeEnum.CrateDiamond || blockType == BlockTypeEnum.CrateBloodstone;
		}

		public static bool IsDoor(BlockTypeEnum blockType)
		{
			return BlockType.IsUpperDoor(blockType) || BlockType.IsLowerDoor(blockType) || blockType == BlockTypeEnum.NormalLowerDoor || blockType == BlockTypeEnum.StrongLowerDoor;
		}

		internal static bool IsSpawnerClickable(BlockTypeEnum blockType)
		{
			return blockType == BlockTypeEnum.EnemySpawnOff || blockType == BlockTypeEnum.EnemySpawnRareOff || blockType == BlockTypeEnum.AlienSpawnOff || blockType == BlockTypeEnum.HellSpawnOff || blockType == BlockTypeEnum.BossSpawnOff;
		}

		internal static bool ShouldDropLoot(BlockTypeEnum blockType)
		{
			return blockType == BlockTypeEnum.LootBlock || blockType == BlockTypeEnum.LuckyLootBlock;
		}

		internal static bool IsUpperDoor(BlockTypeEnum blockType)
		{
			return blockType == BlockTypeEnum.NormalUpperDoorClosed || blockType == BlockTypeEnum.NormalUpperDoorOpen || blockType == BlockTypeEnum.StrongUpperDoorClosed || blockType == BlockTypeEnum.StrongUpperDoorOpen;
		}

		internal static bool IsLowerDoor(BlockTypeEnum blockType)
		{
			return blockType == BlockTypeEnum.NormalLowerDoorOpenX || blockType == BlockTypeEnum.NormalLowerDoorOpenZ || blockType == BlockTypeEnum.NormalLowerDoorClosedX || blockType == BlockTypeEnum.NormalLowerDoorClosedZ || blockType == BlockTypeEnum.StrongLowerDoorOpenX || blockType == BlockTypeEnum.StrongLowerDoorOpenZ || blockType == BlockTypeEnum.StrongLowerDoorClosedX || blockType == BlockTypeEnum.StrongLowerDoorClosedZ || blockType == BlockTypeEnum.StrongLowerDoor || blockType == BlockTypeEnum.NormalLowerDoor;
		}

		public static bool IsStructure(BlockTypeEnum blockType)
		{
			return blockType == BlockTypeEnum.SpawnPointBasic || blockType == BlockTypeEnum.SpawnPointBuilder || blockType == BlockTypeEnum.SpawnPointExplorer || blockType == BlockTypeEnum.SpawnPointCombat || blockType == BlockTypeEnum.TeleportStation || blockType == BlockTypeEnum.GlassIron || blockType == BlockTypeEnum.GlassStrong || blockType == BlockTypeEnum.GlassMystery || blockType == BlockTypeEnum.GlassBasic;
		}

		public bool Opaque
		{
			get
			{
				return this.LightTransmission == 0;
			}
		}

		public bool Clear
		{
			get
			{
				return this.LightTransmission == 16;
			}
		}

		public int this[BlockFace i]
		{
			get
			{
				return this.TileIndices[(int)(i % BlockFace.NUM_FACES)];
			}
		}

		public int this[int i]
		{
			get
			{
				return this.TileIndices[i % 6];
			}
		}

		public int TransmitLight(int inlight)
		{
			return Math.Max(0, (inlight - 1) * this.LightTransmission >> 4);
		}

		private BlockType(BlockTypeEnum type, string name, int hardness, float lightTransmission, float selfIllumination, float damageTransmision, bool isItemEntity, bool xlucent, bool interior, bool alphaInTexture, bool hasSpecular, bool blockPlayer, bool canBeTouched, bool canBuildOn, bool canBeDug, bool fullBright, bool spawnEntity, bool bounceLasers, float bounceRestitution, int texIndexPosY, int texIndexPosX, int texIndexPosZ, int texIndexNegY, int texIndexNegX, int texIndexNegZ)
		{
			this.Name = name;
			this._type = type;
			this.Hardness = hardness;
			this.LightTransmission = (int)Math.Floor((double)(lightTransmission * 16f + 0.5f));
			this.SelfIllumination = (int)Math.Floor((double)(selfIllumination * 15f + 0.5f));
			this.IsItemEntity = isItemEntity;
			this.HasAlpha = alphaInTexture;
			this.NeedsFancyLighting = hasSpecular;
			this.BlockPlayer = blockPlayer;
			this.CanBeDug = canBeDug;
			this.CanBeTouched = canBeTouched;
			this.DrawFullBright = fullBright;
			this.LightAsTranslucent = xlucent;
			this.InteriorFaces = interior;
			this.SpawnEntity = spawnEntity;
			this.CanBuildOn = canBuildOn;
			this.DamageTransmision = damageTransmision;
			this.BouncesLasers = bounceLasers;
			this.BounceRestitution = bounceRestitution;
			this.AllowSlopes = blockPlayer;
			this.Facing = BlockFace.NUM_FACES;
			this.ParentBlockType = type;
			this.TileIndices = new int[] { texIndexPosX, texIndexNegZ, texIndexNegX, texIndexPosZ, texIndexPosY, texIndexNegY };
		}

		private BlockType(BlockTypeEnum type, string name, int hardness, float lightTransmission, float selfIllumination, float damageTransmision, bool isItemEntity, bool xlucent, bool interior, bool alphaInTexture, bool hasSpecular, bool blockPlayer, bool canBeTouched, bool canBuildOn, bool canBeDug, bool fullBright, bool spawnEntity, bool bounceLasers, float bounceRestitution, int texIndexPosY)
		{
			this.Name = name;
			this._type = type;
			this.Hardness = hardness;
			this.LightTransmission = (int)Math.Floor((double)(lightTransmission * 16f + 0.5f));
			this.SelfIllumination = (int)Math.Floor((double)(selfIllumination * 15f + 0.5f));
			this.IsItemEntity = isItemEntity;
			this.HasAlpha = alphaInTexture;
			this.NeedsFancyLighting = hasSpecular;
			this.BlockPlayer = blockPlayer;
			this.CanBeDug = canBeDug;
			this.CanBeTouched = canBeTouched;
			this.DrawFullBright = fullBright;
			this.LightAsTranslucent = xlucent;
			this.InteriorFaces = interior;
			this.SpawnEntity = spawnEntity;
			this.CanBuildOn = canBuildOn;
			this.DamageTransmision = damageTransmision;
			this.BouncesLasers = bounceLasers;
			this.BounceRestitution = bounceRestitution;
			this.AllowSlopes = blockPlayer;
			this.Facing = BlockFace.NUM_FACES;
			this.ParentBlockType = type;
			this.TileIndices = new int[] { texIndexPosY, texIndexPosY, texIndexPosY, texIndexPosY, texIndexPosY, texIndexPosY };
		}

		private BlockType(BlockTypeEnum type, string name, int hardness, float lightTransmission, float selfIllumination, float damageTransmision, bool isItemEntity, bool xlucent, bool interior, bool alphaInTexture, bool hasSpecular, bool blockPlayer, bool canBeTouched, bool canBuildOn, bool canBeDug, bool fullBright, bool spawnEntity, bool bounceLasers, float bounceRestitution, BlockFace facing, BlockTypeEnum parentBlock)
		{
			this.Name = name;
			this._type = type;
			this.Hardness = hardness;
			this.LightTransmission = (int)Math.Floor((double)(lightTransmission * 16f + 0.5f));
			this.SelfIllumination = (int)Math.Floor((double)(selfIllumination * 15f + 0.5f));
			this.IsItemEntity = isItemEntity;
			this.HasAlpha = alphaInTexture;
			this.NeedsFancyLighting = hasSpecular;
			this.BlockPlayer = blockPlayer;
			this.CanBeDug = canBeDug;
			this.CanBeTouched = canBeTouched;
			this.DrawFullBright = fullBright;
			this.LightAsTranslucent = xlucent;
			this.InteriorFaces = interior;
			this.SpawnEntity = spawnEntity;
			this.CanBuildOn = canBuildOn;
			this.DamageTransmision = damageTransmision;
			this.BouncesLasers = bounceLasers;
			this.BounceRestitution = bounceRestitution;
			this.AllowSlopes = blockPlayer;
			this.Facing = facing;
			this.ParentBlockType = parentBlock;
			this.TileIndices = new int[] { -1, -1, -1, -1, -1, -1 };
		}

		private const float cDeadBounce = 0.1f;

		private const float cSoftBounce = 0.4f;

		private const float cHardBounce = 0.6f;

		private static readonly BlockType[] _blockTypes = new BlockType[]
		{
			new BlockType(BlockTypeEnum.Empty, Strings.Air, 5, 1f, 0f, 1f, false, false, false, true, false, false, false, false, false, false, false, false, 0.6f, -1),
			new BlockType(BlockTypeEnum.Dirt, Strings.Dirt, 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._00),
			new BlockType(BlockTypeEnum.Grass, Strings.Grass, 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._02, Octal._01, Octal._01, Octal._00, Octal._01, Octal._01),
			new BlockType(BlockTypeEnum.Sand, Strings.Sand, 1, 0f, 0f, 0.7f, false, false, false, false, false, true, true, true, true, false, false, false, 0.1f, Octal._03),
			new BlockType(BlockTypeEnum.Lantern, Strings.Lantern, 2, 0f, 1f, 1f, false, false, false, false, true, true, true, true, true, false, true, false, 0.6f, Octal._04),
			new BlockType(BlockTypeEnum.FixedLantern, Strings.Lantern, 5, 0f, 1f, 1f, false, false, false, false, true, true, true, true, false, false, false, false, 0.6f, Octal._04),
			new BlockType(BlockTypeEnum.Rock, Strings.Rock, 3, 0f, 0f, 0.5f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._05),
			new BlockType(BlockTypeEnum.GoldOre, Strings.Gold_Ore, 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._06),
			new BlockType(BlockTypeEnum.IronOre, Strings.Ore, 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._07),
			new BlockType(BlockTypeEnum.CopperOre, Strings.Copper_Ore, 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._10),
			new BlockType(BlockTypeEnum.CoalOre, Strings.Coal, 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._11),
			new BlockType(BlockTypeEnum.DiamondOre, Strings.Diamonds, 3, 0f, 0f, 0.4f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._12),
			new BlockType(BlockTypeEnum.SurfaceLava, Strings.Lava, 3, 0f, 1f, 1f, false, false, false, false, false, true, true, true, true, true, false, false, 0.6f, Octal._13),
			new BlockType(BlockTypeEnum.DeepLava, Strings.Lava, 3, 0f, 1f, 1f, false, false, false, false, false, false, true, true, true, true, false, false, 0.1f, Octal._13),
			new BlockType(BlockTypeEnum.Bedrock, Strings.Bedrock, 5, 0f, 0f, 0.3f, false, false, false, false, false, true, true, true, false, false, false, true, 0.6f, Octal._14),
			new BlockType(BlockTypeEnum.Snow, Strings.Snow, 1, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._15),
			new BlockType(BlockTypeEnum.Ice, Strings.Ice, 2, 0.9f, 0f, 0.9f, false, true, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._16),
			new BlockType(BlockTypeEnum.Log, Strings.Log, 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._20, Octal._17, Octal._17, Octal._20, Octal._17, Octal._17),
			new BlockType(BlockTypeEnum.Leaves, Strings.Leaves, 1, 0.4f, 0f, 1f, false, true, true, true, false, false, true, true, true, false, false, false, 0.1f, Octal._21),
			new BlockType(BlockTypeEnum.Wood, Strings.Wood, 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._22),
			new BlockType(BlockTypeEnum.BloodStone, Strings.BloodStone, 4, 0f, 0f, 0.2f, false, false, false, false, false, true, true, true, true, false, false, true, 0.6f, Octal._23),
			new BlockType(BlockTypeEnum.SpaceRock, Strings.Space_Rock, 4, 0f, 0f, 0.1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._24),
			new BlockType(BlockTypeEnum.IronWall, Strings.Iron_Wall, 4, 0f, 0f, 0.3f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._25),
			new BlockType(BlockTypeEnum.CopperWall, Strings.Copper_Wall, 4, 0f, 0f, 0.3f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._26),
			new BlockType(BlockTypeEnum.GoldenWall, Strings.Golden_Wall, 4, 0f, 0f, 0.3f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._27),
			new BlockType(BlockTypeEnum.DiamondWall, Strings.Diamond_Wall, 4, 0f, 0f, 0.2f, false, false, false, false, true, true, true, true, true, false, false, true, 0.6f, Octal._30),
			new BlockType(BlockTypeEnum.Torch, Strings.Torch, 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, -1),
			new BlockType(BlockTypeEnum.TorchPOSX, Strings.Torch, 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.POSX, BlockTypeEnum.Torch),
			new BlockType(BlockTypeEnum.TorchNEGZ, Strings.Torch, 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.NEGZ, BlockTypeEnum.Torch),
			new BlockType(BlockTypeEnum.TorchNEGX, Strings.Torch, 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.NEGX, BlockTypeEnum.Torch),
			new BlockType(BlockTypeEnum.TorchPOSZ, Strings.Torch, 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.POSZ, BlockTypeEnum.Torch),
			new BlockType(BlockTypeEnum.TorchPOSY, Strings.Torch, 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.POSY, BlockTypeEnum.Torch),
			new BlockType(BlockTypeEnum.TorchNEGY, Strings.Torch, 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.NEGY, BlockTypeEnum.Torch),
			new BlockType(BlockTypeEnum.Crate, Strings.Crate, 2, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._31),
			new BlockType(BlockTypeEnum.NormalLowerDoorClosedZ, Strings.Door, 1, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.NormalLowerDoor),
			new BlockType(BlockTypeEnum.NormalLowerDoorClosedX, Strings.Door, 1, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.NormalLowerDoor),
			new BlockType(BlockTypeEnum.NormalLowerDoor, Strings.Door, 1, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, -1),
			new BlockType(BlockTypeEnum.NormalUpperDoorClosed, Strings.Door, 1, 0.5f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, false, false, 0.4f, BlockFace.NUM_FACES, BlockTypeEnum.NormalLowerDoor),
			new BlockType(BlockTypeEnum.NormalLowerDoorOpenZ, Strings.Door, 1, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.NormalLowerDoor),
			new BlockType(BlockTypeEnum.NormalLowerDoorOpenX, Strings.Door, 1, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.NormalLowerDoor),
			new BlockType(BlockTypeEnum.NormalUpperDoorOpen, Strings.Door, 1, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, false, false, 0.4f, BlockFace.NUM_FACES, BlockTypeEnum.NormalLowerDoor),
			new BlockType(BlockTypeEnum.TNT, Strings.TNT, 1, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._34, Octal._33, Octal._33, Octal._34, Octal._33, Octal._33),
			new BlockType(BlockTypeEnum.C4, Strings.C4, 1, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._36, Octal._35, Octal._35, Octal._36, Octal._35, Octal._35),
			new BlockType(BlockTypeEnum.Slime, Strings.Space_Goo, 4, 0f, 1f, 1f, false, false, false, false, false, true, true, true, true, true, false, false, 0.1f, Octal._32),
			new BlockType(BlockTypeEnum.SpaceRockInventory, Strings.Space_Rock, 4, 0f, 0f, 0.1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._24),
			new BlockType(BlockTypeEnum.GlassBasic, Strings.Window_Sticks, 2, 0.75f, 0f, 0.2f, false, true, false, true, false, true, true, true, true, false, false, true, 0.6f, Octal._40),
			new BlockType(BlockTypeEnum.GlassIron, Strings.Window_Iron, 3, 0.75f, 0f, 0.2f, false, true, true, true, false, true, true, true, true, false, false, true, 0.6f, Octal._41),
			new BlockType(BlockTypeEnum.GlassStrong, Strings.Window_Bulletproof, 4, 0.95f, 0f, 0.2f, false, true, true, true, false, true, true, true, true, false, false, true, 0.6f, Octal._37),
			new BlockType(BlockTypeEnum.GlassMystery, Strings.Window_Clear, 4, 0.95f, 0f, 0.2f, false, true, false, true, false, true, true, true, true, false, false, true, 0.6f, Octal._37),
			new BlockType(BlockTypeEnum.CrateStone, Strings.Crate_Stone, 2, 0f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._42),
			new BlockType(BlockTypeEnum.CrateCopper, Strings.Crate_Copper, 2, 0f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._43),
			new BlockType(BlockTypeEnum.CrateIron, Strings.Crate_Iron, 3, 0f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._44),
			new BlockType(BlockTypeEnum.CrateGold, Strings.Crate_Gold, 3, 0f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._45),
			new BlockType(BlockTypeEnum.CrateDiamond, Strings.Crate_Diamond, 4, 0f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._46),
			new BlockType(BlockTypeEnum.CrateBloodstone, Strings.Crate_Bloodstone, 4, 0f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._47),
			new BlockType(BlockTypeEnum.CrateSafe, Strings.Crate_Stone, 5, 0f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._42),
			new BlockType(BlockTypeEnum.SpawnPointBasic, Strings.Spawn_Basic, 5, 0f, 0.7f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._50),
			new BlockType(BlockTypeEnum.SpawnPointBuilder, Strings.Spawn_Basic, 5, 0.7f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._50),
			new BlockType(BlockTypeEnum.SpawnPointCombat, Strings.Spawn_Basic, 5, 0.7f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._50),
			new BlockType(BlockTypeEnum.SpawnPointExplorer, Strings.Spawn_Basic, 5, 0.7f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._50),
			new BlockType(BlockTypeEnum.StrongLowerDoorClosedZ, Strings.Strong_Door, 2, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.StrongLowerDoor),
			new BlockType(BlockTypeEnum.StrongLowerDoorClosedX, Strings.Strong_Door, 2, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.StrongLowerDoor),
			new BlockType(BlockTypeEnum.StrongLowerDoor, Strings.Strong_Door, 2, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, -1),
			new BlockType(BlockTypeEnum.StrongUpperDoorClosed, Strings.Strong_Door, 2, 0.5f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, false, false, 0.4f, BlockFace.NUM_FACES, BlockTypeEnum.StrongLowerDoor),
			new BlockType(BlockTypeEnum.StrongLowerDoorOpenZ, Strings.Strong_Door, 2, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.StrongLowerDoor),
			new BlockType(BlockTypeEnum.StrongLowerDoorOpenX, Strings.Strong_Door, 2, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.StrongLowerDoor),
			new BlockType(BlockTypeEnum.StrongUpperDoorOpen, Strings.Strong_Door, 2, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, false, false, 0.4f, BlockFace.NUM_FACES, BlockTypeEnum.StrongLowerDoor),
			new BlockType(BlockTypeEnum.LanternFancy, Strings.Lantern, 2, 0f, 1f, 1f, false, false, false, false, true, true, true, true, true, false, true, false, 0.6f, Octal._51),
			new BlockType(BlockTypeEnum.TurretBlock, Strings.Space_Goo, 4, 0f, 1f, 1f, false, false, false, false, false, true, true, true, true, true, false, false, 0.1f, Octal._32),
			new BlockType(BlockTypeEnum.LootBlock, Strings.Loot_Block, 4, 0f, 0.5f, 1f, false, false, false, false, true, true, true, true, true, true, false, false, 0.1f, Octal._52),
			new BlockType(BlockTypeEnum.LuckyLootBlock, Strings.Lucky_Loot_Block, 4, 0f, 0.5f, 1f, false, false, false, false, true, true, true, true, true, true, false, false, 0.1f, Octal._53),
			new BlockType(BlockTypeEnum.BombBlock, Strings.Space_Goo, 4, 0f, 1f, 1f, false, false, false, false, false, true, true, true, true, true, false, false, 0.1f, Octal._32),
			new BlockType(BlockTypeEnum.EnemySpawnOn, Strings.Active_Spawner, 5, 0f, 0.4f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._56),
			new BlockType(BlockTypeEnum.EnemySpawnOff, Strings.Monster_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._56),
			new BlockType(BlockTypeEnum.EnemySpawnRareOn, Strings.Active_Spawner, 5, 0f, 0.4f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._55),
			new BlockType(BlockTypeEnum.EnemySpawnRareOff, Strings.Monster_Spawner_2, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._55),
			new BlockType(BlockTypeEnum.EnemySpawnAltar, Strings.Door, 5, 0f, 0.2f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._55),
			new BlockType(BlockTypeEnum.TeleportStation, Strings.Teleport_Station, 1, 0.7f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._54),
			new BlockType(BlockTypeEnum.CraftingStation, Strings.Door, 5, 0.7f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._50),
			new BlockType(BlockTypeEnum.HellForge, Strings.Door, 5, 0.7f, 0f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._50),
			new BlockType(BlockTypeEnum.AlienSpawnOn, Strings.Active_Spawner, 5, 0f, 0.4f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._60),
			new BlockType(BlockTypeEnum.AlienSpawnOff, Strings.Alien_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._60),
			new BlockType(BlockTypeEnum.HellSpawnOn, Strings.Active_Spawner, 5, 0f, 0.4f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._61),
			new BlockType(BlockTypeEnum.HellSpawnOff, Strings.Hell_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._61),
			new BlockType(BlockTypeEnum.BossSpawnOn, Strings.Active_Spawner, 5, 0f, 0.4f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._62),
			new BlockType(BlockTypeEnum.BossSpawnOff, Strings.Boss_Spawner, 5, 0f, 0.9f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._62),
			new BlockType(BlockTypeEnum.EnemySpawnDim, Strings.Active_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._56),
			new BlockType(BlockTypeEnum.EnemySpawnRareDim, Strings.Active_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._55),
			new BlockType(BlockTypeEnum.AlienSpawnDim, Strings.Active_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._60),
			new BlockType(BlockTypeEnum.HellSpawnDim, Strings.Active_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._61),
			new BlockType(BlockTypeEnum.BossSpawnDim, Strings.Active_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._62),
			new BlockType(BlockTypeEnum.AlienHordeOn, Strings.Active_Spawner, 5, 0f, 0.4f, 1f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._60),
			new BlockType(BlockTypeEnum.AlienHordeOff, Strings.Alien_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._60),
			new BlockType(BlockTypeEnum.AlienHordeDim, Strings.Active_Spawner, 5, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._62),
			new BlockType(BlockTypeEnum.NumberOfBlocks, Strings.Air, 5, 1f, 0f, 1f, false, false, false, true, false, true, false, false, false, false, false, false, 0.6f, -1)
		};

		public BlockTypeEnum _type;

		public string Name;

		public int[] TileIndices;

		public int LightTransmission;

		public int SelfIllumination;

		public int DamageMask;

		public BlockFace Facing;

		public BlockTypeEnum ParentBlockType;

		public bool IsItemEntity;

		public bool BlockPlayer;

		public bool NeedsFancyLighting;

		public bool HasAlpha;

		public bool CanBeDug;

		public bool CanBeTouched;

		public bool CanBuildOn;

		public bool DrawFullBright;

		public bool LightAsTranslucent;

		public bool InteriorFaces;

		public bool SpawnEntity;

		public float DamageTransmision;

		public int Hardness;

		public bool BouncesLasers;

		public float BounceRestitution;

		public bool AllowSlopes;
	}
}
