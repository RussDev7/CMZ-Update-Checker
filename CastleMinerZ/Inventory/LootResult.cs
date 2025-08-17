using System;
using DNA.CastleMinerZ.AI;

namespace DNA.CastleMinerZ.Inventory
{
	public struct LootResult
	{
		public EnemyTypeEnum spawnID;

		public InventoryItemIDs lootItemID;

		public int count;

		public int value;
	}
}
