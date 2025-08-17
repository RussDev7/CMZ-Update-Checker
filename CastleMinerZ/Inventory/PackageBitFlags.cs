using System;

namespace DNA.CastleMinerZ.Inventory
{
	[Flags]
	public enum PackageBitFlags
	{
		None = 0,
		Common = 1,
		Normal = 2,
		Rare = 4,
		Epic = 8,
		Legendary = 16,
		Alien = 32,
		Hell = 64,
		Desert = 128,
		Forest = 256,
		Moutain = 512,
		Volcano = 1024,
		Underground = 2048,
		SkyIsland = 4096,
		Dragon = 8192,
		Champion = 16384,
		Boss = 32768,
		UndeadDragon = 65536,
		CurrentBiome = 131072
	}
}
