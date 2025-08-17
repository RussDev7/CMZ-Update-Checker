using System;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;

namespace DNA.CastleMinerZ.Inventory
{
	public abstract class ItemBlockInventoryItemClass : BlockInventoryItemClass
	{
		public ItemBlockInventoryItemClass(InventoryItemIDs id, BlockTypeEnum blockType, string description)
			: base(id, blockType, description, 0.025f)
		{
		}

		public abstract Entity CreateWorldEntity(bool attachedToLocalPlayer, BlockTypeEnum blockType, DoorEntity.ModelNameEnum modelName);
	}
}
