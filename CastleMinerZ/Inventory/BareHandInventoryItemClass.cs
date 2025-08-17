using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;

namespace DNA.CastleMinerZ.Inventory
{
	public class BareHandInventoryItemClass : InventoryItem.InventoryItemClass
	{
		public BareHandInventoryItemClass()
			: base(InventoryItemIDs.BareHands, "Bare Hands", " ", 1, TimeSpan.FromSeconds(0.5))
		{
			this._playerMode = PlayerMode.Fist;
			this.EnemyDamage = 0.025f;
			this.EnemyDamageType = DamageType.BLUNT;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			return new Entity();
		}
	}
}
